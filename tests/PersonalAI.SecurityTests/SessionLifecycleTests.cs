using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PersonalAI.Api;

namespace PersonalAI.SecurityTests;

public sealed class SessionLifecycleTests
{
    private const string Token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string Origin = "http://session-bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb.localhost";

    private static SessionConfiguration Session() => SessionConfiguration.Parse(Encoding.UTF8.GetBytes(
        JsonSerializer.Serialize(new { protocol = 1, token = Token, sessionId = new string('b', 32), origin = Origin })));

    [Fact]
    public void RevocationIsTerminalIdempotentAndInvalidatesCredential()
    {
        using var session = Session();
        Assert.True(session.Verify(Encoding.ASCII.GetBytes(Token)));
        session.Revoke();
        session.Revoke();
        Assert.True(session.Revoked.IsCancellationRequested);
        Assert.False(session.Verify(Encoding.ASCII.GetBytes(Token)));
        Assert.False(session.Verify(new byte[64]));
    }

    [Theory]
    [InlineData("sleep")]
    [InlineData("channel-disconnect")]
    public async Task SyntheticStreamAuthenticatesBeforeOpeningAndCancelsOnRevocation(string reason)
    {
        _ = reason; // Both lifecycle causes invoke the same terminal revocation primitive.
        using var session = Session();
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var opened = 0;
        await using var app = ApiHost.Create(session, TextWriter.Null, host =>
        {
            host.MapGet("/test/stream", async (HttpContext context) =>
            {
                Interlocked.Increment(ref opened);
                entered.SetResult();
                try
                {
                    await context.Response.WriteAsync("SYNTHETIC_FRAME\n", context.RequestAborted);
                    await context.Response.Body.FlushAsync(context.RequestAborted);
                    await Task.Delay(Timeout.InfiniteTimeSpan, context.RequestAborted);
                }
                finally { if (context.RequestAborted.IsCancellationRequested) cancelled.SetResult(); }
            });
        });
        await app.StartAsync();
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        HttpRequestMessage Request(string? token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{ApiHost.GetPort(app)}/test/stream");
            request.Headers.Add("Origin", Origin);
            if (token is not null) request.Headers.Add("Authorization", "Bearer " + token);
            return request;
        }
        try
        {
            foreach (var invalid in new[] { null, "wrong" })
            {
                using var request = Request(invalid);
                using var rejected = await client.SendAsync(request);
                Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
                Assert.Equal(0, Volatile.Read(ref opened));
            }
            using var authorized = Request(Token);
            using var response = await client.SendAsync(authorized, HttpCompletionOption.ResponseHeadersRead);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(3));
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            Assert.Equal("SYNTHETIC_FRAME", await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(3)));
            session.Revoke();
            await cancelled.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Assert.Equal(1, opened);
            try { Assert.Null(await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(3))); }
            catch (IOException) { /* HTTP abort may report a truncated chunked response. */ }
            using var stale = Request(Token);
            using var denied = await client.SendAsync(stale);
            Assert.Equal(HttpStatusCode.Unauthorized, denied.StatusCode);
        }
        finally { await app.StopAsync(); }
    }

    [Fact]
    public async Task ProductionHostHasNoSyntheticStreamingEndpoint()
    {
        using var session = Session();
        await using var app = ApiHost.Create(session, TextWriter.Null);
        await app.StartAsync();
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
        using var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{ApiHost.GetPort(app)}/test/stream");
        request.Headers.Add("Origin", Origin);
        request.Headers.Add("Authorization", "Bearer " + Token);
        try
        {
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally { await app.StopAsync(); }
    }
}
