using System;

namespace PlexRipper.Domain.Common.API
{
    public static class ApiHelper
    {
        public static Uri ChangePath(this Uri uri, string path, params string[] args)
        {
            var builder = new UriBuilder(uri);

            if (args != null && args.Length > 0)
            {
                builder.Path = builder.Path + string.Format(path, args);
            }
            else
            {
                builder.Path += path;
            }
            return builder.Uri;
        }
        public static Uri ChangePath(this Uri uri, string path)
        {
            return ChangePath(uri, path, null);
        }

        public static Uri AddQueryParameter(this Uri uri, string parameter, string value)
        {
            if (string.IsNullOrEmpty(parameter) || string.IsNullOrEmpty(value)) return uri;
            var builder = new UriBuilder(uri);
            var startingTag = string.Empty;
            var hasQuery = false;
            if (string.IsNullOrEmpty(builder.Query))
            {
                startingTag = "?";
            }
            else
            {
                hasQuery = true;
                startingTag = builder.Query.Contains("?") ? "&" : "?";
            }

            builder.Query = hasQuery
                ? $"{builder.Query}{startingTag}{parameter}={value}"
                : $"{startingTag}{parameter}={value}";
            return builder.Uri;
        }
    }
}
