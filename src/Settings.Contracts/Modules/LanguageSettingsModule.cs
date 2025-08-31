namespace Reaparr.Settings.Contracts;

public record LanguageSettingsModule : BaseSettingsModule<LanguageSettingsModule>, ILanguageSettings
{
    private string _language = "en-US";

    public static LanguageSettingsModule Create() => new() { Language = "en-US" };

    public required string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }
}
