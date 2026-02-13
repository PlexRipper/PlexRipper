namespace Reaparr.Settings.Contracts;

public record LanguageSettingsModule
    : BaseSettingsModule<LanguageSettingsModule>,
        IBaseSettingsModule<LanguageSettingsModule>,
        ILanguageSettings
{
    public static LanguageSettingsModule Create() => new() { Language = "en-US" };

    public required string Language
    {
        get;
        set => SetProperty(ref field, value);
    } = "en-US";
}
