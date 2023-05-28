using PlexRipper.WebAPI.Controllers;

namespace PlexRipper.WebAPI.Common;

public static class ApiRoutes
{
    public static string Base => "/api";

    public static class Settings
    {
        public static string Controller => Base + "/" + nameof(SettingsController).Replace("Controller", string.Empty);

        public static string GetSettings => Controller;

        public static string UpdateSettings => Controller;

        public static string ResetDatabase => Controller + "ResetDb";
    }

    public static class Download
    {
        public static string Controller => Base + "/" + nameof(DownloadController).Replace("Controller", string.Empty);

        public static string GetDownloadTasks => Controller;

        public static string PostClearCompleted => Controller + "/clear";

        public static string PostDownloadMedia => Controller + "/download";

        public static string PostPauseCommand => Controller + "/pause";

        public static string PostDeleteCommand => Controller + "/delete";

        public static string PostRestartCommand => Controller + "/restart";

        public static string PostStopCommand => Controller + "/stop";

        public static string GetStartCommand(int id)
        {
            return Controller + "/start/" + id;
        }
    }

    public static class Account
    {
        public static string Controller => Base + "/" + nameof(PlexAccountController).Replace("Controller", string.Empty);

        public static string PostValidate => Controller + "/validate";

        public static string PostCreateAccount => Controller + "/";
    }
}