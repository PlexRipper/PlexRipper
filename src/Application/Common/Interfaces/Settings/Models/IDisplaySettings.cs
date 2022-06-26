namespace PlexRipper.Application
{
    public interface IDisplaySettings
    {
        ViewMode TvShowViewMode { get; set; }

        ViewMode MovieViewMode { get; set; }
    }
}