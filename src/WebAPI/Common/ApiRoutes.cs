using System;
using PlexRipper.WebAPI.Controllers;

namespace PlexRipper.WebAPI.Common
{
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
    }
}