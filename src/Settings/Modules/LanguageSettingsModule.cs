using Settings.Contracts;

namespace PlexRipper.Settings;

public class LanguageSettingsModule : BaseSettingsModule<ILanguageSettings>, ILanguageSettingsModule
{
    #region Properties

    public string Language { get; set; }

    public override string Name => "LanguageSettings";

    public override ILanguageSettings DefaultValues() => new LanguageSettings { Language = "en-US" };

    #endregion

    #region Public Methods

    public override ILanguageSettings GetValues() => new LanguageSettings { Language = Language };

    public Result Update(ILanguageSettingsModule sourceSettings)
    {
        Language = sourceSettings.Language;
        return Result.Ok();
    }

    #endregion
}
