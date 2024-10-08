using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexAccountMapper
{
    #region ToDTO

    public static PlexAccountDTO ToDTO(this PlexAccount source) =>
        new()
        {
            Id = source.Id,
            DisplayName = source.DisplayName,
            Username = source.Username,
            Password = source.Password,
            IsEnabled = source.IsEnabled,
            IsMain = source.IsMain,
            IsValidated = source.IsValidated,
            ValidatedAt = source.ValidatedAt,
            Uuid = source.Uuid,
            PlexId = source.PlexId,
            Email = source.Email,
            Title = source.Title,
            Is2Fa = source.Is2Fa,
            ClientId = source.ClientId,
            HasPassword = source.HasPassword,
            AuthenticationToken = source.AuthenticationToken,
            VerificationCode = source.VerificationCode,
            IsAuthTokenMode = source.IsAuthTokenMode,
            PlexServerAccess = source.PlexAccountServers.Select(x => x.PlexServerId).ToList(),
            PlexLibraryAccess = source.PlexAccountLibraries.Select(x => x.PlexLibraryId).ToList(),
        };

    public static List<PlexAccountDTO> ToDTO(this List<PlexAccount> plexAccounts) => plexAccounts.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static PlexAccount ToModel(this PlexAccountDTO source) =>
        new()
        {
            Id = source.Id,
            DisplayName = source.DisplayName,
            Username = source.Username,
            Password = source.Password,
            IsEnabled = source.IsEnabled,
            IsMain = source.IsMain,
            PlexAccountServers = [],
            PlexAccountLibraries = [],
            IsValidated = source.IsValidated,
            ValidatedAt = source.ValidatedAt,
            Uuid = source.Uuid,
            PlexId = source.PlexId,
            Email = source.Email,
            Title = source.Title,
            HasPassword = source.HasPassword,
            AuthenticationToken = source.AuthenticationToken,
            ClientId = source.ClientId,
            VerificationCode = source.VerificationCode,
            Is2Fa = source.Is2Fa,
            IsAuthTokenMode = source.IsAuthTokenMode,
        };

    #endregion
}
