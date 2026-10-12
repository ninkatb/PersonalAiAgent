using System.Text.Json;

namespace PersonalAI.Api;

// Dynamic messages, scopes and exceptions are never persisted. Only bounded
// platform metadata is retained, even when a caller accidentally logs a secret.
public sealed class SafeLogProvider(TextWriter writer) : ILoggerProvider
{
    private readonly TextWriter output = writer;
    private readonly object gate = new();
    public ILogger CreateLogger(string categoryName) => new SafeLogger(this);
    public void Dispose() { }

    private sealed class SafeLogger(SafeLogProvider owner) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;
            lock (owner.gate)
                owner.output.WriteLine(JsonSerializer.Serialize(new
                {
                    level = logLevel.ToString(), eventId = eventId.Id, message = "[REDACTED]"
                }));
        }
    }
}
