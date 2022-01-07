using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json;
using FluentResults;

namespace PlexRipper.Settings
{
    public abstract class BaseSettingsModule<TModel> where TModel : BaseSettingsModule<TModel>
    {
        #region Fields

        protected readonly Subject<TModel> _subject = new();

        #endregion

        #region Properties

        public IObservable<TModel> ModuleHasChanged => _subject.AsObservable();

        public abstract string Name { get; }

        #endregion

        #region Public Methods

        public abstract TModel GetValues();

        public Result Update(TModel sourceSettings)
        {
            var hasChanged = false;

            foreach (PropertyInfo prop in typeof(TModel).GetProperties())
            {
                var sourceProp = sourceSettings.GetType().GetProperty(prop.Name);
                var sourceValue = sourceProp.GetValue(sourceSettings, null);
                var targetProp = GetType().GetProperty(prop.Name);
                var targetValue = targetProp.GetValue(this, null);
                if (sourceValue != targetValue)
                {
                    targetProp.SetValue(this, sourceValue);
                    hasChanged = true;
                }
            }

            if (hasChanged)
            {
                EmitModuleHasChanged(GetValues());
            }

            return Result.Ok();
        }

        public void Reset()
        {
            Update(this as TModel);
        }

        #endregion

        #region Private Methods

        protected void EmitModuleHasChanged(TModel module)
        {
            _subject.OnNext(module);
        }

        protected Result<JsonElement> GetJsonSettingsModule(JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty(Name, out JsonElement confirmationSettings))
            {
                return Result.Ok(confirmationSettings);
            }

            return Result.Fail($"Could not find settings module {Name} in config file").LogError();
        }

        #endregion
    }
}