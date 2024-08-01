using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Settings.Contracts;

public record BaseSettingsModule<TModel>
    where TModel : class
{
    protected BaseSettingsModule()
    {
        _moduleUpdatedSubject = new BehaviorSubject<TModel>((this as TModel)!);
    }

    private readonly BehaviorSubject<TModel> _moduleUpdatedSubject;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _moduleUpdatedSubject.OnNext((this as TModel)!);
    }

    /// <summary>
    /// This has to be a public method otherwise it will be marked as a JSON serializable property or used to compare with in assertions.
    /// </summary>
    /// <returns></returns>
    [JsonIgnore]
    public IObservable<TModel> HasChanged => _moduleUpdatedSubject.AsObservable();

    protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (Equals(field, value))
            return;

        field = value;
        OnPropertyChanged(propertyName);
    }
}
