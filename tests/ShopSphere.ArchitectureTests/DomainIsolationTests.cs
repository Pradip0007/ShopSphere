namespace ShopSphere.ArchitectureTests;

public sealed class DomainIsolationTests
{
    [Fact]
    public void Domain_should_not_depend_on_EntityFrameworkCore()
    {
        var result = Types.InAssembly(Assemblies.Domain)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "EF Core must not leak into the Domain. Offending types:\n  - {0}",
            string.Join("\n  - ", result.FailingTypeNames ?? []));
    }
}