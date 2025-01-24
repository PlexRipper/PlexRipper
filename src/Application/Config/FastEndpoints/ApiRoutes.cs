namespace PlexRipper.Application;

public static class ApiRoutes
{
    public static string Base => "/api";

    public static string DownloadController => Base + "/" + "Download";

    public static string DebugController => Base + "/" + "Debug";

    public static string PlexAccountController => Base + "/" + "PlexAccount";

    public static string BackgroundJobsController => Base + "/" + "BackgroundJobs";

    public static string PlexServerController => Base + "/" + "PlexServer";

    public static string PlexLibraryController => Base + "/" + "PlexLibrary";

    public static string PlexServerConnectionController => Base + "/" + "PlexServerConnection";

    public static string FolderPathController => Base + "/" + "FolderPath";

    public static string NotificationController => Base + "/" + "Notification";

    public static string SettingsController => Base + "/" + "Settings";

    public static string PlexMediaController => Base + "/" + "PlexMedia";

    public static string AuthenticatedController => Base + "/" + "Authentication";

    public static string LoginEndpoint => AuthenticatedController + "/login";

    public static string LogOutEndpoint => AuthenticatedController + "/logout";
}
