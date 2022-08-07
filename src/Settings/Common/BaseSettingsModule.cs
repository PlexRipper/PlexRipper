using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using PlexRipper.Application;

namespace PlexRipper.Settings;

public abstract class BaseSettingsModule<TModel> : IBaseSettingsModule<TModel> where TModel : class
{
    #region Fields

    private readonly Subject<TModel> _moduleUpdatedSubject = new();

    #endregion

    #region Constructor

    protected BaseSettingsModule()
    {
        // Initialize with the default values
        Reset();
    }

    #endregion

    #region Properties

    public IObservable<TModel> ModuleHasChanged => _moduleUpdatedSubject.AsObservable();

    public abstract string Name { get; }

    public abstract TModel DefaultValues();

    #endregion

    #region Public Methods

    public abstract TModel GetValues();

    public TModel Reset()
    {
        return Update(DefaultValues());
    }

    /// <inheritdoc/>
    public virtual Result SetFromJson(JsonElement jsonElement)
    {
        var jsonSettings = GetJsonSettingsModule(jsonElement);
        if (jsonSettings.IsFailed)
        {
            Reset();
            return jsonSettings.ToResult();
        }

        var defaultValues = DefaultValues();
        var rootSettingsModule = jsonSettings.Value;
        foreach (var prop in typeof(TModel).GetProperties())
        {
            var targetProp = GetType().GetProperty(prop.Name);
            var targetPropType = targetProp.PropertyType;
            var targetValue = targetProp.GetValue(this, null);

            // Is settings value available in JSON? Else revert to default value
            if (rootSettingsModule.TryGetProperty(prop.Name, out var jsonValueElement))
            {
                var sourceValue = jsonValueElement.GetTypedValue(targetPropType);
                if (sourceValue != targetValue)
                    targetProp.SetValue(this, sourceValue);
            }
            else
            {
                Log.Warning(
                    $"The userSettings, in module \"{Name}\", was missing property \"{prop.Name}\". " +
                    $"Will revert to default value now, this is normal if you just updated PlexRipper as new settings might have been added.");
                var defaultValue = defaultValues.GetType().GetProperty(prop.Name).GetValue(defaultValues, null);
                if (defaultValue != targetValue)
                    targetProp.SetValue(this, defaultValue);
            }
        }

        return Result.Ok();
    }

    public TModel Update(TModel sourceSettings)
    {
        var hasChanged = false;

        foreach (var prop in typeof(TModel).GetProperties())
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
            EmitModuleHasChanged(GetValues());

        return GetValues();
    }

    #endregion

    #region Private Methods

    protected void EmitModuleHasChanged(TModel module)
    {
        _moduleUpdatedSubject.OnNext(module);
    }

    protected Result<JsonElement> GetJsonSettingsModule(JsonElement jsonElement)
    {
        if (jsonElement.TryGetProperty(Name, out var rootSettingsModule))
            return Result.Ok(rootSettingsModule);

        return Result.Fail($"Could not find settings module {Name} in config file").LogError();
    }

    #endregion
}