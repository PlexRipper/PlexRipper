namespace Settings.Contracts;

public record LanguageSettingsModule : BaseSettingsModule<LanguageSettingsModule>, ILanguageSettings
{
    private string _language = "en-US";

    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }
}
