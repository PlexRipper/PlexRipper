using PlexRipper.Application;
using PlexRipper.PlexApi.Common.DTO;
using PlexRipper.PlexApi.Helpers;
using PlexRipper.PlexApi.Models;
using RestSharp;
using DataFormat = RestSharp.DataFormat;

namespace PlexRipper.PlexApi.Api;

public class PlexApi
{
    public PlexApi(PlexApiClient client)
    {
        _client = client;
    }

    private PlexApiClient _client { get; }

    private const string _signInUrl = "https://plex.tv/api/v2/users/signin";

    private const string _getAccountUrl = "https://plex.tv/users/account.json";

    private const string _plexServerUrl = "https://plex.tv/api/v2/resources";

    private const string _plexPinUrl = "https://plex.tv/api/v2/pins";

    /// <summary>
    /// Sign into the Plex API
    /// This is for authenticating users credentials with Plex.
    /// <para>NOTE: Plex "Managed" users do not work.</para>
    /// </summary>
    /// <returns></returns>
    public async Task<Result<PlexAccountDTO>> PlexSignInAsync(PlexAccount plexAccount)
    {
        Log.Debug($"Requesting PlexToken for account {plexAccount.Username}");
        var credentials = new CredentialsDTO
        {
            Login = plexAccount.Username,
            Password = plexAccount.Password,
            RememberMe = false,
        };

        if (plexAccount.Is2Fa)
            credentials.VerificationCode = plexAccount.VerificationCode;

        var request = new RestRequest(new Uri(_signInUrl), Method.Post)
            .AddPlexHeaders(plexAccount.ClientId)
            .AddJsonBody(credentials);

        return await _client.SendRequestAsync<PlexAccountDTO>(request, 0);
    }

    public async Task<Result<string>> RefreshPlexAuthTokenAsync(PlexAccount plexAccount)
    {
        var result = await PlexSignInAsync(plexAccount);
        if (result.IsSuccess)
        {
            Log.Debug($"Returned token was: {result.Value.AuthToken}");
            return result.Value.AuthToken;
        }

        return Result.Fail("Result from RequestPlexSignInDataAsync() was null.").LogError();
    }

    public async Task<Result<PlexServerStatus>> GetServerStatusAsync(string serverBaseUrl, string authToken, Action<PlexApiClientProgress> action = null)
    {
        // TODO Use healthCheck from here:
        // https://github.com/Arcanemagus/plex-api/wiki/Plex-Web-API-Overview
        var request = new RestRequest(new Uri($"{serverBaseUrl}/identity"));

        request.AddToken(authToken);

        Log.Debug($"Requesting PlexServerStatus for {serverBaseUrl}");
        var response = await _client.SendRequestAsync<ServerIdentityResponse>(request, 1, action);

        var statusCodeReason = response.GetStatusCodeReason();
        var statusCode = statusCodeReason?.StatusCode() ?? 0;
        var statusMessage = statusCodeReason?.ErrorMessage() ?? "";
        switch (statusCode)
        {
            case 200:
                statusMessage = $"The Plex server is online!";
                break;
            case 408:
                statusMessage = $"The Plex server could not be reached, most likely it's offline.";
                break;
        }

        return Result.Ok(new PlexServerStatus
        {
            StatusCode = statusCode,
            StatusMessage = statusMessage,
            LastChecked = DateTime.UtcNow,
            IsSuccessful = response.IsSuccess,
        });
    }

    public async Task<PlexAccountDTO> GetAccountAsync(string authToken)
    {
        var request = new RestRequest(new Uri(_getAccountUrl));

        request.AddToken(authToken);

        var result = await _client.SendRequestAsync<PlexAccountDTO>(request);
        return result.ValueOrDefault;
    }

    /// <summary>
    /// Retrieves all the accessible plex server based on the <see cref="PlexAccount"/> token
    /// </summary>
    /// <param name="authToken">The Plex account authentication token.</param>
    /// <returns></returns>
    public async Task<Result<List<ServerResource>>> GetServerAsync(string authToken)
    {
        var request = new RestRequest(new Uri(_plexServerUrl))
        {
            RequestFormat = DataFormat.Xml,
        };
        request.AddToken(authToken);

        var result = await _client.SendRequestAsync<Resources>(request);
        return result.IsFailed ? result.ToResult() : Result.Ok(result.Value.Resource);
    }

