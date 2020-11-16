using PlexRipper.Application.Settings.Models.Base;

namespace PlexRipper.Application.Settings.Models
{
    public class DateTimeModel : BaseModel
    {
        #region Properties

        public string ShortDateFormat { get; set; } = "dd/MM/yyyy";

        public string LongDateFormat { get; set; } = "EEEE, dd MMMM yyyy";

        public string TimeFormat { get; set; } = "HH:MM";

        public bool ShowRelativeDates { get; set; } = true;

        #endregion Properties
    }
}