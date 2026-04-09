namespace Reaparr.Environment;

public enum EnvironmentVariableKey
{
    IntegrationTestMode,
    Unmasked,
    LogEnvironmentVariables,
    LogLevel,
    Version,
    InformationalVersion,
    ReaparrPlatform,
    ReaparrDataPath,
    ReaparrConfigPath,
    ReaparrDownloadsPath,
    ReaparrMoviesPath,
    ReaparrTvShowsPath,
    ReaparrMusicPath,
    ReaparrPhotosPath,
    ReaparrOtherPath,
    ReaparrGamesPath,
    AuthHeaderTokenName,
    SeqUrl,
}

public static class EnvironmentVariableKeyExtensions
{
    public static string Value(this EnvironmentVariableKey key) =>
        key switch
        {
            EnvironmentVariableKey.IntegrationTestMode => "IntegrationTestMode",
            EnvironmentVariableKey.Unmasked => "UNMASKED",
            EnvironmentVariableKey.LogEnvironmentVariables => "LOG_ENV_VARS",
            EnvironmentVariableKey.LogLevel => "LOG_LEVEL",
            EnvironmentVariableKey.Version => "VERSION",
            EnvironmentVariableKey.InformationalVersion => "INFORMATIONAL_VERSION",
            EnvironmentVariableKey.ReaparrPlatform => "REAPARR_PLATFORM",
            EnvironmentVariableKey.ReaparrDataPath => "REAPARR_DATA_PATH",
            EnvironmentVariableKey.ReaparrConfigPath => "REAPARR_CONFIG_PATH",
            EnvironmentVariableKey.ReaparrDownloadsPath => "REAPARR_DOWNLOADS_PATH",
            EnvironmentVariableKey.ReaparrMoviesPath => "REAPARR_MOVIES_PATH",
            EnvironmentVariableKey.ReaparrTvShowsPath => "REAPARR_TV_SHOWS_PATH",
            EnvironmentVariableKey.ReaparrMusicPath => "REAPARR_MUSIC_PATH",
            EnvironmentVariableKey.ReaparrPhotosPath => "REAPARR_PHOTOS_PATH",
            EnvironmentVariableKey.ReaparrOtherPath => "REAPARR_OTHER_PATH",
            EnvironmentVariableKey.ReaparrGamesPath => "REAPARR_GAMES_PATH",
            EnvironmentVariableKey.AuthHeaderTokenName => "AUTH_HEADER_TOKEN",
            EnvironmentVariableKey.SeqUrl => "SEQ_URL",
            _ => throw new ArgumentOutOfRangeException(nameof(key), key, null),
        };
}
