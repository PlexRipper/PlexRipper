namespace Reaparr.Settings.Contracts;

public record DownloadManagerSettingsModule
    : BaseSettingsModule<DownloadManagerSettingsModule>,
        IDownloadManagerSettings
{
    private int _downloadSegments = 4;

    public static DownloadManagerSettingsModule Create() => new() { DownloadSegments = 4 };

    public required int DownloadSegments
    {
        get => _downloadSegments;
        set => SetProperty(ref _downloadSegments, value);
    }
}
