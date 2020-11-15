using System;

namespace PlexRipper.Domain
{
    public class PlexServerStatus : BaseEntity
    {
        #region Properties

        public int StatusCode { get; set; }

        public bool IsSuccessful { get; set; }

        public string StatusMessage { get; set; }

        public DateTime LastChecked { get; set; }

        #endregion

        #region Relationships

        public PlexServer PlexServer { get; set; }

        public int PlexServerId { get; set; }

        #endregion
    }
}