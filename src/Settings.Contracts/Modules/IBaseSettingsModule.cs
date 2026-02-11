namespace Reaparr.Settings.Contracts;

public interface IBaseSettingsModule<TModel>
    where TModel : BaseSettingsModule<TModel>, IBaseSettingsModule<TModel>
{
    static abstract TModel Create();

    /// <summary>
    /// Resets this settings module to its default values.
    /// </summary>
    void Reset() => ((BaseSettingsModule<TModel>)this).Update(TModel.Create());
}
