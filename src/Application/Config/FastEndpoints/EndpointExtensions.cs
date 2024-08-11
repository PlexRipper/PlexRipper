using Application.Contracts;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public static class EndpointExtensions
{
    public static async Task SendResponseAsync<T>(this IEndpoint ep, Result<T> result, CancellationToken ct = default)
    {
        var resultDTO = result.ToResultDTO();
        await ep.SendResponseAsync(result.ToResult(), resultDTO, ct);
    }

    public static async Task SendResponseAsync(
        this IEndpoint ep,
        Result result,
        ResultDTO resultDTO,
        CancellationToken ct = default
    )
    {
        if (result.IsSuccess)
        {
            // Status code 201 Created
            if (result.Has201CreatedRequestSuccess())
            {
                await ep.HttpContext.Response.SendOkAsync(resultDTO, cancellation: ct);
                return;
            }

            // Status code 204 No Content
            if (result.Has204NoContentRequestSuccess())
            {
                await ep.HttpContext.Response.SendNoContentAsync(ct);
                return;
            }

            // Status code 200 Ok
            await ep.HttpContext.Response.SendOkAsync(resultDTO, cancellation: ct);
            return;
        }

        // Status Code 400 Bad Request
        if (result.Has400BadRequestError())
        {
            await ep.HttpContext.Response.SendAsync(resultDTO, StatusCodes.Status400BadRequest, cancellation: ct);
            return;
        }

        // Status Code 401 Unauthorized
        if (result.Has401UnauthorizedError())
        {
            await ep.HttpContext.Response.SendAsync(resultDTO, StatusCodes.Status401Unauthorized, cancellation: ct);
            return;
        }

        // Status Code 403 Forbidden
        if (result.Has404NotFoundError())
        {
            await ep.HttpContext.Response.SendAsync(resultDTO, StatusCodes.Status404NotFound, cancellation: ct);
            return;
        }

        // Status Code 500
        await ep.HttpContext.Response.SendAsync(resultDTO, StatusCodes.Status500InternalServerError, cancellation: ct);
    }
}
