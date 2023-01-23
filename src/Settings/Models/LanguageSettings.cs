using Settings.Contracts;

namespace PlexRipper.Settings.Models;

public class LanguageSettings : ILanguageSettings
{
    public string Language { get; set; }
}