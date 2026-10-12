using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PersonalAI.Api;

namespace PersonalAI.SecurityTests;

public sealed class LocalTransportTests : IAsyncLifetime
{
    private const string Token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string SessionId = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    private const string Origin = "http://session-" + SessionId + ".localhost";
    private readonly StringWriter logs = new();
    private WebApplication app = null!;
    private HttpClient client = null!;
    private int port;

    public async Task InitializeAsync()
    {
        app = ApiHost.Create(ParseStartup(), logs, testHost =>
        {
            testHost.MapPost("/test/validate", async (HttpContext context) =>
            {
                _ = await RequestValidation.ReadAsync<TestDto>(context, dto => dto.Count is >= 1 and <= 10);
                await context.Response.WriteAsJsonAsync(new { status = "ok" });
            });
            testHost.MapGet("/test/failure", (HttpContext context) =>
                throw new InvalidOperationException("SYNTHETIC_PRIVATE_EXCEPTION"));
        });
        await app.StartAsync();
        port = ApiHost.GetPort(app);
        client = new HttpClient(new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false })
        { BaseAddress = new Uri($"http://127.0.0.1:{port}"), Timeout = TimeSpan.FromSeconds(3) };
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.StopAsync();
        await app.DisposeAsync();
    }

    private static SessionConfiguration ParseStartup(string? json = null) => SessionConfiguration.Parse(
        Encoding.UTF8.GetBytes(json ?? JsonSerializer.Serialize(new { protocol = 1, token = Token, sessionId = SessionId, origin = Origin })));

    private HttpRequestMessage Request(string path = "/health", string? token = Token,
        string? origin = Origin, string? host = null, string method = "GET")
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (token is not null) request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + token);
        if (origin is not null) request.Headers.TryAddWithoutValidation("Origin", origin);
        if (host is not null) request.Headers.Host = host;
        return request;
    }

    [Fact]
    public async Task HealthReturnsOnlyHarmlessStatusAndNoCache()
    {
        using var response = await client.SendAsync(Request());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("{\"status\":\"ok\"}", await response.Content.ReadAsStringAsync());
        Assert.True(response.Headers.CacheControl!.NoStore);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("wrong")]
    [InlineData("cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc")]
    public async Task RejectsMissingWrongAndOtherSessionToken(string? token)
    {
        using var response = await client.SendAsync(Request(token: token));
        await AssertProblem(response, HttpStatusCode.Unauthorized, "SESSION_UNAUTHORIZED");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("null")]
    [InlineData("http://127.0.0.1:5173")]
    [InlineData("http://evil.example")]
    [InlineData(Origin + "/")]
    [InlineData(Origin + ".evil.example")]
    public async Task RejectsUntrustedBrowserEvenWithCorrectToken(string? origin)
    {
        using var response = await client.SendAsync(Request(origin: origin));
        await AssertProblem(response, HttpStatusCode.Forbidden, "SESSION_BOUNDARY_INVALID");
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("127.0.0.1:1")]
    [InlineData("evil.example")]
    public async Task RejectsIncorrectHost(string host)
    {
        using var response = await client.SendAsync(Request(host: host));
        await AssertProblem(response, HttpStatusCode.Forbidden, "SESSION_BOUNDARY_INVALID");
    }

    [Fact]
    public async Task DirectUnauthenticatedRequestAndUnknownRoutesHaveNoException()
    {
        using var direct = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.Forbidden, direct.StatusCode);
        using var unknown = await client.SendAsync(Request("/unknown", token: null));
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        using var debug = await client.SendAsync(Request("/debug"));
        await AssertProblem(debug, HttpStatusCode.NotFound, "REQUEST_REJECTED");
    }

    [Theory]
    [InlineData(Origin, "GET", "authorization", "/health", 204)]
    [InlineData(Origin, "POST", "authorization", "/health", 403)]
    [InlineData(Origin, "GET", "authorization, x-extra", "/health", 403)]
    [InlineData(Origin, "GET", "authorization", "/unknown", 403)]
    [InlineData(Origin, "GET", "authorization", "/health?secret=SYNTHETIC", 403)]
    [InlineData("http://evil.example", "GET", "authorization", "/health", 403)]
    public async Task PreflightIsLimitedAndReturnsNoProtectedData(string origin, string method, string headers, string path, int status)
    {
        using var request = Request(path, token: null, origin: origin, method: "OPTIONS");
        request.Headers.Add("Access-Control-Request-Method", method);
        request.Headers.Add("Access-Control-Request-Headers", headers);
        using var response = await client.SendAsync(request);
        Assert.Equal(status, (int)response.StatusCode);
        if (status == 204)
        {
            Assert.Equal("", await response.Content.ReadAsStringAsync());
            Assert.Equal(Origin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        }
        else Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Theory]
    [InlineData("{\"count\":0}")]
    [InlineData("{\"count\":11}")]
    [InlineData("{\"count\":\"SYNTHETIC_PRIVATE_BODY\"}")]
    [InlineData("{\"count\":1,\"secret\":\"SYNTHETIC_PRIVATE_BODY\"}")]
    [InlineData("not-json-SYNTHETIC_PRIVATE_BODY")]
    [InlineData("null")]
    public async Task ValidationUsesStableProblemWithoutRequestContent(string body)
    {
        using var request = Request("/test/validate", method: "POST");
        request.Content = new StringContent(body, Encoding.UTF8, "application/json");
        using var response = await client.SendAsync(request);
        await AssertProblem(response, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        Assert.DoesNotContain("SYNTHETIC", logs.ToString());
    }

    [Fact]
    public async Task ValidDtoAndDuplicateCredentialHeadersAreHandled()
    {
        using var request = Request("/test/validate", method: "POST");
        request.Content = new StringContent("{\"count\":1}", Encoding.UTF8, "application/json");
        using var valid = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        using var duplicate = Request();
        duplicate.Headers.TryAddWithoutValidation("Authorization", "Bearer " + Token);
        using var rejected = await client.SendAsync(duplicate);
        Assert.Equal(HttpStatusCode.Unauthorized, rejected.StatusCode);
    }

    [Fact]
    public async Task UnexpectedExceptionAndStructuredLogsDoNotExposeSecrets()
    {
        using var response = await client.SendAsync(Request("/test/failure"));
        await AssertProblem(response, HttpStatusCode.InternalServerError, "INTERNAL_ERROR");
        using var provider = new SafeLogProvider(logs);
        var logger = provider.CreateLogger("SYNTHETIC_SECRET_CATEGORY");
        using var scope = logger.BeginScope("SYNTHETIC_SECRET_SCOPE");
        logger.LogError(new EventId(77, "SYNTHETIC_EVENT"), new Exception("SYNTHETIC_EXCEPTION"),
            "credential {Credential}", "SYNTHETIC_API_KEY");
        Assert.DoesNotContain("SYNTHETIC", logs.ToString());
        Assert.Contains("REDACTED", logs.ToString());
        Assert.Contains("\"eventId\":77", logs.ToString());
    }

    [Fact]
    public async Task ListenerIsOnlyIpv4LoopbackAndRejectsMachineNetworkAddress()
    {
        var listeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Where(x => x.Port == port);
        Assert.All(listeners, endpoint => Assert.Equal(IPAddress.Loopback, endpoint.Address));
        Assert.NotEmpty(listeners);
        var addresses = NetworkInterface.GetAllNetworkInterfaces().SelectMany(x => x.GetIPProperties().UnicastAddresses)
            .Select(x => x.Address).Where(x => x.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(x));
        foreach (var address in addresses)
        {
            using var socket = new TcpClient();
            await Assert.ThrowsAnyAsync<SocketException>(async () =>
                await socket.ConnectAsync(address, port).WaitAsync(TimeSpan.FromSeconds(3)));
        }
    }

    [Fact]
    public void StartupDtoAndAuthenticatorFailClosed()
    {
        var session = ParseStartup();
        Assert.True(session.Verify(Encoding.ASCII.GetBytes(Token)));
        Assert.False(session.Verify(Encoding.ASCII.GetBytes("wrong")));
        foreach (var json in new[] { "{}", "null", "{\"protocol\":2}",
            JsonSerializer.Serialize(new { protocol = 1, token = Token, sessionId = SessionId, origin = "http://evil.example" }),
            JsonSerializer.Serialize(new { protocol = 1, token = Token, sessionId = SessionId, origin = Origin, extra = "SYNTHETIC" }) })
            Assert.ThrowsAny<Exception>(() => ParseStartup(json));
    }

    [Fact]
    public void BoundaryAndCredentialUnitMatrixRejectsDuplicateAndMalformedHeaders()
    {
        var session = ParseStartup();
        var context = new DefaultHttpContext();
        context.Connection.LocalPort = 12345;
        context.Request.Headers.Host = "127.0.0.1:12345";
        context.Request.Headers.Origin = Origin;
        Assert.True(session.MatchesBoundary(context.Request));
        context.Request.Headers.Origin = new[] { Origin, Origin };
        Assert.False(session.MatchesBoundary(context.Request));
        context.Request.Headers.Origin = Origin;
        context.Request.Headers.Host = new[] { "127.0.0.1:12345", "127.0.0.1:12345" };
        Assert.False(session.MatchesBoundary(context.Request));
        Assert.False(session.Authorizes(context.Request));
        context.Request.Headers.Authorization = "Basic  " + Token;
        Assert.False(session.Authorizes(context.Request));
        context.Request.Headers.Authorization = "Bearer " + Token;
        Assert.True(session.Authorizes(context.Request));
    }

    [Fact]
    public async Task StartupFrameRejectsEofOversizeAndCancellation()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => SidecarEntry.ReadFrame(new MemoryStream([1]), default));
        await Assert.ThrowsAsync<InvalidDataException>(() => SidecarEntry.ReadFrame(new MemoryStream(new byte[1024]), default));
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => SidecarEntry.ReadFrame(new MemoryStream([1]), cancelled.Token));
        Assert.Equal("{}", Encoding.UTF8.GetString(await SidecarEntry.ReadFrame(new MemoryStream("{}\n"u8.ToArray()), default)));
    }

    private static async Task AssertProblem(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal(code, json.RootElement.GetProperty("errorCode").GetString());
        Assert.Empty(json.RootElement.GetProperty("parameters").EnumerateObject());
        Assert.True(Guid.TryParseExact(json.RootElement.GetProperty("correlationId").GetString(), "N", out _));
        Assert.Equal((int)status >= 500, json.RootElement.GetProperty("retryable").GetBoolean());
        Assert.DoesNotContain("SYNTHETIC", body);
        Assert.DoesNotContain("stack", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Token, body);
    }

    private sealed record TestDto(int Count);
}
