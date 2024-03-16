using Application.Contracts;
using FastEndpoints;

namespace PlexRipper.Application.FastEndpoints;

public abstract class BaseEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse> where TRequest : class where TResponse : ResultDTO
{
    public abstract string EndpointPath { get; }

    protected async Task SendResult(Result result, CancellationToken ct)
    {
        await SendResultAsync(result.ToIResult());
    }
}