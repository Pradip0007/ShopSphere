using System.Reflection;

namespace ShopSphere.ArchitectureTests;

internal static class Assemblies
{
    public static readonly Assembly Domain =
        typeof(ShopSphere.Domain.AssemblyMarker).Assembly;

    public static readonly Assembly Infrastructure =
        typeof(ShopSphere.Infrastructure.AssemblyMarker).Assembly;

    public static readonly Assembly Api =
        typeof(ShopSphere.Api.AssemblyMarker).Assembly;
}