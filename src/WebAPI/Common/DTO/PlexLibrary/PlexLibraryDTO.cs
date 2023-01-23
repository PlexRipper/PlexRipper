using PlexRipper.WebAPI.Common.DTO.FolderPath;

namespace PlexRipper.WebAPI.Common.DTO;

public sealed class PlexLibraryDTO
{
    public int Id { get; set; }


    public string Key { get; set; }


    public string Title { get; set; }


    public PlexMediaType Type { get; set; }


    public DateTime UpdatedAt { get; set; }


    public DateTime CreatedAt { get; set; }


    public DateTime ScannedAt { get; set; }


    public DateTime SyncedAt { get; set; }


    public bool Outdated { get; set; }


    public Guid Uuid { get; set; }


    public long MediaSize { get; set; }


    public int LibraryLocationId { get; set; }


    public string LibraryLocationPath { get; set; }


    public FolderPathDTO DefaultDestination { get; set; }


    public int DefaultDestinationId { get; set; }


    public int PlexServerId { get; set; }


    public int Count { get; set; }


    public int SeasonCount { get; set; }


    public int EpisodeCount { get; set; }


    public List<PlexMediaDTO> Movies { get; set; }


    public List<PlexMediaDTO> TvShows { get; set; }


    public List<DownloadTaskDTO> DownloadTasks { get; set; }
}