using Microsoft.Extensions.DependencyInjection;

namespace Company.Core.Template.Application.Common.CustomMediator;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();

        // Resolve the specific handler for the request
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);

        // This is the delegate that calls the actual handler's .Handle method
        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            (Task<TResponse>)handler.GetType().GetMethod("Handle")!.Invoke(handler, new object[] { request, cancellationToken })!;

        // Resolve all pipeline behaviors registered for the specific request type
        var pipelineType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = _serviceProvider.GetServices(pipelineType).Reverse();

        // Chain the behaviors together using a foreach loop and reflection
        // This avoids the InvalidCastException by not casting to a generic interface
        foreach (var behavior in behaviors)
        {
            // Store the previous step of the pipeline
            var next = handlerDelegate;

            // Create a new delegate that calls the current behavior's Handle method
            handlerDelegate = () => (Task<TResponse>)behavior.GetType().GetMethod("Handle")!
                .Invoke(behavior, new object[] { request, next, cancellationToken })!;
        }

        // Execute the fully chained pipeline
        return handlerDelegate();
    }
}