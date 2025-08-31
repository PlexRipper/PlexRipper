using FastEndpoints;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public abstract class BaseEndpoint<TRequest> : Endpoint<TRequest, BaseResultDTO>
    where TRequest : class
{
    public abstract string EndpointPath { get; }

    protected async Task SendFluentResult(Result result, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO();
        await this.SendResponseAsync(
            result,
            async statusCode =>
            {
                resultDTO.StatusCode = statusCode;
                await Send.ResponseAsync(resultDTO, statusCode, ct);
            }
        );
    }
}

public abstract class BaseEndpoint<TRequest, TDTO> : BaseEndpoint<TRequest>
    where TRequest : class
{
    protected async Task SendFluentResult<T>(Result<T> result, CancellationToken ct = default)
    {
        this.HttpContext.AddResponseHeaders();

        var resultDTO = result.ToResultDTO();
        await this.SendResponseAsync(
            result.ToResult(),
            async statusCode =>
            {
                resultDTO.StatusCode = statusCode;
                await Send.ResponseAsync(resultDTO, statusCode, ct);
            }
        );
    }

    protected async Task SendFluentResult<T>(Result<T> result, Func<T, TDTO> mapper, CancellationToken ct = default)
    {
        this.HttpContext.AddResponseHeaders();

        var resultDTO = result.ToResultDTO(mapper);
        await this.SendResponseAsync(
            result.ToResult(),
            async statusCode =>
            {
                resultDTO.StatusCode = statusCode;
                await Send.ResponseAsync(resultDTO, statusCode, ct);
            }
        );
    }
}

public abstract class BaseEndpointWithoutRequest : EndpointWithoutRequest<BaseResultDTO>
{
    public abstract string EndpointPath { get; }

    protected async Task SendFluentResult(Result result, CancellationToken ct = default)
    {
        this.HttpContext.AddResponseHeaders();

        var resultDTO = result.ToResultDTO();
        await this.SendResponseAsync(
            result,
            async statusCode =>
            {
                resultDTO.StatusCode = statusCode;
                await Send.ResponseAsync(resultDTO, statusCode, ct);
            }
        );
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
        this.HttpContext.AddResponseHeaders();

        var resultDTO = result.ToResultDTO(mapper);
        await this.SendResponseAsync(
            result.ToResult(),
            async statusCode =>
            {
                resultDTO.StatusCode = statusCode;
                await Send.ResponseAsync(resultDTO, statusCode, ct);
            }
        );
    }
}
