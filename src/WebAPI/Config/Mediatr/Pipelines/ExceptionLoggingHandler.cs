using Logging.Interface;
using MediatR.Pipeline;

namespace PlexRipper.WebAPI;

public class ExceptionLoggingHandler<TRequest, TResponse, TException>
    : IRequestExceptionHandler<TRequest, TResponse, TException>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase, new()
    where TException : Exception
{
    private readonly ILog _log;

    public ExceptionLoggingHandler(ILog log)
    {
        _log = log;
    }

    public Task Handle(
        TRequest request,
        TException exception,
        RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken
    )
    {
        _log.Error(
            "An exception of type {@ExceptionType} happened when handling request of type {@RequestType}",
            typeof(TException),
            typeof(TRequest)
        );
        _log.Error(exception);

        state.SetHandled(default!);

        return Task.CompletedTask;
    }
}
