using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class NotificationMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial NotificationDTO ToDTO(this Notification notifications);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<NotificationDTO> ToDTO(this List<Notification> notifications);

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial Notification ToModel(this NotificationDTO notifications);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<Notification> ToModel(this List<NotificationDTO> notifications);

    #endregion
}