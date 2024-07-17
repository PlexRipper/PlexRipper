using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexServerConnectionMapper
{
    #region ToDTO

    public static PlexServerConnectionDTO ToDTO(this PlexServerConnection source) =>
        new()
        {
            Id = source.Id,
            Protocol = source.Protocol,
            Address = source.Address,
            Port = source.Port,
            Local = source.Local,
            Relay = source.Relay,
            IPv4 = source.IPv4,
            IPv6 = source.IPv6,
            PortFix = source.PortFix,
            PlexServerId = source.PlexServerId,
            Url = source.Url,
            ServerStatusList = source.PlexServerStatus.ToDTO(),
            LatestConnectionStatus = source.LatestConnectionStatus?.ToDTO(),
            Progress = default,
        };

    public static List<PlexServerConnectionDTO> ToDTO(this List<PlexServerConnection> source) =>
        source.ConvertAll(ToDTO);

    public static ServerConnectionCheckStatusProgressDTO ToDTO(this ServerConnectionCheckStatusProgress source) =>
        new()
        {
            PlexServerId = source.PlexServerId,
            PlexServerConnectionId = source.PlexServerConnectionId,
            RetryAttemptIndex = source.RetryAttemptIndex,
            RetryAttemptCount = source.RetryAttemptCount,
            TimeToNextRetry = source.TimeToNextRetry,
            StatusCode = source.StatusCode,
            ConnectionSuccessful = source.ConnectionSuccessful,
            Completed = source.Completed,
            Message = source.Message,
        };

    public static List<ServerConnectionCheckStatusProgressDTO> ToDTO(
        this List<ServerConnectionCheckStatusProgress> source
    ) => source.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static PlexServerConnection ToModel(this PlexServerConnectionDTO source) =>
        new()
        {
            Id = source.Id,
            Protocol = source.Protocol,
            Address = source.Address,
            Port = source.Port,
            Local = source.Local,
            Relay = source.Relay,
            IPv4 = source.IPv4,
            IPv6 = source.IPv6,
            PortFix = source.PortFix,
            PlexServer = null,
            PlexServerId = source.PlexServerId,
            PlexServerStatus = source.ServerStatusList.ToModel(),
        };

    public static List<PlexServerConnection> ToModel(this List<PlexServerConnectionDTO> source) =>
        source.ConvertAll(ToModel);

    public static ServerConnectionCheckStatusProgress ToModel(this ServerConnectionCheckStatusProgressDTO source) =>
        new()
        {
            RetryAttemptIndex = source.RetryAttemptIndex,
            RetryAttemptCount = source.RetryAttemptCount,
            TimeToNextRetry = source.TimeToNextRetry,
            StatusCode = source.StatusCode,
            ConnectionSuccessful = source.ConnectionSuccessful,
            Completed = source.Completed,
            Message = source.Message,
            PlexServerConnection = default,
        };

    public static List<ServerConnectionCheckStatusProgress> ToModel(
        this List<ServerConnectionCheckStatusProgressDTO> source
    ) => source.ConvertAll(ToModel);

    #endregion
}
