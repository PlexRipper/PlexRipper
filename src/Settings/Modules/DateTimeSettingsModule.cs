using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class DateTimeSettingsModule : BaseSettingsModule<IDateTimeSettings>, IDateTimeSettingsModule
    {
        #region Properties

        public override string Name => "DateTimeSettings";

        public string LongDateFormat { get; set; } = "EEEE, dd MMMM yyyy";

        public string ShortDateFormat { get; set; } = "dd/MM/yyyy";

        public bool ShowRelativeDates { get; set; } = true;

        public string TimeFormat { get; set; } = "HH:mm:ss";

        public string TimeZone { get; set; } = "UTC";

        #endregion

        #region Public Methods

        public void Reset()
        {
            Update(new DateTimeSettingsModule());
        }

        /// <inheritdoc/>
        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var dateTimeSettings = jsonSettings.Value;

            if (dateTimeSettings.TryGetProperty(nameof(ShortDateFormat), out JsonElement shortDateFormat))
            {
                ShortDateFormat = shortDateFormat.GetString();
            }

            if (dateTimeSettings.TryGetProperty(nameof(LongDateFormat), out JsonElement longDateFormat))
            {
                LongDateFormat = longDateFormat.GetString();
            }

            if (dateTimeSettings.TryGetProperty(nameof(TimeFormat), out JsonElement timeFormat))
            {
                TimeFormat = timeFormat.GetString();
            }

            if (dateTimeSettings.TryGetProperty(nameof(TimeZone), out JsonElement timeZone))
            {
                TimeZone = timeZone.GetString();
            }

            if (dateTimeSettings.TryGetProperty(nameof(ShowRelativeDates), out JsonElement showRelativeDates))
            {
                ShowRelativeDates = showRelativeDates.GetBoolean();
            }

            return Result.Ok();
        }

        public override IDateTimeSettings GetValues()
        {
            return new DateTimeSettings
            {
                TimeFormat = TimeFormat,
                TimeZone = TimeZone,
                LongDateFormat = LongDateFormat,
                ShortDateFormat = ShortDateFormat,
                ShowRelativeDates = ShowRelativeDates,
            };
        }

  #endregion
    }
}