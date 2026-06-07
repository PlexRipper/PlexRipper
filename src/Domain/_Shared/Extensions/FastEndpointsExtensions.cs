using System.Collections.Concurrent;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Builder;

namespace Reaparr.Domain;

public static class FastEndpointsExtensions
{
    private static readonly ConcurrentDictionary<Type, XmlSerializer> _serializerCache = new();

    /// <summary>
    /// Marks an endpoint as internal-only for documentation and discovery purposes.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder to decorate.</param>
    /// <returns>The same builder to allow fluent configuration.</returns>
    public static TBuilder IsInternalApi<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithGroupName("Internal API");
        return builder;
    }

    /// <summary>
    /// Marks an endpoint as public-facing for documentation and discovery purposes.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder to decorate.</param>
    /// <returns>The same builder to allow fluent configuration.</returns>
    public static TBuilder IsPublicApi<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithGroupName("Public API");
        return builder;
    }

    /// <summary>
    /// Adds the "Download Client" tag to the endpoint for grouping in documentation/UI.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder to decorate.</param>
    /// <returns>The same builder to allow fluent configuration.</returns>
    public static TBuilder IsDownloadClient<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithTags("Download Client");
        return builder;
    }

    /// <summary>
    /// Adds the "Indexer" tag to the endpoint for grouping in documentation/UI.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint builder to decorate.</param>
    /// <returns>The same builder to allow fluent configuration.</returns>
    public static TBuilder IsIndexer<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.WithTags("Indexer");
        return builder;
    }

    /// <summary>
    /// Sends an XML response asynchronously using a cached <see cref="XmlSerializer"/>.
    /// </summary>
    /// <typeparam name="TResponse">The response type to serialize.</typeparam>
    /// <param name="ep">The response sender associated with the current endpoint.</param>
    /// <param name="response">The response-object to serialize to XML.</param>
    /// <param name="statusCode">The HTTP status code to set on the response.</param>
    /// <param name="contentType">The response content type. Defaults to "application/xml".</param>
    /// <param name="cancellationToken">Token to observe while awaiting the copy operation.</param>
    public static async Task XmlAsync<TResponse>(
        this IResponseSender ep,
        TResponse response,
        int statusCode = 200,
        string contentType = "application/xml",
        CancellationToken cancellationToken = default
    )
    {
        ep.HttpContext.MarkResponseStart();
        ep.HttpContext.Response.StatusCode = statusCode;
        ep.HttpContext.Response.ContentType = contentType;

        var xmlSerializer = _serializerCache.GetOrAdd(typeof(TResponse), t => new XmlSerializer(t));

        await using var stream = new MemoryStream();
        xmlSerializer.Serialize(stream, response);
        stream.Position = 0;
        await stream.CopyToAsync(ep.HttpContext.Response.Body, cancellationToken);
    }

    /// <summary>
    /// Sends a FluentResults <see cref="Result"/> response using the project-standard <see cref="BaseResultDTO"/> envelope.
    /// </summary>
    /// <param name="sender">The response sender associated with the current endpoint.</param>
    /// <param name="result">The result to convert and send.</param>
    /// <param name="ct">Token to observe while sending the response.</param>
    public static async Task FluentResult(this IResponseSender sender, Result result, CancellationToken ct = default) =>
        await sender.SendFluentResultDTOAsync(result, result.ToResultDTO(), ct);

    /// <summary>
    /// Sends a FluentResults <see cref="Result{T}"/> response using the project-standard <see cref="ResultDTO{T}"/> envelope.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <param name="sender">The response sender associated with the current endpoint.</param>
    /// <param name="result">The result to convert and send.</param>
    /// <param name="ct">Token to observe while sending the response.</param>
    public static async Task FluentResult<T>(this IResponseSender sender, Result<T> result, CancellationToken ct = default) =>
        await sender.SendFluentResultDTOAsync(result.ToResult(), result.ToResultDTO(), ct);

    /// <summary>
    /// Sends a FluentResults <see cref="Result{T}"/> response after mapping the value into the response DTO type.
    /// </summary>
    /// <typeparam name="T">The result value type.</typeparam>
    /// <typeparam name="TResponse">The mapped response DTO type.</typeparam>
    /// <param name="sender">The response sender associated with the current endpoint.</param>
    /// <param name="result">The result to convert and send.</param>
    /// <param name="mapper">Maps a successful result value to the response DTO value.</param>
    /// <param name="ct">Token to observe while sending the response.</param>
    public static async Task FluentResult<T, TResponse>(
        this IResponseSender sender,
        Result<T> result,
        Func<T, TResponse> mapper,
        CancellationToken ct = default
    )
    {
        var resultDTO = result.ToResultDTO(mapper);
        await sender.SendFluentResultDTOAsync(result.ToResult(), resultDTO, ct);
    }

    private static async Task SendFluentResultDTOAsync<TResponse>(
        this IResponseSender sender,
        Result result,
        TResponse resultDTO,
        CancellationToken ct
    )
        where TResponse : BaseResultDTO
    {
        await result.SendResponseAsync(async statusCode =>
        {
            resultDTO.StatusCode = statusCode;
            await sender.HttpContext.Response.SendAsync(resultDTO, statusCode, cancellation: ct);
        });
    }

    private static async Task SendResponseAsync(this Result result, Func<int, Task> sendAsync)
    {
        if (result.IsSuccess)
        {
            // Status code 201 Created
            if (result.Has201CreatedRequestSuccess())
                await sendAsync(StatusCodes.Status201Created);
            // Status code 204 No Content
            else if (result.Has204NoContentRequestSuccess())
                await sendAsync(StatusCodes.Status204NoContent);
            // Status code 200 Ok
            else
                await sendAsync(StatusCodes.Status200OK);
        }
        else
        {
            // Status Code 400 Bad Request
            if (result.Has400BadRequestError())
                await sendAsync(StatusCodes.Status400BadRequest);
            // Status Code 401 Unauthorized
            else if (result.Has401UnauthorizedError())
                await sendAsync(StatusCodes.Status401Unauthorized);
            // Status Code 403 Forbidden
            else if (result.Has403ForbiddenError())
                await sendAsync(StatusCodes.Status403Forbidden);
            // Status Code 404 Not Found
            else if (result.Has404NotFoundError())
                await sendAsync(StatusCodes.Status404NotFound);
            // Status Code 502 Bad Gateway
            else if (result.Has502BadGatewayError())
                await sendAsync(StatusCodes.Status502BadGateway);
            // Status Code 504 Gateway Timeout
            else if (result.Has504GatewayTimeoutError())
                await sendAsync(StatusCodes.Status504GatewayTimeout);
            // Status Code 500 Internal Server Error
            else
                await sendAsync(StatusCodes.Status500InternalServerError);
        }
    }
}
