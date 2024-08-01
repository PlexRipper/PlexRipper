using System.ComponentModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Settings.Contracts;

public record BaseSettingsModel<TModel> : INotifyPropertyChanged
    where TModel : class
{
    private readonly Subject<TModel> _moduleUpdatedSubject = new();

    public void Text()
    {
        Console.WriteLine("BaseSettingsModel");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _moduleUpdatedSubject.OnNext((this as TModel)!);
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// This has to be a public method otherwise it will be marked as a JSON serializable property or used to compare with in assertions.
    /// </summary>
    /// <returns></returns>
    [JsonIgnore]
    public IObservable<TModel> HasChanged => _moduleUpdatedSubject.AsObservable();

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
    {
        if (Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
