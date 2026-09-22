using Grpc.Core;
using Grpc.Core.Interceptors;
using ShopSphere.Api.Middleware;

namespace ShopSphere.Api.Features.Inventory;

public sealed class CorrelationIdGrpcInterceptor(
    IHttpContextAccessor http) : Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        var id = http.HttpContext?
            .Items[CorrelationIdMiddleware.Header] as string;

        if (id is not null)
        {
            var metadata = context.Options.Headers ?? new Metadata();

            metadata.Add(
                CorrelationIdMiddleware.Header,
                id);

            context = new ClientInterceptorContext<TRequest, TResponse>(
                context.Method,
                context.Host,
                context.Options.WithHeaders(metadata));
        }

        return continuation(request, context);
    }
}
