using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexServerStatusMapper
{
    #region ToDTO

    public static PlexServerStatusDTO ToDTO(this PlexServerStatus source) =>
        new()
        {
            Id = source.Id,
            StatusCode = source.StatusCode,
            IsSuccessful = source.IsSuccessful,
            StatusMessage = source.StatusMessage,
            LastChecked = source.LastChecked,
            PlexServerId = source.PlexServerId,
            PlexServerConnectionId = source.PlexServerConnectionId,
        };

    public static List<PlexServerStatusDTO> ToDTO(this List<PlexServerStatus> source) => source.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static PlexServerStatus ToModel(this PlexServerStatusDTO source) =>
        new()
        {
            Id = source.Id,
            IsSuccessful = source.IsSuccessful,
            StatusCode = source.StatusCode,
            StatusMessage = source.StatusMessage,
            LastChecked = source.LastChecked,
            PlexServer = default,
            PlexServerId = source.PlexServerId,
            PlexServerConnection = default,
            PlexServerConnectionId = source.PlexServerConnectionId,
        };

    public static List<PlexServerStatus> ToModel(this List<PlexServerStatusDTO> source) => source.ConvertAll(ToModel);

    #endregion
}
