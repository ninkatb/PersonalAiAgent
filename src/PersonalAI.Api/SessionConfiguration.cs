using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using PersonalAI.Domain;

namespace PersonalAI.Api;

public sealed class SessionConfiguration : IRequestAuthenticator, IDisposable
{
    private readonly byte[] credential;
    private readonly object gate = new();
    private readonly CancellationTokenSource revocation = new();
    private bool revoked;
    public string Origin { get; }
    public CancellationToken Revoked => revocation.Token;

    public void Revoke()
    {
        lock (gate)
        {
            if (revoked) return;
            revoked = true;
            CryptographicOperations.ZeroMemory(credential);
        }
        revocation.Cancel();
    }

    public void Dispose() { Revoke(); revocation.Dispose(); }

    private SessionConfiguration(StartupMessage message)
    {
        credential = Encoding.ASCII.GetBytes(message.Token);
        Origin = message.Origin;
    }

    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        PropertyNameCaseInsensitive = false,
        MaxDepth = 4
    };

    public static SessionConfiguration Parse(ReadOnlySpan<byte> json)
    {
        var message = JsonSerializer.Deserialize<StartupMessage>(json, JsonOptions);
        if (message is null || message.Protocol != 1 || !IsHex(message.Token, 64) ||
            !IsHex(message.SessionId, 32) ||
            message.Origin != $"http://session-{message.SessionId}.localhost")
            throw new InvalidDataException("STARTUP_INVALID");
        return new SessionConfiguration(message);
    }

    private static bool IsHex(string? value, int length) =>
        value?.Length == length && value.All(c => c is >= '0' and <= '9' or >= 'a' and <= 'f');

    public bool Verify(ReadOnlySpan<byte> candidate)
    {
        lock (gate)
            return !revoked && candidate.Length == credential.Length &&
                CryptographicOperations.FixedTimeEquals(candidate, credential);
    }

    public bool Authorizes(HttpRequest request)
    {
        var authorization = request.Headers.Authorization;
        if (authorization.Count != 1 || authorization[0] is not { } value ||
            value.Length != 71 || !value.StartsWith("Bearer ", StringComparison.Ordinal)) return false;
        return Verify(Encoding.ASCII.GetBytes(value[7..]));
    }

    public bool MatchesBoundary(HttpRequest request) =>
        request.Headers.Origin.Count == 1 && request.Headers.Origin[0] == Origin &&
        request.Headers.Host.Count == 1 &&
        request.Headers.Host[0] == $"127.0.0.1:{request.HttpContext.Connection.LocalPort}";

    private sealed record StartupMessage(int Protocol, string Token, string SessionId, string Origin);
}
