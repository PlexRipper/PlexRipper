using FastEndpoints;
using Logging.Interface;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class PlexSignInCommandHandler : ICommandHandler<PlexSignInCommand, Result<PlexAccount>>
{
    private readonly ILog _log;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    public PlexSignInCommandHandler(ILog log, IPlexApiClientFactory plexApiClientFactory)
    {
        _log = log;
        _plexApiClientFactory = plexApiClientFactory;
    }

    public async Task<Result<PlexAccount>> ExecuteAsync(PlexSignInCommand command, CancellationToken ct)
    {
        var plexAccount = command.PlexAccount;
        _log.Debug("Requesting PlexToken for account {UserName}", plexAccount.Username);

        var plexTvClient = _plexApiClientFactory.CreateTvClient();

        var responseResult = await plexTvClient
            .Authentication.PostUsersSignInDataAsync(
                new PostUsersSignInDataRequest
                {
                    ClientID = plexAccount.ClientId,
                    RequestBody = new PostUsersSignInDataRequestBody
                    {
                        Login = plexAccount.Username,
                        Password = plexAccount.Password,
                        RememberMe = false,
                        VerificationCode = plexAccount.Is2Fa ? plexAccount.VerificationCode : string.Empty,
                    },
                }
            )
            .ToResponse();

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
}
