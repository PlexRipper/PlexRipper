namespace PlexRipper.Domain;

public record DownloadTaskKey(DownloadTaskType Type, Guid Id, int PlexServerId, int PlexLibraryId);