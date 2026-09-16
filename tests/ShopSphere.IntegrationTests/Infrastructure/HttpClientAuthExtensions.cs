namespace ShopSphere.IntegrationTests.Infrastructure;

public static class HttpClientAuthExtensions
{
    public static HttpClient WithAuth(
        this HttpClient client,
        Guid? userId = null,
        string? email = null)
    {
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.HeaderUserId);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.HeaderEmail);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.HeaderRoles);
        client.DefaultRequestHeaders.Remove(TestAuthenticationHandler.HeaderPermissions);

        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.HeaderUserId,
            (userId ?? Guid.NewGuid()).ToString("D"));

        if (!string.IsNullOrWhiteSpace(email))
        {
            client.DefaultRequestHeaders.Add(
                TestAuthenticationHandler.HeaderEmail,
                email);
        }

        return client;
    }

    public static HttpClient WithAdmin(
        this HttpClient client,
        Guid? userId = null,
        string? email = null)
    {
        client.WithAuth(userId, email);
        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.HeaderRoles,
            "admin");
        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.HeaderPermissions,
            "products.write");

        return client;
    }

    public static HttpClient WithCustomer(
        this HttpClient client,
        Guid? userId = null,
        string? email = null)
    {
        client.WithAuth(userId, email);
        client.DefaultRequestHeaders.Add(
            TestAuthenticationHandler.HeaderRoles,
            "customer");

        return client;
    }
}
