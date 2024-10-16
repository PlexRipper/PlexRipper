using System.Text.Json;
using Application.Contracts;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Errors;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;
using ILog = Logging.Interface.ILog;
using Type = LukeHagar.PlexAPI.SDK.Models.Requests.Type;

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

    private string GetClientId => Guid.NewGuid().ToString();

    private IPlexAPI CreateClient(string authToken, PlexApiClientOptions options) =>
        new PlexAPI(
            client: _clientFactory(options),
            clientID: GetClientId,
            serverUrl: options.ConnectionUrl,
            accessToken: authToken
        );

    private IPlexAPI CreateTvClient(string authToken = "", PlexApiClientOptions? options = null)
    {
        options ??= new PlexApiClientOptions { ConnectionUrl = "https://plex.tv/api/v2" };

        options.ConnectionUrl = "https://plex.tv/api/v2";

        return new PlexAPI(
            client: _clientFactory(options),
            clientID: GetClientId,
            serverUrl: options.ConnectionUrl,
            accessToken: authToken
        );
    }

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
    /// Sign in user with username and password and return user data with Plex authentication token.
    /// <remarks>NOTE: Plex "Managed" users do not work.</remarks>
    /// <example>URL: https://plex.tv/api/v2/users/signin?X-Plex-Client-Identifier=Chrome</example>
    /// </summary>
    /// <returns></returns>
    public async Task<Result<PlexAccount>> PlexSignInAsync(PlexAccount plexAccount)
    {
        _log.Debug("Requesting PlexToken for account {UserName}", plexAccount.Username);

        var plexTvClient = CreateTvClient();

        var responseResult = await ToResponse(
            plexTvClient.Authentication.PostUsersSignInDataAsync(
                new PostUsersSignInDataRequest()
                {
                    RequestBody = new PostUsersSignInDataRequestBody()
                    {
                        Login = plexAccount.Username,
                        Password = plexAccount.Password,
                        RememberMe = false,
                        VerificationCode = plexAccount.Is2Fa ? plexAccount.VerificationCode : string.Empty,
                    },
                }
            )
        );

        var result = responseResult.ToApiResult(x => new PlexAccount
        {
            Id = plexAccount.Id,
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsAuthTokenMode = plexAccount.IsAuthTokenMode,
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            PlexId = x.UserPlexAccount!.Id,
            Uuid = x.UserPlexAccount!.Uuid,
            ClientId = plexAccount.ClientId,
            Title = x.UserPlexAccount!.Title,
            Email = x.UserPlexAccount!.Email,
            HasPassword = x.UserPlexAccount!.HasPassword.GetValueOrDefault(),
            AuthenticationToken = x.UserPlexAccount!.AuthToken,
            IsMain = plexAccount.IsMain,
            PlexAccountServers = [],
            PlexAccountLibraries = [],
            Is2Fa = x.UserPlexAccount!.TwoFactorEnabled.GetValueOrDefault(),
            VerificationCode = string.Empty,
        });

        if (result.IsSuccess)
        {
            _log.Information(
                "Successfully retrieved the PlexAccount data for user {PlexAccountDisplayName} from the PlexApi",
                plexAccount.DisplayName
            );
        }

        return result;
    }

    public async Task<Result<string>> RefreshPlexAuthTokenAsync(PlexAccount plexAccount)
    {
        var result = await PlexSignInAsync(plexAccount);
        if (result.IsSuccess)
        {
            _log.Debug("Returned token was: {AuthToken}", result.Value.AuthenticationToken);
            return result.Value.AuthenticationToken;
        }

        return Result.Fail("Result from RequestPlexSignInDataAsync() was null.").LogError();
    }

    public async Task<Result<PlexServerStatus>> GetServerStatusAsync(
        PlexServerConnection connection,
        Action<PlexApiClientProgress>? action = null
    )
    {
        var client = CreateClient(
            string.Empty,
            new PlexApiClientOptions
            {
                ConnectionUrl = connection.Url,
                Action = action,
                Timeout = 10,
                RetryCount = 0,
            }
        );

        var responseResult = await ToResponse(client.Server.GetServerIdentityAsync());

        var statusCode = responseResult.IsSuccess
            ? responseResult.Value.StatusCode
            : responseResult.GetStatusCodeReason()?.GetStatusCode();
        var statusMessage = statusCode switch
        {
            200 => "The Plex server is online!",
            401 => "The Plex token has expired and needs to be refreshed.",
            _ => "The Plex server could not be reached, most likely it's offline.",
        };

        return Result.Ok(
            new PlexServerStatus
            {
                StatusCode = statusCode ?? -1,
                StatusMessage = statusMessage,
                LastChecked = DateTime.UtcNow,
                IsSuccessful = responseResult.IsSuccess,
                PlexServerId = connection.PlexServerId,
                PlexServerConnectionId = connection.Id,
            }
        );
    }

    /// <summary>
    ///     Retrieves all the accessible plex server based on the <see cref="PlexAccount" /> token
    ///     Including the various unique connections to each the server.
    ///     <remarks>https://plex.tv/api/v2/resources?X-Plex-Token={{AUTH_TOKEN}}</remarks>
    /// </summary>
    /// <param name="authToken">The Plex account authentication token.</param>
    /// <returns> A list of <see cref="PlexDevice" /> with all the connections to the servers.</returns>
    public async Task<Result<List<PlexDevice>>> GetAccessibleServers(string authToken)
    {
        if (string.IsNullOrEmpty(authToken))
            return ResultExtensions.IsEmpty(nameof(authToken)).LogError();

        var plexTvClient = CreateTvClient(
            authToken,
            new PlexApiClientOptions { ConnectionUrl = string.Empty, Timeout = 15 }
        );

        var result = await Task.WhenAll(
            ToResponse(plexTvClient.Plex.GetServerResourcesAsync()),
            ToResponse(
                plexTvClient.Plex.GetServerResourcesAsync(IncludeHttps.Enable, IncludeRelay.Enable, IncludeIPv6.Enable)
            )
        );

        if (result[0].IsFailed && result[1].IsFailed)
            return Result.Merge(result[0].ToResult(), result[1].ToResult());

        if (result[0].IsFailed && result[1].IsSuccess)
            return result[1].ToApiResult(x => x.PlexDevices ?? []);

        if (result[0].IsSuccess && result[1].IsFailed)
            return result[0].ToApiResult(x => x.PlexDevices ?? []);

        var deviceList1 = result[0].Value?.PlexDevices ?? [];
        var deviceList2 = result[1].Value?.PlexDevices ?? [];

        // Combine connections from both lists
        var combinedConnections = new List<Connections>();
        combinedConnections.AddRange(deviceList1.SelectMany(x => x.Connections));
        combinedConnections.AddRange(deviceList2.SelectMany(x => x.Connections));

        // Find duplicate connections and remove them all later.
        // This avoids the same connection going to different servers.
        var duplicateConnectionUris = combinedConnections
            .GroupBy(x => x.Uri)
            .Where(g => g.Count() > 1) // Find URIs that appear more than once
            .Select(g => g.Key) // Get the URI of the duplicates
            .ToHashSet();

        foreach (var device1 in deviceList1)
        {
            var device2 = deviceList2.FirstOrDefault(x => x.ClientIdentifier == device1.ClientIdentifier);
            if (device2 is null || !device2.Connections.Any())
                continue;

            var serverConnections = device1.Connections.Concat(device2.Connections).ToList();

            serverConnections.RemoveAll(x => duplicateConnectionUris.Contains(x.Uri));

            device1.Connections = serverConnections;
        }

        return Result.Ok(deviceList1);
    }

    /// <summary>
    ///     Returns a detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
    ///     <remarks>{{SERVER_URL}}/library/sections?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    /// <param name="plexAuthToken"></param>
    /// <param name="connection"></param>
    /// <returns></returns>
    public async Task<Result<List<PlexLibrary>>> GetAccessibleLibraryInPlexServerAsync(
        string plexAuthToken,
        PlexServerConnection connection
    )
    {
        var client = CreateClient(plexAuthToken, new PlexApiClientOptions { ConnectionUrl = connection.Url });

        var response = await ToResponse(client.Library.GetAllLibrariesAsync());

        if (response.IsFailed)
            return response.ToResult();

        if (response.Value.Object?.MediaContainer.Directory is null)
        {
            _log.Error(
                "Plex server: {PlexServerName} returned an empty response when libraries were requested",
                connection.PlexServer?.Name
            );
            return response.ToResult();
        }

        var directories = response.Value.Object.MediaContainer.Directory;

        var mappedLibraries = directories
            .Select(x => new PlexLibrary
            {
                Id = 0,
                Type = x.Type.ToPlexMediaTypeFromPlexApi(),
                Title = x.Title,
                Key = x.Key,
                CreatedAt = DateTimeExtensions.FromUnixTime(x.CreatedAt),
                UpdatedAt = DateTimeExtensions.FromUnixTime(x.UpdatedAt),
                ScannedAt = DateTimeExtensions.FromUnixTime(x.ScannedAt),
                SyncedAt = null,
                Uuid = x.Uuid,
                PlexServer = null,
                PlexServerId = connection.PlexServerId,
                DefaultDestination = null,
                DefaultDestinationId = null,
                Movies = [],
                TvShows = [],
                PlexAccountLibraries = [],
            })
            .ToList();

        return Result.Ok(mappedLibraries);
    }

    /// <summary>
    /// Gets the all the root level media metadata contained in this Plex library. For movies its all movies, and for tv shows its all the shows without seasons and episodes.
    /// <remarks>URL: {{SERVER_URL}}/library/sections/{{LIBRARY_KEY}}/all?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="authToken"></param>
    /// <param name="libraryKey"></param>
    /// <param name="startIndex"></param>
    /// <param name="batchSize"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<Result<GetLibraryItemsMediaContainer>> GetMetadataForLibraryAsync(
        PlexServerConnection connection,
        string authToken,
        string libraryKey,
        int startIndex,
        int batchSize,
        Type? type = null
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

        GetLibraryItemsQueryParamType? apiType = type switch
        {
            Type.Movie => GetLibraryItemsQueryParamType.Movie,
            Type.TvShow => GetLibraryItemsQueryParamType.TvShow,
            Type.Season => GetLibraryItemsQueryParamType.Season,
            Type.Episode => GetLibraryItemsQueryParamType.Episode,
            _ => null,
        };

        var response = await ToResponse(
            client.Library.GetLibraryItemsAsync(
                new GetLibraryItemsRequest()
                {
                    Type = apiType,
                    SectionKey = libraryKeyInt,
                    Tag = Tag.All,
                    IncludeMeta = GetLibraryItemsQueryParamIncludeMeta.Disable,
                    IncludeGuids = IncludeGuids.Enable,
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

    public async Task<Result<PlexAccount>> ValidatePlexToken(PlexAccount plexAccount, string authToken)
    {
        var client = CreateTvClient(authToken);

        var response = await ToResponse(client.Authentication.GetTokenDetailsAsync());

        return response.ToApiResult(x => new PlexAccount
        {
            Id = plexAccount.Id,
            DisplayName = plexAccount.DisplayName,
            Username = x.UserPlexAccount!.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsAuthTokenMode = plexAccount.IsAuthTokenMode,
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            PlexId = x.UserPlexAccount!.Id,
            Uuid = x.UserPlexAccount!.Uuid,
            ClientId = plexAccount.ClientId,
            Title = x.UserPlexAccount!.Title,
            Email = x.UserPlexAccount!.Email,
            HasPassword = x.UserPlexAccount!.HasPassword.GetValueOrDefault(),
            AuthenticationToken = x.UserPlexAccount!.AuthToken,
            IsMain = plexAccount.IsMain,
            PlexAccountServers = [],
            PlexAccountLibraries = [],
            Is2Fa = x.UserPlexAccount!.TwoFactorEnabled.GetValueOrDefault(),
            VerificationCode = string.Empty,
        });
    }
}
