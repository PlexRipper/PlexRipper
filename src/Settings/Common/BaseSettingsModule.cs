using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using FluentResults;

namespace PlexRipper.Settings
{
    public abstract class BaseSettingsModule<TModel>
    {
        protected readonly Subject<TModel> _subject = new();

        public IObservable<TModel> ModuleHasChanged => _subject.AsObservable();

        protected void EmitModuleHasChanged(TModel module)
        {
            _subject.OnNext(module);
        }

        public abstract string Name { get; }

        protected Result<JsonElement> GetJsonSettingsModule(JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty(Name, out JsonElement confirmationSettings))
            {
                return Result.Ok(confirmationSettings);
            }

            return Result.Fail($"Could not find settings module {Name} in config file").LogError();
        }
    }
}