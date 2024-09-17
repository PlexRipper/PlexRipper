using Application.Contracts;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Errors;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexRipper.PlexApi.Api.Media.Providers;
using PlexRipper.PlexApi.Api.Users.SignIn;
using PlexRipper.PlexApi.Helpers;
using PlexRipper.PlexApi.Models;
using RestSharp;
using DataFormat = RestSharp.DataFormat;
using ILog = Logging.Interface.ILog;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace PlexRipper.PlexApi.Api;

public class PlexApi
{
    private readonly ILog _log;
    private readonly Func<PlexApiClientOptions?, NewPlexApiClient> _clientFactory;

    public PlexApi(ILog log, PlexApiClient client, Func<PlexApiClientOptions?, NewPlexApiClient> clientFactory)
    {
        _log = log;
        _client = client;
        _clientFactory = clientFactory;
    }

    private readonly PlexApiClient _client;

    private string GetClientId => Guid.NewGuid().ToString();

    private PlexAPI CreateClient(string authToken, PlexApiClientOptions options) =>
        new(
            client: _clientFactory(options),
            xPlexClientIdentifier: GetClientId,
            serverUrl: options.ConnectionUrl,
            accessToken: authToken
        );

    private PlexAPI CreateTvClient(string authToken = "", PlexApiClientOptions? options = null)
    {
        options ??= new PlexApiClientOptions() { ConnectionUrl = "https://plex.tv/api/v2/" };

        options.ConnectionUrl = "https://plex.tv/api/v2/";

        return new PlexAPI(
            client: _clientFactory(options),
            xPlexClientIdentifier: GetClientId,
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
                new()
                {
                    Login = plexAccount.Username,
                    Password = plexAccount.Password,
                    RememberMe = false,
                    VerificationCode = plexAccount.Is2Fa ? plexAccount.VerificationCode : string.Empty,
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
        _log.Debug("Requesting PlexServerStatus for {Url}", connection.Url);

        var client = CreateClient(
            string.Empty,
            new PlexApiClientOptions
            {
                ConnectionUrl = connection.Url,
                Action = action,
                Timeout = 5,
            }
        );

        var responseResult = await ToResponse(client.Server.GetServerIdentityAsync());

        var statusCode = responseResult.IsSuccess
            ? responseResult.Value.StatusCode
            : responseResult.GetStatusCodeReason().StatusCode();
        var statusMessage = statusCode switch
        {
            200 => "The Plex server is online!",
            _ => "The Plex server could not be reached, most likely it's offline.",
        };

        return Result.Ok(
            new PlexServerStatus
            {
                StatusCode = statusCode,
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
    ///     Including the various connections to the server.
    ///     <remarks>https://plex.tv/api/v2/resources?X-Plex-Token={{AUTH_TOKEN}}</remarks>
    /// </summary>
    /// <param name="authToken">The Plex account authentication token.</param>
    /// <returns></returns>
    public async Task<Result<List<PlexDevice>>> GetAccessibleServers(string authToken)
    {
        if (string.IsNullOrEmpty(authToken))
            return ResultExtensions.IsEmpty(nameof(authToken)).LogError();

        var plexTvClient = CreateTvClient(
            authToken,
            new PlexApiClientOptions() { ConnectionUrl = string.Empty, Timeout = 15 }
        );

        var result = await Task.WhenAll(
            [
                ToResponse(plexTvClient.Plex.GetServerResourcesAsync()),
                ToResponse(
                    plexTvClient.Plex.GetServerResourcesAsync(
                        IncludeHttps.Enable,
                        IncludeRelay.Enable,
                        IncludeIPv6.Enable
                    )
                ),
            ]
        );

        if (result[0].IsFailed && result[1].IsFailed)
            return result[0].ToResult().WithReasons(result[1].Reasons);

        if (result[0].IsFailed && result[1].IsSuccess)
            return result[1].ToApiResult(x => x.PlexDevices ?? []);

        if (result[0].IsSuccess && result[1].IsFailed)
            return result[0].ToApiResult(x => x.PlexDevices ?? []);

        var list = new List<PlexDevice>();
        list.AddRange(result[0].Value?.PlexDevices ?? []);

        var result1Devices = result[1].Value?.PlexDevices ?? [];
        foreach (var serverResource in list)
        {
            var newServerResource = result1Devices.FirstOrDefault(x =>
                x.ClientIdentifier == serverResource.ClientIdentifier
            );
            if (newServerResource is null || !newServerResource.Connections.Any())
                continue;

            serverResource.Connections = serverResource
                .Connections.Concat(newServerResource.Connections)
                .Distinct()
                .ToList();
        }

        return Result.Ok(list);
    }

    /// <summary>
    ///     Returns a detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
    ///     <remarks>{{SERVER_URL}}/library/sections?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    /// <param name="plexAuthToken"></param>
    /// <param name="plexFullHost"></param>
    /// <returns></returns>
    public async Task<Result<LibrariesResponse>> GetAccessibleLibraryInPlexServerAsync(
        string plexAuthToken,
        string plexFullHost
    )
    {
        var request = new RestRequest(PlexApiPaths.GetLibraries(plexFullHost));

        request.AddToken(plexAuthToken);
        request.Timeout = 15000;

        _log.Debug("GetLibrarySectionsAsync => {Url}", request.Resource);
        return await _client.SendRequestAsync<LibrariesResponse>(request);
    }

    /// <summary>
    /// This returns the media providers that are available on the Plex server.
    /// NOTE: This includes all the accessible Plex libraries.
    /// </summary>
    /// <param name="plexAuthToken"></param>
    /// <param name="plexFullHost"></param>
    /// <returns></returns>
    public async Task<Result<MediaProvidersResponse>> GetServerProviderDataAsync(
        string plexAuthToken,
        string plexFullHost
    )
    {
        var request = new RestRequest(PlexApiPaths.GetProviderData(plexFullHost));

        request.AddToken(plexAuthToken);
        request.AddPlexHeaders(plexAuthToken);
        request.Timeout = 15000;

        _log.Debug("GetLibrarySectionsAsync => {Url}", request.Resource);
        return await _client.SendRequestAsync<MediaProvidersResponse>(request);
    }

    /// <summary>
    ///     Gets the all the root level media metadata contained in this Plex library. For movies its all movies, and for tv shows its all the shows without seasons and episodes.
    ///     <remarks>URL: {{SERVER_URL}}/library/sections/{{LIBRARY_KEY}}/all?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    /// <param name="authToken"></param>
    /// <param name="plexServerBaseUrl"></param>
    /// <param name="libraryKey"></param>
    /// <param name="startIndex"></param>
    /// <param name="batchSize"></param>
    /// <returns></returns>
    public async Task<Result<PlexMediaContainerDTO>> GetMetadataForLibraryAsync(
        string authToken,
        string plexServerBaseUrl,
        string libraryKey,
        int startIndex,
        int batchSize
    )
    {
        var request = new RestRequest(PlexApiPaths.GetLibrariesMetadata(plexServerBaseUrl, libraryKey));

        request.AddToken(authToken);
        request.AddLimitHeaders(startIndex, batchSize);

        // the Metadata is not needed for now
        // request.AddQueryParameter("includeMeta", "1");
        return await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
    }

    public async Task<PlexMediaContainerDTO> GetSeasonsAsync(string authToken, string plexFullHost, int ratingKey)
    {
        var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}/children"));

        request.AddToken(authToken);

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    /// <summary>
    ///     Gets all seasons contained within a media container.
    /// </summary>
    /// <param name="authToken">The authentication token.</param>
    /// <param name="plexServerUrl">The <see cref="PlexServer" /> url.</param>
    /// <param name="plexLibraryKey">The rating key from the <see cref="PlexLibrary" />.</param>
    /// <returns></returns>
    public async Task<PlexMediaContainerDTO> GetAllSeasonsAsync(
        string authToken,
        string plexServerUrl,
        string plexLibraryKey
    )
    {
        var request = new RestRequest(new Uri($"{plexServerUrl}/library/sections/{plexLibraryKey}/all"));

        request.AddToken(authToken);
        request.AddQueryParameter("type", "3");

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    /// <summary>
    ///     Gets all episodes within a media container.
    /// </summary>
    /// <param name="authToken">The authentication token.</param>
    /// <param name="plexServerUrl"></param>
    /// <param name="plexLibraryKey">The rating key from the <see cref="PlexLibrary" />.</param>
    /// <param name="from">The start range from which to request.</param>
    /// <param name="to">The end range to request for.</param>
    /// <returns></returns>
    public async Task<PlexMediaContainerDTO> GetAllEpisodesAsync(
        string authToken,
        string plexServerUrl,
        string plexLibraryKey,
        int from,
        int to
    )
    {
        var request = new RestRequest(new Uri($"{plexServerUrl}/library/sections/{plexLibraryKey}/all"));

        request.AddToken(authToken).AddLimitHeaders(from, to);
        request.AddQueryParameter("type", "4");

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    public async Task<PlexMediaContainerDTO> GetRecentlyAddedAsync(string authToken, string hostUrl, string sectionId)
    {
        var request = new RestRequest(new Uri($"{hostUrl}/library/sections/{sectionId}/recentlyAdded"));

        request.AddToken(authToken).AddLimitHeaders(0, 50);

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    /// <summary>
    ///     Retrieves the banner of <see cref="PlexMedia" />. Max size is width 680px and height 1000px;
    /// </summary>
    /// <param name="imageUrl">The absolute url of the banner, e.g. http://serverurl.com/library/metadata/22519/banner/252352</param>
    /// <param name="authToken">The server authentication token.</param>
    /// <param name="width">The optional width of the banner, default is 680px.</param>
    /// <param name="height">The optional height of the banner, default is 1000px.</param>
    /// <returns>The raw image data in a <see cref="Result" /></returns>
    public async Task<Result<byte[]>> GetPlexMediaImageAsync(
        string imageUrl,
        string authToken,
        int width = 0,
        int height = 0
    )
    {
        if (width > 0 && height > 0)
            imageUrl = $"{imageUrl}&width={width}&height={height}&minSize=1&upscale=1";

        var request = new RestRequest(new Uri(imageUrl));
        request.AddToken(authToken);
        request.Timeout = 10000;

        return await _client.SendImageRequestAsync(request);
    }

    public async Task<Result<AuthPin>> Get2FAPin(string clientId)
    {
        var request = new RestRequest(new Uri(PlexApiPaths.PlexPinUrl), Method.Post);

        request.AddPlexHeaders(clientId);

        return await _client.SendRequestAsync<AuthPin>(request);
    }

    public async Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId)
    {
        var request = new RestRequest(new Uri($"{PlexApiPaths.PlexPinUrl}/{pinId}"))
        {
            RequestFormat = DataFormat.Json,
        };

        request.AddPlexHeaders(clientId);

        return await _client.SendRequestAsync<AuthPin>(request);
    }

    public async Task<Result<SignInResponse>> ValidatePlexToken(string authToken, string clientId = "")
    {
        var request = new RestRequest(new Uri($"{PlexApiPaths.ValidatePlexTokenUrl}"))
        {
            RequestFormat = DataFormat.Json,
        };

        request.AddToken(authToken);
        request.AddPlexClientIdentifier(clientId);

        return await _client.SendRequestAsync<SignInResponse>(request);
    }
}
