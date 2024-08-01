namespace Settings.Contracts;

public record DownloadManagerSettingsModel : BaseSettingsModel<DownloadManagerSettingsModel>, IDownloadManagerSettings
{
    private int _downloadSegments = 4;

    public int DownloadSegments
    {
        get => _downloadSegments;
        set => SetProperty(ref _downloadSegments, value);
    }
}
