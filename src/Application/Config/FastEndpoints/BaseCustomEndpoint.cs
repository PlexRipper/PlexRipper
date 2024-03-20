using Application.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public abstract class BaseCustomEndpoint<TRequest, TResponse> : Endpoint<TRequest, TResponse> where TRequest : class where TResponse : ResultDTO
{
    public abstract string EndpointPath { get; }

    protected async Task SendResult(Result result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }

    protected async Task SendResult<T>(Result<T> result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }
}

public abstract class BaseCustomEndpointTest<TRequest> : Endpoint<TRequest, ResultDTO>
    where TRequest : class
{
    protected async Task SendFluentResult(Result result, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO();
        if (result.IsSuccess)
        {
            if (result.Has201CreatedRequestSuccess())
            {
                // Status code 201 Created
                await SendOkAsync(resultDTO, ct);
                return;
            }

            if (result.Has204NoContentRequestSuccess())
            {
                await SendNoContentAsync(ct);
                return;
            }

            // Status code 200 Ok
            await SendOkAsync(resultDTO, ct);
        }

        if (result.Has400BadRequestError())
        {
            await SendAsync(resultDTO, StatusCodes.Status400BadRequest, ct);
            return;
        }

        if (result.Has404NotFoundError())
        {
            await SendAsync(resultDTO, StatusCodes.Status404NotFound, ct);
            return;
        }

        // Status Code 500
        await SendAsync(resultDTO, StatusCodes.Status500InternalServerError, ct);
    }
}

public abstract class BaseCustomEndpointTest<TRequest, TDTO> : BaseCustomEndpointTest<TRequest>
    where TRequest : class
{
    public abstract string EndpointPath { get; }

    protected async Task SendFluentResult<T>(Result<T> result, Func<T, TDTO> mapper, CancellationToken ct = default)
    {
        if (result.IsSuccess)
        {
            var resultDTO = result.ToResultDTO(mapper);

            if (result.Has201CreatedRequestSuccess())
            {
                // Status code 201 Created
                await SendOkAsync(resultDTO, ct);
                return;
            }

            if (result.Has204NoContentRequestSuccess())
            {
                await SendNoContentAsync(ct);
                return;
            }

            // Status code 200 Ok
            await SendOkAsync(resultDTO, ct);
        }

        // If in failed state, then we can call the non-generic version of SendFluentResult because there wont be a non-null value
        await SendFluentResult(result.ToResult(), ct);
    }

    protected async Task SendResult(Result result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }

    protected async Task SendResult<T>(Result<T> result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }
}

public abstract class BaseCustomEndpointWithoutRequest<TResponse> : EndpointWithoutRequest<TResponse>
{
    public abstract string EndpointPath { get; }

    protected async Task SendResult(Result result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }

    protected async Task SendResult<T>(Result<T> result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }
}

public abstract class BaseCustomEndpointWithoutRequest : EndpointWithoutRequest
{
    public abstract string EndpointPath { get; }

    protected async Task SendResult(Result result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }

    protected async Task SendResult<T>(Result<T> result, CancellationToken ct = default)
    {
        await SendResultAsync(result.ToIResult());
    }
}