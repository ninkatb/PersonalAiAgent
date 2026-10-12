using PersonalAI.Domain;
using PersonalAI.Application;

namespace PersonalAI.SecurityTests;

public class ArchitectureBoundaryTests
{
    [Fact]
    public void DomainReferencesOnlyPlatformAssemblies()
    {
        var references = typeof(IAIProvider).Assembly.GetReferencedAssemblies();
        Assert.All(references, assembly => Assert.StartsWith("System.", assembly.Name));
    }

    [Fact]
    public void ApplicationReferencesOnlyDomainPlatformAndDependencyInjection()
    {
        var references = typeof(FoundationStatus).Assembly.GetReferencedAssemblies();
        Assert.All(references, assembly => Assert.True(
            assembly.Name == "PersonalAI.Domain" ||
            assembly.Name == "Microsoft.Extensions.DependencyInjection.Abstractions" ||
            assembly.Name!.StartsWith("System.", StringComparison.Ordinal),
            $"Unexpected core dependency: {assembly.Name}"));
    }
}
