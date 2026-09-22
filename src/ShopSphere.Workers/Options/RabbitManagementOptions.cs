namespace ShopSphere.Workers.Options;

public sealed class RabbitManagementOptions
{
    public string BaseUrl { get; set; } = "http://localhost:15672/";
    public string User { get; set; } = "guest";
    public string Password { get; set; } = string.Empty;
}