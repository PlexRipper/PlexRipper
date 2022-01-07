using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.Settings
{
    public abstract class BaseSettingsModule<TModel> : IBaseSettingsModule<TModel> where TModel : class
    {
        #region Fields

        protected readonly Subject<TModel> _subject = new();

        #endregion

        #region Properties

        public IObservable<TModel> ModuleHasChanged => _subject.AsObservable();

        public abstract string Name { get; }

        protected abstract TModel DefaultValue { get; }

        #endregion

        protected BaseSettingsModule()
        {
            Reset();
        }

        #region Public Methods

        public abstract TModel GetValues();

        public TModel Update(TModel sourceSettings)
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

            return GetValues();
        }

        public TModel Reset()
        {
            return Update(DefaultValue);
        }

        #endregion

        #region Private Methods

        protected void EmitModuleHasChanged(TModel module)
        {
            _subject.OnNext(module);
        }

        /// <inheritdoc/>
        public virtual Result SetFromJson(JsonElement jsonElement)
        {
            if (!jsonElement.TryGetProperty(Name, out JsonElement rootSettingsModule))
            {
                return Result.Fail($"Could not find settings module {Name} in config file").LogError();
            }

            foreach (PropertyInfo prop in typeof(TModel).GetProperties())
            {
                if (rootSettingsModule.TryGetProperty(prop.Name, out JsonElement jsonValueElement))
                {
                    var targetProp = GetType().GetProperty(prop.Name);
                    var targetPropType = targetProp.PropertyType;
                    var targetValue = targetProp.GetValue(this, null);

                    var sourceValue = jsonValueElement.GetTypedValue(targetPropType);
                    if (sourceValue != targetValue)
                    {
                        targetProp.SetValue(this, sourceValue);
                    }
                }
            }

            return Result.Ok();
        }

        #endregion
    }
}