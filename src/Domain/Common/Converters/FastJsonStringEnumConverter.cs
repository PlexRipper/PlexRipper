using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Logging;

namespace PlexRipper.Domain
{
    public class FastJsonStringEnumConverter : JsonConverterFactory
    {
        private readonly JsonNamingPolicy namingPolicy;

        private readonly bool allowIntegerValues;

        private readonly JsonStringEnumConverter baseConverter;

        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(DownloadStatus) ||
                   typeToConvert == typeof(FileSystemEntityType) ||
                   typeToConvert == typeof(FolderType) ||
                   typeToConvert == typeof(NotificationLevel) ||
                   typeToConvert == typeof(PlexMediaType) ||
                   typeToConvert == typeof(ViewMode);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(DownloadStatus))
            {
                return new DownloadStatusConverter();
            }
            else if (typeToConvert == typeof(FileSystemEntityType))
            {
                return new FileSystemEntityTypeConverter();
            }
            else if (typeToConvert == typeof(FolderType))
            {
                return new FolderTypeConverter();
            }
            else if (typeToConvert == typeof(NotificationLevel))
            {
                return new NotificationLevelConverter();
            }
            else if (typeToConvert == typeof(PlexMediaType))
            {
                return new PlexMediaTypeConverter();
            }
            else if (typeToConvert == typeof(ViewMode))
            {
                return new ViewModeConverter();
            }
            else
            {
                var ex = new NotSupportedException("CreateConverter got called on type that this converter factory doesn't support");
                Log.Error(ex);
                throw ex;
            }
        }
    }
}