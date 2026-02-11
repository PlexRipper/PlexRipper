namespace Reaparr.Settings.Contracts;

public interface IBaseSettingsModule<TModel>
    where TModel : class
{
    /// <summary>
    /// Resets this settings module to the values produced by <paramref name="defaultFactory"/>.
    /// </summary>
    /// <param name="defaultFactory">A factory that returns the default state for this module.</param>
    void Reset(Func<TModel> defaultFactory);
}
