namespace Reaparr.Settings.Contracts;

public record DownloadManagerSettingsModule
    : BaseSettingsModule<DownloadManagerSettingsModule>,
        IDownloadManagerSettings
{
    private int _downloadSegments = 4;

    private bool _keepCompletedInDownloadFolder = false;

    public static DownloadManagerSettingsModule Create() =>
        new() { DownloadSegments = 4, KeepCompletedInDownloadFolder = false };

    public required int DownloadSegments
    {
        get => _downloadSegments;
        set => SetProperty(ref _downloadSegments, value);
    }

    public required bool KeepCompletedInDownloadFolder
    {
        get => _keepCompletedInDownloadFolder;
        set => SetProperty(ref _keepCompletedInDownloadFolder, value);
    }
}
