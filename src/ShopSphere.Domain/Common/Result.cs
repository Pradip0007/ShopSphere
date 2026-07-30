namespace ShopSphere.Domain.Common;

/// <summary>
/// Outcome of a domain operation that either succeeds (with no payload)
/// or fails with a single <see cref="Error"/>.
/// </summary>
public readonly record struct Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Common.Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");
        if (!isSuccess && error == Common.Error.None)
            throw new InvalidOperationException("A failed result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(true, Common.Error.None);
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Force the caller to handle both branches.</summary>
    public TOut Match<TOut>(Func<TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);
}