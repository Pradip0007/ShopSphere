using MediatR;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Wishlist;

namespace ShopSphere.Api.Features.Wishlist;

public sealed class WishlistItemHandler(
    IWishlistRepository repository,
    TimeProvider clock)
    : IRequestHandler<WishlistItemCommand>
{
    public async Task Handle(
        WishlistItemCommand request,
        CancellationToken ct)
    {
        ShopSphere.Domain.Wishlist.Wishlist? wishlist =
            await repository.GetAsync(
                request.UserId,
                ct);

        if (request.Action == WishlistItemAction.Add)
        {
            wishlist ??= ShopSphere.Domain.Wishlist.Wishlist.For(
                request.UserId);

            await repository.AddItemAsync(
                wishlist,
                new ProductId(request.ProductId),
                clock,
                ct);

            return;
        }

        if (wishlist is null)
            return;

        await repository.RemoveItemAsync(
            wishlist,
            new ProductId(request.ProductId),
            ct);
    }
}