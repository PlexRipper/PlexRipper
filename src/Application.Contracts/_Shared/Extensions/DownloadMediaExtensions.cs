namespace Reaparr.Application.Contracts;

public static class DownloadMediaExtensions
{
    /// <summary>
    /// Merges the <see cref="DownloadMediaDTO"/>s by PlexServerId and <see cref="PlexMediaType"/>.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<DownloadMediaDTO> Merge(this List<DownloadMediaDTO> source, PlexMediaType type)
    {
        var typedList = source.FindAll(x => x.Type == type);

        var result = new List<DownloadMediaDTO>();

        foreach (
            var (plexServerId, selectionKind) in typedList.Select(x => (x.PlexServerId, x.SelectionKind)).Distinct()
        )
        {
            var plexLibraryIds = typedList.Select(x => x.PlexLibraryId).Distinct().ToList();
            result.Add(
                new DownloadMediaDTO
                {
                    MediaIds = typedList
                        .FindAll(x => x.PlexServerId == plexServerId && x.SelectionKind == selectionKind)
                        .SelectMany(x => x.MediaIds)
                        .ToList(),
                    PlexServerId = plexServerId,
                    PlexLibraryId = plexLibraryIds.Count > 1 ? 0 : plexLibraryIds.FirstOrDefault(),
                    Type = type,
                    Qualities = typedList
                        .FindAll(x => x.PlexServerId == plexServerId && x.SelectionKind == selectionKind)
                        .SelectMany(x => x.Qualities)
                        .ToList(),
                    KeepCompletedInDownloadFolder = typedList
                        .FindAll(x => x.PlexServerId == plexServerId && x.SelectionKind == selectionKind)
                        .Any(x => x.KeepCompletedInDownloadFolder),
                    SelectionKind = selectionKind,
                }
            );
        }

        return result;
    }

    public static List<DownloadMediaDTO> MergeAndGroupList(this List<DownloadMediaDTO> downloadMediaList)
    {
        return downloadMediaList
            .GroupBy(x => new
            {
                x.PlexServerId,
                x.PlexLibraryId,
                x.Type,
                x.SelectionKind,
            })
            .Select(group => new DownloadMediaDTO
            {
                PlexServerId = group.Key.PlexServerId,
                PlexLibraryId = group.Key.PlexLibraryId,
                MediaIds = group.SelectMany(x => x.MediaIds).Distinct().ToList(),
                Type = group.Key.Type,
                SelectionKind = group.Key.SelectionKind,
                Qualities = group.SelectMany(x => x.Qualities).ToList(),
                KeepCompletedInDownloadFolder = group.Any(x => x.KeepCompletedInDownloadFolder),
            })
            .ToList();
    }
}
