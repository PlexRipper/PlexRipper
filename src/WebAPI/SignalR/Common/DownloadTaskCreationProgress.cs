namespace PlexRipper.WebAPI.SignalR.Common;

public class DownloadTaskCreationProgress
{
    public decimal Percentage { get; set; }


    public int Current { get; set; }


    public int Total { get; set; }

    /// <summary>
    /// Has the library finished refreshing.
    /// </summary>

    public bool IsComplete { get; set; }
}