using Application.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public abstract class BaseCustomEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse> where TRequest : class where TResponse : ResultDTO
{
    public abstract string EndpointPath { get; }

    protected async Task SendResult(Result result, CancellationToken ct)
    {
        await SendResultAsync(result.ToIResult());
    }

    protected async Task SendResult<T>(Result<T> result, CancellationToken ct)
    {
        await SendResultAsync(result.ToIResult());
    }
}

public abstract class BaseCustomEndpointWithoutRequest : EndpointWithoutRequest
{
    public abstract string EndpointPath { get; }

    protected async Task SendResult(Result result, CancellationToken ct)
    {
        await SendResultAsync(result.ToIResult());
    }

    protected async Task SendResult<T>(Result<T> result, CancellationToken ct)
    {
        await SendResultAsync(result.ToIResult());
    }
}