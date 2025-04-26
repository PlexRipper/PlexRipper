using Application.Contracts;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Errors;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Newtonsoft.Json;
using PlexApi.Contracts;
using ILog = Logging.Interface.ILog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlexRipper.PlexApi;

public class PlexApiWrapper
{
    private readonly ILog _log;
    private readonly Func<PlexApiClientOptions?, IPlexApiClient> _clientFactory;

    public PlexApiWrapper(ILog log, Func<PlexApiClientOptions?, IPlexApiClient> clientFactory)
    {
        _log = log;
        _clientFactory = clientFactory;
    }

    private IPlexAPI CreateClient(string authToken, PlexApiClientOptions options) =>
        new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, accessToken: authToken);

    private async Task<Result<T>> ToResponse<T>(Task<T> operation)
        where T : class
    {
        try
        {
            return Result.Ok(await operation);
        }
        catch (SDKException e)
        {
            return e.RawResponse.FromSdkExceptionToResult<T>();
        }
        catch (JsonSerializationException e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
        catch (Exception e)
        {
            var errorsProperty = e.GetType().GetProperty("Errors");
            var rawResponseProperty = e.GetType().GetProperty("RawResponse");

            if (errorsProperty != null && rawResponseProperty != null)
            {
                var rawResponse = rawResponseProperty.GetValue(e);
                if (rawResponse is null)
                    return Result.Fail(new ExceptionalError(e)).LogError();

                var errors = errorsProperty.GetValue(e);
                var parsedErrors = JsonSerializer.Deserialize<List<PlexError>>(JsonSerializer.Serialize(errors));

                return ((HttpResponseMessage)rawResponse).FromSdkExceptionToResult<T>(parsedErrors);
            }

            if (rawResponseProperty != null)
            {
                var rawResponse = rawResponseProperty.GetValue(e);
                if (rawResponse is null)
                    return Result.Fail(new ExceptionalError(e)).LogError();

                return ((HttpResponseMessage)rawResponse).FromSdkExceptionToResult<T>();
            }

            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    /// <summary>
    /// Gets all the root level media metadata contained in this Plex library. For movies its all movies, and for tv shows its all the shows without seasons and episodes.
    /// <remarks>URL: {{SERVER_URL}}/library/sections/{{LIBRARY_KEY}}/all?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="authToken"></param>
    /// <param name="libraryKey"></param>
    /// <param name="startIndex"></param>
    /// <param name="batchSize"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<Result<GetAllMediaLibraryMediaContainer>> GetMetadataForLibraryAsync(
        PlexServerConnection connection,
        string authToken,
        string libraryKey,
        int startIndex,
        int batchSize,
        PlexMediaType type
    )
    {
        if (!int.TryParse(libraryKey, out var libraryKeyInt))
            return ResultExtensions.IsInvalidId(nameof(libraryKey), libraryKey).LogError();

        var client = CreateClient(
            authToken,
            new PlexApiClientOptions()
            {
                ConnectionUrl = connection.Url,
                Timeout = 30,
                RetryCount = 3,
            }
        );

        var response = await ToResponse(
            client.Library.GetAllMediaLibraryAsync(
                new()
                {
                    Type = type.ToApiTypeEnum<GetAllMediaLibraryQueryParamType>(),
                    SectionKey = libraryKeyInt,
                    IncludeMeta = GetAllMediaLibraryQueryParamIncludeMeta.Disable,
                    IncludeGuids = QueryParamIncludeGuids.Enable,
                    XPlexContainerStart = startIndex,
                    XPlexContainerSize = batchSize,
                }
            )
        );

        if (response.IsFailed)
            return response.ToResult();

        var value = response.Value?.Object?.MediaContainer ?? null;

        return value is null
            ? ResultExtensions.IsNull(nameof(response.Value.Object.MediaContainer)).LogError()
            : Result.Ok(value);
    }
}
