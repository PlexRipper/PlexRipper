using System.Text.Json;
using FluentResults;

namespace Settings.Contracts;

public interface IBaseSettingsModule<TModel>
    where TModel : class
{
    public string Name { get; }

    public TModel Update(TModel sourceSettings);

    public TModel Reset();

    /// <summary>
    /// Parses the Json Element from the PlexRipperSettings.json and defaults its value if nothing is found.
    /// This also works when adding new settings and ensuring old config files get used as much as possible.
    /// </summary>
    /// <param name="settingsJsonElement"></param>
    public Result SetFromJson(JsonElement settingsJsonElement);

    public IObservable<TModel> ModuleHasChanged { get; }

    public TModel GetValues();

    public TModel DefaultValues();
}
