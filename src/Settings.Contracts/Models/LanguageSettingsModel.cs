namespace Settings.Contracts;

public record LanguageSettingsModel : BaseSettingsModel<LanguageSettingsModel>, ILanguageSettings
{
    private string _language = "en-US";

    public string Language
    {
        get => _language;
        set => SetProperty(ref _language, value);
    }
}
