namespace ShopSphere.ArchitectureTests;

internal static class Slices
{
    public const string Root = "ShopSphere.Api.Features.";

    public static readonly string[] Names =
    [
        "Admin",
        "Auth",
        "Cart",
        "Catalog",
        "Checkout",
        "Inventory",
        "Me",
        "Newsletter",
        "Orders",
        "Reviews",
        "Webhooks"
    ];

    public static string Namespace(string slice) => Root + slice;

    public static bool IsAllowedDependency(string from, string to) =>
        (from, to) switch
        {
            ("Auth", "Cart") => true,
            ("Checkout", "Cart") => true,
            ("Checkout", "Orders") => true,
            ("Reviews", "Admin") => true,
            _ => false
        };
}
