using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Catalog.Events;
using ShopSphere.Domain.Common;

namespace ShopSphere.UnitTests.Catalog;

public sealed class ProductTests
{
    [Fact]
    public void Create_should_start_as_draft()
    {
        var product = TestBuilders.Draft();

        product.Status.Should().Be(ProductStatus.Draft);
        product.Title.Should().Be("Test Product");
        product.Price.Amount.Should().Be(9.99m);
        product.Price.Currency.Should().Be("USD");
    }

    [Fact]
    public void Publish_should_change_status_to_published()
    {
        var product = TestBuilders.Draft();

        product.Publish();

        product.Status.Should().Be(ProductStatus.Published);
    }

    [Fact]
    public void Publish_should_raise_product_published_event()
    {
        var product = TestBuilders.Draft();

        product.Publish();

        product.DomainEvents.Should()
            .ContainSingle(e => e is ProductPublishedEvent);
    }

    [Fact]
    public void Publish_should_fail_when_already_published()
    {
        var product = TestBuilders.Draft();
        product.Publish();

        var act = () => product.Publish();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Archive_should_change_published_product_to_archived()
    {
        var product = TestBuilders.Draft();
        product.Publish();

        product.Archive();

        product.Status.Should().Be(ProductStatus.Archived);
    }

    [Fact]
    public void Archive_should_fail_when_product_is_draft()
    {
        var product = TestBuilders.Draft();

        var act = () => product.Archive();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Unarchive_should_return_archived_product_to_draft()
    {
        var product = TestBuilders.Draft();
        product.Publish();
        product.Archive();

        product.Unarchive();

        product.Status.Should().Be(ProductStatus.Draft);
    }

    [Fact]
    public void Rename_should_update_title_and_slug()
    {
        var product = TestBuilders.Draft();

        product.Rename("New Product Name");

        product.Title.Should().Be("New Product Name");
        product.Slug.Value.Should().Be("new-product-name");
    }

        [Fact]
    public void Create_should_reject_empty_title()
    {
        var act = () => Product.Create(
            title: "",
            description: "Test description",
            sku: Sku.From("TEST-SKU-2"),
            categoryId: CategoryId.New(),
            price: new Money(9.99m, "USD"));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_reject_title_longer_than_200_characters()
    {
        var title = new string('A', 201);

        var act = () => Product.Create(
            title: title,
            description: "Test description",
            sku: Sku.From("TEST-SKU-3"),
            categoryId: CategoryId.New(),
            price: new Money(9.99m, "USD"));

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*200 characters or fewer*");
    }

    [Fact]
    public void Create_should_reject_negative_price()
    {
        var act = () => TestBuilders.Draft(price: -1m);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*Price cannot be negative*");
    }

    [Fact]
    public void ChangePrice_should_update_price()
    {
        var product = TestBuilders.Draft();

        product.ChangePrice(new Money(19.99m, "USD"));

        product.Price.Amount.Should().Be(19.99m);
        product.Price.Currency.Should().Be("USD");
    }

    [Fact]
    public void ChangePrice_should_reject_zero_price()
    {
        var product = TestBuilders.Draft();

        var act = () => product.ChangePrice(new Money(0m, "USD"));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*must be positive*");
    }

    [Fact]
    public void ChangePrice_should_fail_for_archived_product()
    {
        var product = TestBuilders.Draft();
        product.Publish();
        product.Archive();

        var act = () => product.ChangePrice(new Money(19.99m, "USD"));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*archived product*");
    }

    [Fact]
    public void UpdateDescription_should_update_description()
    {
        var product = TestBuilders.Draft();

        product.UpdateDescription("Updated description");

        product.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDescription_should_reject_description_longer_than_4000_characters()
    {
        var product = TestBuilders.Draft();
        var description = new string('x', 4001);

        var act = () => product.UpdateDescription(description);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*4000 characters*");
    }
}