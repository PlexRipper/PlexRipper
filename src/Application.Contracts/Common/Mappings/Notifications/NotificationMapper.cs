using PlexRipper.Domain;

namespace Application.Contracts;

public static class NotificationMapper
{
    #region ToDTO

    public static NotificationDTO ToDTO(this Notification source) =>
        new()
        {
            Id = source.Id,
            Level = source.Level,
            CreatedAt = source.CreatedAt,
            Message = source.Message,
            Hidden = source.Hidden,
        };

    public static List<NotificationDTO> ToDTO(this List<Notification> source) =>
        source.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static Notification ToModel(this NotificationDTO source) =>
        new()
        {
            Id = source.Id,
            Level = source.Level,
            CreatedAt = source.CreatedAt,
            Message = source.Message,
            Hidden = source.Hidden,
        };

    public static List<Notification> ToModel(this List<NotificationDTO> source) =>
        source.ConvertAll(ToModel);

    #endregion
}
