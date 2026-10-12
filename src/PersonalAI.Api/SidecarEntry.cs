using System.Text.Json;

namespace PersonalAI.Api;

public static class SidecarEntry
{
    public static async Task<int> RunAsync()
    {
        if (!Console.IsInputRedirected || !Console.IsOutputRedirected) return 78;
        try
        {
            using var input = Console.OpenStandardInput();
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            // Console's Windows pipe read may ignore cancellation. Run it on a
            // background worker and bound startup independently of that read.
            var startup = await Task.Run(() => ReadFrame(input, deadline.Token))
                .WaitAsync(deadline.Token);
            using var session = SessionConfiguration.Parse(startup);
            Array.Clear(startup);
            await using var app = ApiHost.Create(session, Console.Error);
            // Observe EOF before listener startup, including a parent interrupted
            // after sending a valid handshake but before receiving readiness.
            _ = WatchParent(input, app, session);
            await app.StartAsync(deadline.Token);
            session.Revoked.ThrowIfCancellationRequested();
            await Console.Out.WriteLineAsync(JsonSerializer.Serialize(new
            {
                protocol = 1, pid = Environment.ProcessId, port = ApiHost.GetPort(app)
            }));
            await Console.Out.FlushAsync();
            // The parent holds stdin open. EOF (or unexpected input) stops this session.
            await app.WaitForShutdownAsync();
            return 0;
        }
        catch
        {
            Console.Error.WriteLine("STARTUP_FAILED");
            return 78;
        }
    }

    public static async Task<byte[]> ReadFrame(Stream input, CancellationToken cancellationToken)
    {
        var frame = new List<byte>();
        var single = new byte[1];
        while (frame.Count < 1024)
        {
            if (await input.ReadAsync(single, cancellationToken) != 1)
                throw new InvalidDataException("STARTUP_INCOMPLETE");
            if (single[0] == '\n') return frame.ToArray();
            frame.Add(single[0]);
        }
        throw new InvalidDataException("STARTUP_TOO_LARGE");
    }

    private static async Task WatchParent(Stream input, WebApplication app, SessionConfiguration session)
    {
        try { _ = await input.ReadAsync(new byte[1]); }
        catch (Exception error) when (error is IOException or ObjectDisposedException) { }
        finally
        {
            try { session.Revoke(); }
            finally { app.Lifetime.StopApplication(); }
        }
    }
}
