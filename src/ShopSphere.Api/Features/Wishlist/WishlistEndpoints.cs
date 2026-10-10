using System.Security.Claims;
using MediatR;
using ShopSphere.Api.Infrastructure;
using ShopSphere.Domain.Catalog;
using ShopSphere.Domain.Users;
using ShopSphere.Domain.Wishlist;

namespace ShopSphere.Api.Features.Wishlist;

public sealed class WishlistEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/wishlist")
            .RequireAuthorization()
            .RequireRateLimiting("wishlist")
            .WithTags("Wishlist");

        group.MapGet("/", async (
                ClaimsPrincipal user,
                IWishlistRepository repository,
                CancellationToken ct) =>
            {
                UserId? userId = GetUserId(user);

                if (userId is null)
                    return Results.Unauthorized();

                ShopSphere.Domain.Wishlist.Wishlist? wishlist =
                    await repository.GetAsync(
                        userId.Value,
                        ct);

                var items = wishlist?.Items
                    .Select(item => new WishlistItemDto(
                        item.ProductId.Value,
                        item.AddedAtUtc))
                    .ToArray()
                    ?? [];

                return Results.Ok(new WishlistResponse(items));
            })
            .WithName("GetWishlist")
            .WithSummary("Get the current user's wishlist")
            .Produces<WishlistResponse>();

        group.MapPost("/items", async (
        WishlistItemCommand command,
        ClaimsPrincipal user,
        GuestWishlistStore guestWishlist,
        ISender sender,
        HttpContext http,
        CancellationToken ct) =>
        {
            UserId? userId = GetUserId(user);

            if (userId is null)
            {
                Guid? sessionId = WishlistSessionResolver.From(http);

                if (sessionId is null)
                    return Results.Unauthorized();

                await guestWishlist.AddAsync(
                    sessionId.Value,
                    command.ProductId);

                return Results.NoContent();
            }

            WishlistItemCommand request = command with
            {
                UserId = userId.Value,
                Action = WishlistItemAction.Add
            };

            await sender.Send(request, ct);

            return Results.NoContent();
        })
        .WithName("AddWishlistItem")
        .WithSummary("Add a product to the current user's wishlist")
        .Produces(StatusCodes.Status204NoContent)
        .AllowAnonymous();

        group.MapDelete("/items/{productId:guid}", async (
        Guid productId,
        ClaimsPrincipal user,
        GuestWishlistStore guestWishlist,
        ISender sender,
        HttpContext http,
        CancellationToken ct) =>
        {
            UserId? userId = GetUserId(user);

            if (userId is null)
            {
                Guid? sessionId = WishlistSessionResolver.From(http);

                if (sessionId is null)
                    return Results.Unauthorized();

                await guestWishlist.RemoveAsync(
                    sessionId.Value,
                    productId);

                return Results.NoContent();
            }

            await sender.Send(
                new WishlistItemCommand(
                    userId.Value,
                    productId,
                    WishlistItemAction.Remove),
                ct);

            return Results.NoContent();
        })
        .WithName("RemoveWishlistItem")
        .WithSummary("Remove a product from the current user's wishlist")
        .Produces(StatusCodes.Status204NoContent)
        .AllowAnonymous();
    }

    private static UserId? GetUserId(ClaimsPrincipal user)
    {
        string? value =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst("sub")?.Value;

        return Guid.TryParse(value, out Guid id)
            ? new UserId(id)
            : null;
    }
}

public sealed record WishlistItemDto(
    Guid ProductId,
    DateTimeOffset AddedAtUtc);

public sealed record WishlistResponse(
    IReadOnlyList<WishlistItemDto> Items);