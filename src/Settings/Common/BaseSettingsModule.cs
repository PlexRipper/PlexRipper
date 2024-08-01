using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.Settings;

public abstract class BaseSettingsModule<TModel> : IBaseSettingsModule<TModel>
    where TModel : class
{
    #region Fields

    protected readonly ILog _log = LogManager.CreateLogInstance<TModel>();

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

    public TModel Reset() => Update(DefaultValues());

    public virtual TModel Update(TModel sourceSettings)
    {
        if (sourceSettings is null)
        {
            _log.Warning("Can not update settings module {Name} with source settings null", Name);
            return GetValues();
        }

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

    protected virtual void EmitModuleHasChanged(TModel module)
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
