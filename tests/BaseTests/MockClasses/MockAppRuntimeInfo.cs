namespace Reaparr.BaseTests;

public class MockAppRuntimeInfo : IAppRuntimeInfo
{
    public MockAppRuntimeInfo() { }

    public string? DataPath { get; }
    
    public string? ConfigPath { get; }
    
    public string? DownloadsPath { get; }
    
    public string? MoviesPath { get; }
    
    public string? TvShowsPath { get; }
    
    public string? MusicPath { get; }
    
    public string? PhotosPath { get; }
    
    public string? OtherPath { get; }
    
    public string? GamesPath { get; }
}
