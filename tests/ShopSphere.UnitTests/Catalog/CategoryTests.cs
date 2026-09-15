using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Catalog.Events;

namespace ShopSphere.UnitTests.Catalog;

public sealed class CategoryTests
{
    [Fact]
    public void Create_should_set_name_and_generate_slug()
    {
        var category = Category.Create("  Electronics  ");

        category.Name.Should().Be("Electronics");
        category.Slug.Value.Should().Be("electronics");
        category.ParentId.Should().BeNull();
    }

    [Fact]
    public void Create_should_use_explicit_slug()
    {
        var slug = Slug.From("custom-category");

        var category = Category.Create("Electronics", slug);

        category.Slug.Should().Be(slug);
    }

    [Fact]
    public void Create_should_set_parent()
    {
        var parentId = CategoryId.New();

        var category = Category.Create("Phones", parentId: parentId);

        category.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Create_should_raise_created_event()
    {
        var category = Category.Create("Electronics");

        category.DomainEvents.Should()
            .ContainSingle(e => e is CategoryCreatedEvent);
    }

    [Fact]
    public void Create_should_reject_empty_name()
    {
        var act = () => Category.Create("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_should_reject_name_longer_than_200_characters()
    {
        var act = () => Category.Create(new string('x', 201));

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("*200 characters or fewer*");
    }

    [Fact]
    public void Rename_should_trim_and_update_name()
    {
        var category = Category.Create("Electronics");

        category.Rename("  Mobile Phones  ");

        category.Name.Should().Be("Mobile Phones");
    }

    [Fact]
    public void Rename_should_reject_empty_name()
    {
        var category = Category.Create("Electronics");

        var act = () => category.Rename("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MoveTo_should_update_parent()
    {
        var category = Category.Create("Phones");
        var parentId = CategoryId.New();

        category.MoveTo(parentId);

        category.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void MoveTo_should_allow_removing_parent()
    {
        var parentId = CategoryId.New();
        var category = Category.Create("Phones", parentId: parentId);

        category.MoveTo(null);

        category.ParentId.Should().BeNull();
    }

    [Fact]
    public void MoveTo_should_reject_category_as_own_parent()
    {
        var category = Category.Create("Phones");

        var act = () => category.MoveTo(category.Id);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*cannot be its own parent*");
    }
}