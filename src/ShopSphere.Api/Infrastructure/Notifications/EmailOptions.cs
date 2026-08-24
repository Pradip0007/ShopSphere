using System.ComponentModel.DataAnnotations;

namespace ShopSphere.Api.Infrastructure.Notifications;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required] public string Host { get; init; } = "localhost";
    [Range(1, 65535)] public int Port { get; init; } = 1025;
    public string? Username { get; init; }
    public string? Password { get; init; }
    public bool UseStartTls { get; init; } = false;
    public string FromAddress { get; init; } = "no-reply@shopsphere.dev";
    public string FromName { get; init; } = "ShopSphere";
}