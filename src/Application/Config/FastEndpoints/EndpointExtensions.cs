using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public static class EndpointExtensions
{
    /// <summary>
    /// This will determine the status code to send based on the <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// NOTE: The callback is needed because using the <see cref="SendAsync"/> method from the <see cref="IEndpoint"/> will use the default serializer from FastEndpoints, which cannot be changed, and throw missing required property errors when serializing ResultDTO.
    /// See: https://fast-endpoints.com/docs/misc-conveniences#send-methods -> Limitations
    /// </remarks>
    /// <param name="ep"></param>
    /// <param name="result"></param>
    /// <param name="sendAsync"> </param>
    public static async Task SendResponseAsync(this IEndpoint ep, Result result, Func<int, Task> sendAsync)
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
            // Status Code 403 Forbidden
            else if (result.Has404NotFoundError())
                await sendAsync(StatusCodes.Status404NotFound);
            // Status Code 500 Internal Server Error
            else
                await sendAsync(StatusCodes.Status500InternalServerError);
        }
    }
}
