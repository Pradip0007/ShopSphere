using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Common;
using ShopSphere.Domain.Users;

namespace ShopSphere.UnitTests.Common;

public sealed class CommonTypesTests
{
    [Fact]
    public void Error_factories_should_create_expected_codes_and_string()
    {
        var validation = Error.Validation("validation.code", "Invalid input.");
        var conflict = Error.Conflict("conflict.code", "Already exists.");
        var notFound = Error.NotFound("not-found.code", "Missing.");

        validation.Code.Should().Be("validation.code");
        conflict.Message.Should().Be("Already exists.");
        notFound.ToString().Should().Be("not-found.code: Missing.");
    }

    [Fact]
    public void Entity_equality_should_use_type_and_identity()
    {
        var productId = ProductId.New();
        var first = Product.Create("Product", "Description", Sku.From("COMMON-1"), CategoryId.New(), new Money(1m, "USD"));
        var second = Product.Create("Product", "Description", Sku.From("COMMON-2"), CategoryId.New(), new Money(1m, "USD"));

        first.Should().NotBe(second);
        productId.Should().Be(productId);
        (productId == new ProductId(productId.Value)).Should().BeTrue();
        (productId != ProductId.New()).Should().BeTrue();
    }

    [Fact]
    public void Identifiers_should_create_non_empty_values_and_format_them()
    {
        var permissionId = PermissionId.New();
        var roleId = RoleId.New();
        var userId = UserId.New();
        var refreshTokenId = RefreshTokenId.New();

        permissionId.Value.Should().NotBe(Guid.Empty);
        roleId.Value.Should().NotBe(Guid.Empty);
        userId.Value.Should().NotBe(Guid.Empty);
        refreshTokenId.Value.Should().NotBe(Guid.Empty);
        permissionId.ToString().Should().Be(permissionId.Value.ToString());
        roleId.ToString().Should().Be(roleId.Value.ToString());
        userId.ToString().Should().Be(userId.Value.ToString());
        refreshTokenId.ToString().Should().Be(refreshTokenId.Value.ToString());
    }

    private sealed class SampleValueObject : ValueObject
    {
        private readonly string _value;

        public SampleValueObject(string value) => _value = value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return _value;
        }
    }

    [Fact]
    public void ValueObject_equality_should_be_structural_and_type_sensitive()
    {
        var first = new SampleValueObject("same");
        var second = new SampleValueObject("same");
        var different = new SampleValueObject("different");

        first.Should().Be(second);
        first.Should().NotBe(different);
        first.GetHashCode().Should().Be(second.GetHashCode());
        (first == second).Should().BeTrue();
        (first != different).Should().BeTrue();
    }

    [Fact]
    public void AggregateRoot_should_clear_domain_events()
    {
        var product = Product.Create("Product", "Description", Sku.From("COMMON-3"), CategoryId.New(), new Money(1m, "USD"));
        product.Publish();

        product.DomainEvents.Should().NotBeEmpty();
        product.ClearDomainEvents();

        product.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task NullDomainEventDispatcher_should_complete_without_dispatching()
    {
        var dispatcher = new NullDomainEventDispatcher();

        var act = () => dispatcher.DispatchAsync([]);

        await act.Should().NotThrowAsync();
    }
}
