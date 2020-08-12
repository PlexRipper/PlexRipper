namespace PlexRipper.Domain.Types
{
    public class LibraryProgress
    {
        public int Id { get; set; }

        public decimal Percentage { get; set; }

        public int Received { get; set; }

        public int Total { get; set; }

        /// <summary>
        /// Is the library currently refreshing the data from the external PlexServer of from our own local database.
        /// </summary>
        public bool IsRefreshing { get; set; }

        /// <summary>
        /// Has the library finished refreshing
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
