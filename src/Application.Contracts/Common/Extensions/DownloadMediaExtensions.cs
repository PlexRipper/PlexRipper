using PlexRipper.Domain;

namespace Application.Contracts;

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

        foreach (var plexServerId in typedList.Select(x => x.PlexServerId).Distinct())
            result.Add(
                new DownloadMediaDTO
                {
                    MediaIds = typedList
                        .FindAll(x => x.PlexServerId == plexServerId)
                        .SelectMany(x => x.MediaIds)
                        .ToList(),
                    PlexServerId = plexServerId,
                    Type = type,
                }
            );

        return result;
    }

    public static List<DownloadMediaDTO> MergeAndGroupList(this List<DownloadMediaDTO> downloadMediaList)
    {
        return downloadMediaList
            .GroupBy(x => new
            {
                x.PlexServerId,
                x.PlexLibraryId,
                x.Type
            })
            .Select(group => new DownloadMediaDTO
            {
                PlexServerId = group.Key.PlexServerId,
                PlexLibraryId = group.Key.PlexLibraryId,
                MediaIds = group.SelectMany(x => x.MediaIds).Distinct().ToList(),
                Type = group.Key.Type,
            })
            .ToList();
    }
}
