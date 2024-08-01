using Settings.Contracts;

namespace PlexRipper.Settings;

public record LanguageSettings : ILanguageSettings
{
    public string Language { get; set; } = "en-US";
}
