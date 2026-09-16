using Microsoft.EntityFrameworkCore;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.UnitTests.Common;

internal static class InMemoryDb
{
    public static ShopSphereDbContext New(
        [System.Runtime.CompilerServices.CallerMemberName] string caller = "")
    {
        var options = new DbContextOptionsBuilder<ShopSphereDbContext>()
            .UseInMemoryDatabase($"{caller}-{Guid.NewGuid():N}")
            .EnableSensitiveDataLogging()
            .Options;

        return new ShopSphereDbContext(options);
    }
}