    /// <summary>
    /// Returns an detailed overview of the PlexLibraries in a PlexServer from the PlexAPI.
    /// </summary>
    /// <param name="plexAuthToken"></param>
    /// <param name="plexFullHost"></param>
    /// <returns></returns>
    public async Task<Result<PlexMediaContainerDTO>> GetLibrarySectionsAsync(string plexAuthToken, string plexFullHost)
    {
        var request = new RestRequest(new Uri($"{plexFullHost}/library/sections"));

        request.AddToken(plexAuthToken);

        Log.Debug($"GetLibrarySectionsAsync => {request.Resource}");
        return await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
    }

    public async Task<PlexMediaContainerDTO> GetMetadataForLibraryAsync(string authToken, string plexServerBaseUrl, string libraryKey)
    {
        var request = new RestRequest(new Uri($"{plexServerBaseUrl}/library/sections/{libraryKey}/all"));

        request.AddToken(authToken);

        request.AddQueryParameter("includeMeta", "1");

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    public async Task<PlexMediaContainerDTO> GetMetadataAsync(string authToken, string plexFullHost, int metadataId)
    {
        var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{metadataId}"));

        request.AddToken(authToken);

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    public async Task<PlexMediaContainerDTO> GetMetadataAsync(string authToken, string metaDataUrl)
    {
        var request = new RestRequest(new Uri(metaDataUrl));

        request.AddToken(authToken);

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    public async Task<PlexMediaContainerDTO> GetSeasonsAsync(string authToken, string plexFullHost, int ratingKey)
    {
        var request = new RestRequest(new Uri($"{plexFullHost}/library/metadata/{ratingKey}/children"));

        request.AddToken(authToken);

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    /// <summary>
    /// Gets all seasons contained within a media container.
    /// </summary>
    /// <param name="authToken">The authentication token.</param>
    /// <param name="plexServerUrl">The <see cref="PlexServer"/> url.</param>
    /// <param name="plexLibraryKey">The rating key from the <see cref="PlexLibrary"/>.</param>
    /// <returns></returns>
    public async Task<PlexMediaContainerDTO> GetAllSeasonsAsync(string authToken, string plexServerUrl, string plexLibraryKey)
    {
        var request = new RestRequest(new Uri($"{plexServerUrl}/library/sections/{plexLibraryKey}/all"));

        request.AddToken(authToken);

        request.AddQueryParameter("type", "3");

        var result = await _client.SendRequestAsync<PlexMediaContainerDTO>(request);
        return result.ValueOrDefault;
    }

    /// <summary>
    /// Gets all episodes within a media container.
    /// </summary>
    /// <param name="authToken">The authentication token.</param>
    /// <param name="plexServerUrl"></param>
    /// <param name="plexLibraryKey">The rating key from the <see cref="PlexLibrary"/>.</param>
    /// <param name="from">The start range from which to request.</param>
    /// <param name="to">The end range to request for.</param>
    /// <returns></returns>
    public async Task<PlexMediaContainerDTO> GetAllEpisodesAsync(string authToken, string plexServerUrl, string plexLibraryKey, int from, int to)
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
    /// Retrieves the banner of <see cref="PlexMedia"/>. Max size is width 680px and height 1000px;
    /// </summary>
    /// <param name="imageUrl">The absolute url of the banner, e.g. http://serverurl.com/library/metadata/22519/banner/252352</param>
    /// <param name="authToken">The server authentication token.</param>
    /// <param name="width">The optional width of the banner, default is 680px.</param>
    /// <param name="height">The optional height of the banner, default is 1000px.</param>
    /// <returns>The raw image data in a <see cref="Result"/></returns>
    public async Task<Result<byte[]>> GetPlexMediaImageAsync(string imageUrl, string authToken, int width = 0, int height = 0)
    {
        if (width > 0 && height > 0)
        {
            var uri = new Uri(imageUrl);

            imageUrl =
                $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}&width={width}&height={height}&minSize=1&upscale=1";
        }

        var request = new RestRequest(new Uri(imageUrl));
        request.AddToken(authToken);
        return await _client.SendImageRequestAsync(request);
    }

    public async Task<Result<AuthPin>> Get2FAPin(string clientId)
    {
        var request = new RestRequest(new Uri(_plexPinUrl), Method.Post);

        request.AddPlexHeaders(clientId);

        return await _client.SendRequestAsync<AuthPin>(request);
    }

    public async Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId)
    {
        var request = new RestRequest(new Uri($"{_plexPinUrl}/{pinId}"))
        {
            RequestFormat = DataFormat.Json,
        };

        request.AddPlexHeaders(clientId);

        return await _client.SendRequestAsync<AuthPin>(request);
    }
}