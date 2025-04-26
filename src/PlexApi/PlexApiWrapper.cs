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

    private string GetClientId => Guid.NewGuid().ToString();

    private IPlexAPI CreateClient(string authToken, PlexApiClientOptions options) =>
        new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, accessToken: authToken);

    private IPlexAPI CreateTvClient(string authToken = "", PlexApiClientOptions? options = null)
    {
        options ??= new PlexApiClientOptions { ConnectionUrl = "https://plex.tv/api/v2" };

        options.ConnectionUrl = "https://plex.tv/api/v2";

        return new PlexAPI(client: _clientFactory(options), serverUrl: options.ConnectionUrl, accessToken: authToken);
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
                    ClientID = GetClientId,
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
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            PlexId = x.UserPlexAccount!.Id,
            Uuid = x.UserPlexAccount!.Uuid,
            ClientId = plexAccount.ClientId,
            Title = x.UserPlexAccount!.Title,
            Email = x.UserPlexAccount!.Email,
            HasPassword = x.UserPlexAccount!.HasPassword.GetValueOrDefault(),
            AuthenticationToken = x.UserPlexAccount!.AuthToken,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
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

    /// <summary>
    ///     Retrieves all the accessible plex server based on the <see cref="PlexAccount" /> token
    ///     Including the various unique connections to each server.
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
            ToResponse(plexTvClient.Plex.GetServerResourcesAsync(clientID: GetClientId)),
            ToResponse(
                plexTvClient.Plex.GetServerResourcesAsync(
                    clientID: GetClientId,
                    includeHttps: IncludeHttps.Enable,
                    includeRelay: IncludeRelay.Enable,
                    includeIPv6: IncludeIPv6.Enable
                )
            )
        );

        if (result[0].IsFailed && result[1].IsFailed)
            return Result.Merge(result[0].ToResult(), result[1].ToResult());

        if (result[0].IsFailed && result[1].IsSuccess)
            return result[1].ToApiResult(x => x.PlexDevices ?? []);

        if (result[0].IsSuccess && result[1].IsFailed)
            return result[0].ToApiResult(x => x.PlexDevices ?? []);

        var deviceList1 = result[0].Value?.PlexDevices?.FindAll(x => x.Provides.Contains("server")) ?? [];
        var deviceList2 = result[1].Value?.PlexDevices?.FindAll(x => x.Provides.Contains("server")) ?? [];

        var uniqueConnections = new HashSet<string>();

        foreach (var device1 in deviceList1)
        {
            var device2 = deviceList2.FirstOrDefault(x => x.ClientIdentifier == device1.ClientIdentifier);
            if (device2 is null || !device2.Connections.Any())
                continue;

            var serverConnections = device1.Connections.Concat(device2.Connections).ToList();

            device1.Connections.Clear();

            foreach (var connection in serverConnections)
            {
                if (uniqueConnections.Add(connection.Uri))
                {
                    device1.Connections.Add(connection);
                }
            }
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

        if (response.Value.Object?.MediaContainer?.Directory is null)
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
            })
            .ToList();

        return Result.Ok(mappedLibraries);
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

    /// <summary>
    /// Used to validate the connection URL to the Plex server.
    /// </summary>
    /// <param name="plexConnectionUrl"></param>
    /// <returns></returns>
    public async Task<Result<GetServerIdentityResponse>> ValidatePlexConnectionUrl(string plexConnectionUrl)
    {
        var client = CreateClient(
            string.Empty,
            new PlexApiClientOptions
            {
                ConnectionUrl = plexConnectionUrl,
                Timeout = 5,
                RetryCount = 0,
            }
        );

        return await ToResponse(client.Server.GetServerIdentityAsync());
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
            IsValidated = true,
            ValidatedAt = DateTime.UtcNow,
            PlexId = x.UserPlexAccount!.Id,
            Uuid = x.UserPlexAccount!.Uuid,
            ClientId = plexAccount.ClientId,
            Title = x.UserPlexAccount!.Title,
            Email = x.UserPlexAccount!.Email,
            HasPassword = x.UserPlexAccount!.HasPassword.GetValueOrDefault(),
            AuthenticationToken = x.UserPlexAccount!.AuthToken,
            CustomAuthenticationToken = plexAccount.CustomAuthenticationToken,
            IsMain = plexAccount.IsMain,
            PlexAccountServers = [],
            PlexAccountLibraries = [],
            Is2Fa = x.UserPlexAccount!.TwoFactorEnabled.GetValueOrDefault(),
            VerificationCode = string.Empty,
        });
    }
}
