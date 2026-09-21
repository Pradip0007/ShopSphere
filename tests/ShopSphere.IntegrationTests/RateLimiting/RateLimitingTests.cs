using System.Net;
using System.Net.Http.Json;
using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;

namespace ShopSphere.IntegrationTests.RateLimiting;

[Collection("Containers")]
public sealed class RateLimitingTests(ContainerFixture containers)
    : IntegrationTestBase(containers, enableRateLimiter: true)
{
    [Fact]
    public async Task Auth_rate_limit_should_return_too_many_requests()
    {
        HttpResponseMessage? lastResponse = null;

        for (var i = 0; i < 11; i++)
        {
            lastResponse = await Client.PostAsJsonAsync(
                "/api/v1/auth/login",
                new
                {
                    email = "rate-limit-test@example.com",
                    password = "InvalidPassword123!"
                });
        }

        lastResponse.Should().NotBeNull();

        var body = await lastResponse!.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {lastResponse.StatusCode}");
        Console.WriteLine($"Body: {body}");

        lastResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Newsletter_rate_limit_should_return_too_many_requests()
    {
        HttpResponseMessage? lastResponse = null;

        for (var i = 0; i < 4; i++)
        {
            lastResponse = await Client.PostAsJsonAsync(
                "/api/v1/newsletter/subscribe",
                new
                {
                    email = "rate-limit-test@example.com"
                });
        }

        lastResponse.Should().NotBeNull();

        var body = await lastResponse!.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {lastResponse.StatusCode}");
        Console.WriteLine($"Body: {body}");

        lastResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Stripe_webhook_rate_limit_should_return_too_many_requests()
    {
        HttpResponseMessage? lastResponse = null;

        for (var i = 0; i < 31; i++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/api/v1/webhooks/stripe");

            request.Content = JsonContent.Create(new
            {
                id = $"evt_rate_limit_{i}",
                type = "payment_intent.succeeded"
            });

            request.Headers.Add("Stripe-Signature", "invalid");

            lastResponse = await Client.SendAsync(request);
        }

        lastResponse.Should().NotBeNull();

        var body = await lastResponse!.Content.ReadAsStringAsync();

        Console.WriteLine($"Status: {lastResponse.StatusCode}");
        Console.WriteLine($"Body: {body}");

        lastResponse.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}