using PersonalAI.Application;
using PersonalAI.Domain;

namespace PersonalAI.UnitTests;

public class FoundationContractTests
{
    [Fact]
    public void FoundationStartsLockedWithoutUnlockOperation()
    {
        Assert.Equal(VaultState.Locked, new FoundationStatus().VaultState);
        Assert.DoesNotContain(typeof(FoundationStatus).GetMethods(), method => method.Name.Contains("Unlock"));
    }

    [Fact]
    public void RequiredPortsAreIndependentInterfaces()
    {
        Type[] ports = [typeof(IAIProvider), typeof(IProviderRegistry), typeof(ISecretVault),
            typeof(IConversationStore), typeof(IToolRegistry), typeof(IApprovalService),
            typeof(IIntegrationConnector), typeof(IMemoryService), typeof(IJobScheduler),
            typeof(ISettingsStore), typeof(IRequestAuthenticator)];
        Assert.All(ports, type => Assert.True(type.IsInterface));
        Assert.DoesNotContain(typeof(IAIProvider).Assembly.GetTypes(), type =>
            !type.IsAbstract && typeof(IAIProvider).IsAssignableFrom(type));
    }
}
