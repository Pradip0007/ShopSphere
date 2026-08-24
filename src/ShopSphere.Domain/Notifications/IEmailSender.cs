namespace ShopSphere.Domain.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public sealed record EmailMessage(
    string ToAddress,
    string ToName,
    string Subject,
    string PlainBody,
    string? HtmlBody);