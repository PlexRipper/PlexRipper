namespace Reaparr.Environment;

public interface IAppRuntimeInfo
{
    string? DataPath { get; }

    string? ConfigPath { get; }

    string? DownloadsPath { get; }

    string? MoviesPath { get; }

    string? TvShowsPath { get; }

    string? MusicPath { get; }

    string? PhotosPath { get; }

    string? OtherPath { get; }

    string? GamesPath { get; }
}
