using System;
using System.Text.Json;
using FluentResults;

namespace PlexRipper.Application
{
    public interface IBaseSettingsModule<TModule, TModel>
    {
        public string Name { get; }

        public Result Update(TModel sourceSettings);

        public void Reset();

        /// <summary>
        /// Parses the Json Element from the PlexRipperSettings.json and defaults its value if nothing is found.
        /// This also works when adding new settings and ensuring old config files get used as much as possible.
        /// </summary>
        /// <param name="settingsJsonElement"></param>
        public Result SetFromJsonObject(JsonElement settingsJsonElement);

        public IObservable<TModel> ModuleHasChanged { get; }

        public TModel GetValues();
    }
}