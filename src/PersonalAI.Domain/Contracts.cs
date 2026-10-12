namespace PersonalAI.Domain;

public enum VaultState { Locked, Unlocked }
public enum ResponseLanguage { FollowUser, SerbianEkavian, SerbianIjekavian, English }
public sealed record AIMessage(string Role, string Content);
public sealed record AIRequest(string Model, IReadOnlyList<AIMessage> Messages);
public sealed record AIMessageChunk(string Content);
public sealed record Conversation(Guid Id, IReadOnlyList<AIMessage> Messages);
public sealed record ToolDefinition(string Id, string ArgumentsSchema);
public sealed record ApprovalRequest(string ToolId, string CanonicalArguments);
public sealed record ConnectorStatus(string Id, bool Connected);
public sealed record MemoryEntry(Guid Id, string Content);
public sealed record ScheduledJob(Guid Id, DateTimeOffset DueAt);
public sealed record UserSettings(string UiLocale, ResponseLanguage ResponseLanguage);

public interface IAIProvider
{
    string Id { get; }
    IAsyncEnumerable<AIMessageChunk> GenerateAsync(AIRequest request, CancellationToken cancellationToken);
}
public interface IProviderRegistry { IAIProvider? Find(string providerId); }
public interface ISecretVault
{
    VaultState State { get; }
    ValueTask<ReadOnlyMemory<byte>> ReadAsync(string secretId, CancellationToken cancellationToken);
    ValueTask LockAsync(CancellationToken cancellationToken);
}
public interface IConversationStore
{
    ValueTask<Conversation?> FindAsync(Guid id, CancellationToken cancellationToken);
    ValueTask SaveAsync(Conversation conversation, CancellationToken cancellationToken);
}
public interface IToolRegistry { ToolDefinition? Find(string toolId); }
public interface IApprovalService
{
    ValueTask<bool> IsApprovedAsync(ApprovalRequest request, CancellationToken cancellationToken);
}
public interface IIntegrationConnector
{
    string Id { get; }
    ValueTask<ConnectorStatus> GetStatusAsync(CancellationToken cancellationToken);
}
public interface IMemoryService
{
    ValueTask<IReadOnlyList<MemoryEntry>> SearchAsync(string query, CancellationToken cancellationToken);
}
public interface IJobScheduler
{
    ValueTask ScheduleAsync(ScheduledJob job, CancellationToken cancellationToken);
}
public interface ISettingsStore
{
    ValueTask<UserSettings> ReadAsync(CancellationToken cancellationToken);
    ValueTask SaveAsync(UserSettings settings, CancellationToken cancellationToken);
}
public interface IRequestAuthenticator { bool Verify(ReadOnlySpan<byte> credential); }
