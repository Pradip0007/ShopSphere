using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Middleware;
using ShopSphere.Domain.Newsletter;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Newsletter.Subscribe;

public sealed class SubscribeHandler(ShopSphereDbContext db)
    : IRequestHandler<SubscribeCommand, SubscribeResponse>
{
    public async Task<SubscribeResponse> Handle(
        SubscribeCommand request,
        CancellationToken cancellationToken)
    {
        string normalized = request.Email.Trim().ToLowerInvariant();

        bool alreadySubscribed = await db.NewsletterSubscriptions
            .AnyAsync(s => s.Email == normalized, cancellationToken);

        if (alreadySubscribed)
        {
            throw new ConflictException("This email is already subscribed.");
        }

        NewsletterSubscription subscription =
            NewsletterSubscription.Subscribe(normalized);

        db.NewsletterSubscriptions.Add(subscription);
        await db.SaveChangesAsync(cancellationToken);

        return new SubscribeResponse(
            "Thanks — check your inbox to confirm.");
    }
}
