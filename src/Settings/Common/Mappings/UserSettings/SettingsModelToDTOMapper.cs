using PlexRipper.Settings.Models;
using Riok.Mapperly.Abstractions;
using Settings.Contracts;

namespace PlexRipper.Settings;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Both)]
public static partial class SettingsModelToDTOMapper
{
    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial SettingsModel ToModel(this SettingsModelDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial GeneralSettings ToModel(this GeneralSettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial ConfirmationSettings ToModel(this ConfirmationSettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial DateTimeSettings ToModel(this DateTimeSettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial DisplaySettings ToModel(this DisplaySettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial LanguageSettings ToModel(this LanguageSettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial DownloadManagerSettings ToModel(this DownloadManagerSettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial ServerSettings ToModel(this ServerSettingsDTO dto);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial DebugSettings ToModel(this DebugSettingsDTO dto);

    #endregion

    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial SettingsModelDTO ToDTO(this SettingsModel model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial GeneralSettingsDTO ToDTO(this GeneralSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial ConfirmationSettingsDTO ToDTO(this ConfirmationSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DateTimeSettingsDTO ToDTO(this DateTimeSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DisplaySettingsDTO ToDTO(this DisplaySettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial LanguageSettingsDTO ToDTO(this LanguageSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DownloadManagerSettingsDTO ToDTO(this DownloadManagerSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial ServerSettingsDTO ToDTO(this ServerSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DebugSettingsDTO ToDTO(this DebugSettings model);

    // Interfaces ToDTO
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial SettingsModelDTO ToDTO(this ISettingsModel model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial GeneralSettingsDTO ToDTO(this IGeneralSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial ConfirmationSettingsDTO ToDTO(this IConfirmationSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DateTimeSettingsDTO ToDTO(this IDateTimeSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DisplaySettingsDTO ToDTO(this IDisplaySettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DownloadManagerSettingsDTO ToDTO(this IDownloadManagerSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial LanguageSettingsDTO ToDTO(this ILanguageSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial ServerSettingsDTO ToDTO(this IServerSettings model);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial DebugSettingsDTO ToDTO(this IDebugSettings model);

    #endregion
}