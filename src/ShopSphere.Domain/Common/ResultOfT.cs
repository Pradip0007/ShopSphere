namespace ShopSphere.Domain.Common;

/// <summary>
/// Outcome that either succeeds with a <typeparamref name="T"/> payload,
/// or fails with an <see cref="Error"/>. Never both.
/// </summary>
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsFailure => !IsSuccess;

    public static Result<T> Success(T value) => new(true, value, Common.Error.None);
    public static Result<T> Failure(Error error) => new(false, default, error);

    /// <summary>Implicit lift so handlers can just return theValue;.</summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>Implicit lift so handlers can just return theError;.</summary>
    public static implicit operator Result<T>(Error error) => Failure(error);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(Error);
}