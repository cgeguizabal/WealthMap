namespace WealthMap.Application.Common.Messaging;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken ct);

public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct);
}