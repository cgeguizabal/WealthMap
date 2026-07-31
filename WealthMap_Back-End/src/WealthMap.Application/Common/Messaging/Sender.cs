using Microsoft.Extensions.DependencyInjection;

namespace WealthMap.Application.Common.Messaging;

public class Sender : ISender
{
    private readonly IServiceProvider _services;

    public Sender(IServiceProvider services) => _services = services;

    public async Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _services.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for {requestType.Name}.");

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _services.GetServices(behaviorType).Cast<object>().Reverse().ToList();

        RequestHandlerDelegate<TResponse> pipeline = token =>
            (Task<TResponse>)handlerType
                .GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!
                .Invoke(handler, new object[] { request, token })!;

        foreach (var behavior in behaviors)
        {
            var next = pipeline;
            var current = behavior;

            pipeline = token =>
                (Task<TResponse>)behaviorType
                    .GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.Handle))!
                    .Invoke(current, new object[] { request, next, token })!;
        }

        return await pipeline(ct);
    }
}