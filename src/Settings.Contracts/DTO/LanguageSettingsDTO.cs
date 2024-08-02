namespace Settings.Contracts;

public class LanguageSettingsDTO : ILanguageSettings
{
    public required string Language { get; set; }
}
