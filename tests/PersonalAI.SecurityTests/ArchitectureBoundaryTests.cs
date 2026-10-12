using PersonalAI.Domain;

namespace PersonalAI.SecurityTests;

public class ArchitectureBoundaryTests
{
    [Fact]
    public void DomainReferencesOnlyPlatformAssemblies()
    {
        var references = typeof(IAIProvider).Assembly.GetReferencedAssemblies();
        Assert.All(references, assembly => Assert.StartsWith("System.", assembly.Name));
    }
}
