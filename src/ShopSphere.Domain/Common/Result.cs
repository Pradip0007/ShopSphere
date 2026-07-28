namespace ShopSphere.Domain.Common;

/// <summary>
/// TEMPORARY stub. Day 9 replaces this with a full Result/Error pattern.
/// </summary>
public readonly record struct Result(bool IsSuccess, string? Error)
{
    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
}