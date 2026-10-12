using Microsoft.Extensions.DependencyInjection;
using PersonalAI.Application;
using PersonalAI.Domain;

namespace PersonalAI.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public void FoundationResolvesLockedStatusAndDoesNotRegisterPrivilegedAdapters()
    {
        using var provider = new ServiceCollection().AddFoundation().BuildServiceProvider(
            new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        Assert.Equal(VaultState.Locked, provider.GetRequiredService<FoundationStatus>().VaultState);
        Assert.Null(provider.GetService<ISecretVault>());
        Assert.Null(provider.GetService<IAIProvider>());
        Assert.Null(provider.GetService<IApprovalService>());
    }
}
