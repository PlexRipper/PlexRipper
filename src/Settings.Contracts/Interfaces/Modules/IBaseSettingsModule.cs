namespace Settings.Contracts;

public interface IBaseSettingsModule<TModel>
    where TModel : class
{
    public string Name { get; }

    public TModel Update(TModel sourceSettings);

    public TModel Reset();

    public IObservable<TModel> ModuleHasChanged { get; }

    public TModel GetValues();

    public TModel DefaultValues();
}
