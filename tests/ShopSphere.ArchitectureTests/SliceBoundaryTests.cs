using FluentAssertions;
using NetArchTest.Rules;

namespace ShopSphere.ArchitectureTests;

public sealed class SliceBoundaryTests
{
    public static IEnumerable<object[]> SlicePairs()
    {
        foreach (string from in Slices.Names)
        foreach (string to in Slices.Names)
        {
            if (from == to || Slices.IsAllowedDependency(from, to))
                continue;

            yield return [from, to];
        }
    }

    [Theory]
    [MemberData(nameof(SlicePairs))]
    public void Slice_should_not_reference_another_slice(string from, string to)
    {
        var result = Types.InAssembly(Assemblies.Api)
            .That()
            .ResideInNamespaceStartingWith(Slices.Namespace(from))
            .Should()
            .NotHaveDependencyOn(Slices.Namespace(to))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Slice '{0}' must not depend on slice '{1}'. Offenders:\n  - {2}",
            from,
            to,
            string.Join("\n  - ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Cart_should_not_reference_orders()
    {
        var result = Types.InAssembly(Assemblies.Api)
            .That()
            .ResideInNamespaceStartingWith(Slices.Namespace("Cart"))
            .Should()
            .NotHaveDependencyOn(Slices.Namespace("Orders"))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Cart must remain independently extractable from Orders. Offenders:\n  - {0}",
            string.Join("\n  - ", result.FailingTypeNames ?? []));
    }
}
