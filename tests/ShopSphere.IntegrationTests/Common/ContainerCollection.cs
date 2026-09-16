using Xunit;

namespace ShopSphere.IntegrationTests.Common;

[CollectionDefinition("Containers")]
public sealed class ContainerCollection : ICollectionFixture<ContainerFixture>
{
}
