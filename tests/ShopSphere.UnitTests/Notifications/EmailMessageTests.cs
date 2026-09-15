using ShopSphere.Domain.Notifications;

namespace ShopSphere.UnitTests.Notifications;

public sealed class EmailMessageTests
{
    [Fact]
    public void EmailMessage_should_store_all_message_fields()
    {
        var message = new EmailMessage(
            "user@example.com",
            "User",
            "Welcome",
            "Plain text",
            "<p>Plain text</p>");

        message.ToAddress.Should().Be("user@example.com");
        message.ToName.Should().Be("User");
        message.Subject.Should().Be("Welcome");
        message.PlainBody.Should().Be("Plain text");
        message.HtmlBody.Should().Be("<p>Plain text</p>");
    }

    [Fact]
    public void EmailMessage_should_allow_missing_html_body_and_use_value_equality()
    {
        var first = new EmailMessage("to", "name", "subject", "body", null);
        var second = new EmailMessage("to", "name", "subject", "body", null);

        first.HtmlBody.Should().BeNull();
        first.Should().Be(second);
    }
}
