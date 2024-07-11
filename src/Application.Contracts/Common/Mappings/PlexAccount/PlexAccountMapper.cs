using PlexRipper.Domain;

namespace Application.Contracts;

public static partial class PlexAccountMapper
{
    #region ToDTO

    public static PlexAccountDTO ToDTO(this PlexAccount plexAccount)
    {
        var dto = plexAccount.ToDTOMapper();
        dto.PlexServerAccess = new List<PlexServerAccessDTO>();

        // Determine the access for each server and libraries
        if (plexAccount.PlexAccountLibraries is not null && plexAccount.PlexAccountServers is not null)
        {
            var plexLibraries = plexAccount.PlexAccountLibraries.Select(y => y.PlexLibrary).ToList();
            foreach (var plexAccountServer in plexAccount.PlexAccountServers)
            {
                var plexServerAccess = plexAccountServer.PlexServer.ToAccessDTO();
                plexServerAccess.PlexLibraryIds = plexLibraries
                    .FindAll(x => x.PlexServerId == plexAccountServer.PlexServerId)
                    .Select(x => x.Id)
                    .ToList();
                dto.PlexServerAccess.Add(plexServerAccess);
            }
        }

        return dto;
    }

    private static PlexAccountDTO ToDTOMapper(this PlexAccount plexAccount) =>
        new()
        {
            Id = plexAccount.Id,
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
            IsValidated = plexAccount.IsValidated,
            ValidatedAt = plexAccount.ValidatedAt,
            Uuid = plexAccount.Uuid,
            PlexId = plexAccount.PlexId,
            Email = plexAccount.Email,
            Title = plexAccount.Title,
            HasPassword = plexAccount.HasPassword,
            AuthenticationToken = plexAccount.AuthenticationToken,
            ClientId = plexAccount.ClientId,
            VerificationCode = plexAccount.VerificationCode,
            Is2Fa = plexAccount.Is2Fa,
            PlexServerAccess = [],
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
        };

    public static List<PlexAccount> ToModel(this List<PlexAccountDTO> source) => source.ConvertAll(ToModel);

    public static PlexAccount ToModel(this UpdatePlexAccountDTO plexAccount) =>
        new()
        {
            Id = plexAccount.Id,
            DisplayName = plexAccount.DisplayName,
            Username = plexAccount.Username,
            Password = plexAccount.Password,
            IsEnabled = plexAccount.IsEnabled,
            IsMain = plexAccount.IsMain,
        };

    #endregion
}
