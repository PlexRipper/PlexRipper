namespace PlexRipper.Application;

public static class ApiRoutes
{
    public static string Base => "/api";

    public static class Settings
    {
        public static string Controller => Base + "/" + "".Replace("Controller", string.Empty);

        public static string GetSettings => Controller;

        public static string UpdateSettings => Controller;

        public static string ResetDatabase => Controller + "ResetDb";
    }

    public static string DownloadController => Base + "/" + "Download";

    public static string PlexAccountController => Base + "/" + "PlexAccount";

    public static string PlexServerController => Base + "/" + "PlexServer";

    public static string PlexLibraryController => Base + "/" + "PlexLibrary";

    public static string PlexServerConnectionController => Base + "/" + "PlexServerConnection";

    public static string FolderPathController => Base + "/" + "FolderPath";

    public static string NotificationController => Base + "/" + "Notification";

    public static string SettingsController => Base + "/" + "Settings";

    public static string PlexMediaController => Base + "/" + "PlexMedia";

    public static class Download
    {
        public static string Controller => Base + "/" + nameof(DownloadController).Replace("Controller", string.Empty);

        public static string GetDownloadTasks => Controller;

        public static string PostClearCompleted => Controller + "/clear";

        public static string PostDownloadMedia => Controller + "/download";

        public static string PostDeleteCommand => Controller + "/delete";

        public static string PostRestartCommand => Controller + "/restart";

        public static string PostStopCommand => Controller + "/stop";

        public static string GetStartCommand(Guid id) => Controller + "/start/" + id;

        public static string GetPauseCommand(Guid id) => Controller + "/pause/" + id;
    }
}