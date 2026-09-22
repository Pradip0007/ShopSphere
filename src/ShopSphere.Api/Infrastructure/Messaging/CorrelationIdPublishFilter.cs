using MassTransit;
using ShopSphere.Api.Middleware;

namespace ShopSphere.Api.Infrastructure.Messaging;

public sealed class CorrelationIdPublishFilter<T> : IFilter<PublishContext<T>>
    where T : class
{
    private readonly IHttpContextAccessor _http;

    public CorrelationIdPublishFilter(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Task Send(
        PublishContext<T> context,
        IPipe<PublishContext<T>> next)
    {
        var id = _http.HttpContext?
            .Items[CorrelationIdMiddleware.Header] as string;

        if (id is not null)
        {
            context.Headers.Set(
                CorrelationIdMiddleware.Header,
                id);
        }

        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }
}
