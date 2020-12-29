using Newtonsoft.Json;
using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class DateTimeModel : BaseModel
    {
        #region Properties

        [JsonProperty("shortDateFormat", Required = Required.Always)]
        public string ShortDateFormat { get; set; } = "dd/MM/yyyy";

        [JsonProperty("longDateFormat", Required = Required.Always)]
        public string LongDateFormat { get; set; } = "EEEE, dd MMMM yyyy";

        [JsonProperty("timeFormat", Required = Required.Always)]
        public string TimeFormat { get; set; } = "HH:MM:ss";

        [JsonProperty("showRelativeDates", Required = Required.Always)]
        public bool ShowRelativeDates { get; set; } = true;

        #endregion Properties
    }
}