using FastEndpoints;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class ValidatePlexTvTokenCommandHandler : ICommandHandler<ValidatePlexTokenCommand, Result<PlexAccount>>
{
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    public ValidatePlexTvTokenCommandHandler(IPlexApiClientFactory plexApiClientFactory)
    {
        _plexApiClientFactory = plexApiClientFactory;
    }

    public async Task<Result<PlexAccount>> ExecuteAsync(ValidatePlexTokenCommand command, CancellationToken ct)
    {
        var plexAccount = command.PlexAccount;
        var client = _plexApiClientFactory.CreateTvClient(plexAccount.GetAuthToken);

        var response = await client.Authentication.GetTokenDetailsAsync().ToResponse();

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
