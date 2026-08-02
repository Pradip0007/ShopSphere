using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace ShopSphere.Api.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);

        ValidationResult[] results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        ValidationFailure[] failures = [.. results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)];

        if (failures.Length != 0)
        {
            throw new ValidationException(failures);
        }

        return await next();
    }
}