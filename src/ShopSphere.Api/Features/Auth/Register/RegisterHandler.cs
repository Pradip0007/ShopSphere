using MediatR;
using Microsoft.EntityFrameworkCore;
using ShopSphere.Api.Middleware;
using ShopSphere.Domain.Users;
using ShopSphere.Infrastructure.Persistence;

namespace ShopSphere.Api.Features.Auth.Register;

public sealed class RegisterHandler(
    ShopSphereDbContext db,
    IPasswordHasher hasher)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        string normalized = request.Email.Trim().ToLowerInvariant();

        bool taken = await db.Users.AnyAsync(u => u.Email == normalized, cancellationToken);
        if (taken)
        {
            // Deliberately generic — do not leak "email already registered".
            throw new ConflictException("Registration failed.");
        }

        User user = User.Register(request.Email, request.Password, hasher);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(user.Id.Value);
    }
}