namespace PlexRipper.Application;

public static class ApiRoutes
{
    public static string Base => "/api";

    public static string DownloadController => Base + "/" + "Download";

    public static string PlexAccountController => Base + "/" + "PlexAccount";

    public static string PlexServerController => Base + "/" + "PlexServer";

    public static string PlexLibraryController => Base + "/" + "PlexLibrary";

    public static string PlexServerConnectionController => Base + "/" + "PlexServerConnection";

    public static string FolderPathController => Base + "/" + "FolderPath";

    public static string NotificationController => Base + "/" + "Notification";

    public static string SettingsController => Base + "/" + "Settings";

    public static string PlexMediaController => Base + "/" + "PlexMedia";

    public static class Settings
    {
        public static string GetSettings => SettingsController;

        public static string UpdateSettings => SettingsController;

        public static string ResetDatabase => SettingsController + "ResetDb";
    }

    public static class Download
    {
        public static string GetDownloadTasks => DownloadController;

        public static string PostClearCompleted => DownloadController + "/clear";

        public static string PostDownloadMedia => DownloadController + "/download";

        public static string PostDeleteCommand => DownloadController + "/delete";

        public static string PostRestartCommand => DownloadController + "/restart";

        public static string PostStopCommand => DownloadController + "/stop";

        public static string GetStartCommand(Guid id) => DownloadController + "/start/" + id;

        public static string GetPauseCommand(Guid id) => DownloadController + "/pause/" + id;
    }
}