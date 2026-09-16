using FluentAssertions;
using NetArchTest.Rules;

namespace ShopSphere.ArchitectureTests;

public sealed class SliceCompletenessTests
{
    [Theory]
    [MemberData(nameof(SliceNames))]
    public void Feature_slice_should_contain_an_endpoint_or_handler(string slice)
    {
        var types = Types.InAssembly(Assemblies.Api)
            .That()
            .ResideInNamespaceStartingWith(Slices.Namespace(slice))
            .GetTypes()
            .ToList();

        types.Should().NotBeEmpty(
            "Feature slice '{0}' has no types and should be removed or implemented.",
            slice);

        types.Any(type =>
                type.Name.EndsWith("Handler", StringComparison.Ordinal)
                || type.Name.EndsWith("Endpoint", StringComparison.Ordinal)
                || type.Name.EndsWith("Endpoints", StringComparison.Ordinal)
                || type.Name.EndsWith("Client", StringComparison.Ordinal))
            .Should().BeTrue(
                "Feature slice '{0}' must contain an endpoint, handler, or client.",
                slice);
    }

    public static IEnumerable<object[]> SliceNames() =>
        Slices.Names.Select(slice => new object[] { slice });
}
