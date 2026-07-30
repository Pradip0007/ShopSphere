namespace ShopSphere.Domain.Common;

/// <summary>
/// A domain error. Code is machine-readable and stable — do NOT change
/// existing values, add new ones. Message is human-readable for logs.
/// </summary>
public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>A validation problem — bad input.</summary>
    public static Error Validation(string code, string message) => new(code, message);

    /// <summary>A conflict with existing state (e.g. duplicate key, stale write).</summary>
    public static Error Conflict(string code, string message) => new(code, message);

    /// <summary>A required resource does not exist.</summary>
    public static Error NotFound(string code, string message) => new(code, message);

    public override string ToString() => $"{Code}: {Message}";
}