namespace PlexRipper.Domain;

public record DownloadTaskGeneric
{
    public int Id { get; init; }

    public int Key { get; init; }

    public PlexMediaType MediaType { get; init; }

    public DownloadTaskType DownloadTaskType { get; init; }

    public bool IsDownloadable { get; init; }

    public List<DownloadTaskGeneric> Children { get; init; }
}