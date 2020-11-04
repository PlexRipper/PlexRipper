namespace PlexRipper.Domain
{
    public class PlexMovieDataPart : PlexMediaDataPart
    {
        #region Properties

        #endregion

        #region Relationships

        public PlexMovieData PlexMovieData { get; set; }

        public int PlexMovieDataId { get; set; }

        #endregion
    }
}