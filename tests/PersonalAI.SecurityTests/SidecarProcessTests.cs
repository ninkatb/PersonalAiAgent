using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace PersonalAI.SecurityTests;

public sealed class SidecarProcessTests
{
    private static Process Start()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "PersonalAI.sln"))) root = root.Parent;
        Assert.NotNull(root);
        var dll = Path.Combine(root.FullName, "src", "PersonalAI.Api", "bin", "Debug", "net10.0", "PersonalAI.Api.dll");
        var info = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
        };
        info.ArgumentList.Add(dll);
        // Hostile environment must not configure LAN or alternate listeners.
        info.Environment["ASPNETCORE_URLS"] = "http://0.0.0.0:5043";
        info.Environment["Kestrel__Endpoints__Public__Url"] = "http://0.0.0.0:5044";
        return Process.Start(info)!;
    }

    [Theory]
    [InlineData("{}\n")]
    [InlineData("incomplete")]
    [InlineData("oversize")]
    public async Task InvalidPrivateChannelExitsWithoutReadinessOrSecretEcho(string input)
    {
        using var process = Start();
        try
        {
            await process.StandardInput.WriteAsync(input == "oversize" ? new string('a', 1025) : input);
            process.StandardInput.Close();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(15));
            Assert.Equal(78, process.ExitCode);
            Assert.Equal("", await process.StandardOutput.ReadToEndAsync());
            Assert.Equal("STARTUP_FAILED", (await process.StandardError.ReadToEndAsync()).Trim());
        }
        finally { if (!process.HasExited) process.Kill(true); }
    }

    [Fact]
    public async Task SilentPrivateChannelTimesOutWithoutOpeningListener()
    {
        using var process = Start();
        try
        {
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(16));
            Assert.Equal(78, process.ExitCode);
            Assert.Equal("", await process.StandardOutput.ReadToEndAsync());
        }
        finally { if (!process.HasExited) process.Kill(true); }
    }

    [Fact]
    public async Task PrivateHandshakeIgnoresPublicEnvironmentAndParentEofStopsSidecar()
    {
        using var process = Start();
        try
        {
            var token = new string('a', 64);
            var sessionId = new string('b', 32);
            var origin = $"http://session-{sessionId}.localhost";
            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(new { protocol = 1, token, sessionId, origin }));
            await process.StandardInput.FlushAsync();
            var frame = await process.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(15));
            Assert.NotNull(frame);
            using var ready = JsonDocument.Parse(frame);
            Assert.Equal(3, ready.RootElement.EnumerateObject().Count());
            Assert.Equal(process.Id, ready.RootElement.GetProperty("pid").GetInt32());
            Assert.Equal(1, ready.RootElement.GetProperty("protocol").GetInt32());
            var port = ready.RootElement.GetProperty("port").GetInt32();
            Assert.InRange(port, 1, 65535);
            using var client = new HttpClient(new HttpClientHandler { UseProxy = false });
            using var request = new HttpRequestMessage(HttpMethod.Get, $"http://127.0.0.1:{port}/health");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Authorization", "Bearer " + token);
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            process.StandardInput.Close();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(0, process.ExitCode);
            Assert.Equal("", await process.StandardOutput.ReadToEndAsync());
            Assert.DoesNotContain(token, await process.StandardError.ReadToEndAsync());
        }
        finally { if (!process.HasExited) process.Kill(true); }
    }
}
