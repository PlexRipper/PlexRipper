using Application.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public abstract class BaseEndpoint<TRequest> : Endpoint<TRequest, ResultDTO>
    where TRequest : class
{
    public abstract string EndpointPath { get; }

    protected async Task SendFluentResult(Result result, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO();
        await this.SendResponseAsync(result, resultDTO, ct);
    }
}

public abstract class BaseEndpoint<TRequest, TDTO> : BaseEndpoint<TRequest>
    where TRequest : class
{
    protected async Task SendFluentResult<T>(Result<T> result, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO();
        await this.SendResponseAsync(result.ToResult(), resultDTO, ct);
    }

    protected async Task SendFluentResult<T>(Result<T> result, Func<T, TDTO> mapper, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO(mapper);
        await this.SendResponseAsync(result.ToResult(), resultDTO, ct);
    }
}

public abstract class BaseEndpointWithoutRequest : EndpointWithoutRequest<ResultDTO>
{
    public abstract string EndpointPath { get; }

    protected async Task SendFluentResult(Result result, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO();
        await this.SendResponseAsync(result, resultDTO, ct);
    }
}

public abstract class BaseEndpointWithoutRequest<TResponse> : BaseEndpointWithoutRequest
{
    protected async Task SendFluentResult<T>(
        Result<T> result,
        Func<T, TResponse> mapper,
        CancellationToken ct = default
    )
    {
        var resultDTO = result.ToResultDTO(mapper);
        await this.SendResponseAsync(result.ToResult(), resultDTO, ct);
    }
}
