using MediatR;

using ShopSphere.Domain.Users;

namespace ShopSphere.Api.Features.Wishlist;

public enum WishlistItemAction
{
    Add,
    Remove
}

public sealed record WishlistItemCommand(
    UserId UserId,
    Guid ProductId,
    WishlistItemAction Action) : IRequest;