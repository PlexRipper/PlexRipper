using Newtonsoft.Json;
using PlexRipper.Application.Common;
using PlexRipper.Settings.Models.Base;

namespace PlexRipper.Settings.Models
{
    public class DateTimeModel : BaseModel, IDateTimeModel
    {
        #region Properties

        [JsonProperty("shortDateFormat", Required = Required.Always)]
        public string ShortDateFormat { get; set; } = "dd/MM/yyyy";

        [JsonProperty("longDateFormat", Required = Required.Always)]
        public string LongDateFormat { get; set; } = "EEEE, dd MMMM yyyy";

        [JsonProperty("timeFormat", Required = Required.Always)]
        public string TimeFormat { get; set; } = "HH:MM:ss";

        [JsonProperty("timeZone", Required = Required.Always)]
        public string TimeZone { get; set; } = "UTC";

        [JsonProperty("showRelativeDates", Required = Required.Always)]
        public bool ShowRelativeDates { get; set; } = true;

        #endregion Properties
    }
}