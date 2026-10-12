using Microsoft.Extensions.DependencyInjection;
using PersonalAI.Domain;

namespace PersonalAI.Application;

// Foundation has no vault, unlock operation or registered external tools.
public sealed class FoundationStatus
{
    public VaultState VaultState => VaultState.Locked;
}

public static class FoundationServices
{
    public static IServiceCollection AddFoundation(this IServiceCollection services)
    {
        services.AddSingleton<FoundationStatus>();
        return services;
    }
}
