using Reaparr.Environment;

namespace Reaparr.BaseTests;

public static partial class FakeData
{
    private static Faker<T> ApplyDownloadTaskBase<T>(this Faker<T> faker, DownloadTaskType downloadTaskType)
        where T : DownloadTaskBase
    {
        return faker
            .StrictMode(true)
            .Ignore(x => x.Id)
            .RuleFor(x => x.PlexId, _ => GetUniqueNumber())
            .RuleFor(x => x.Title, f => f.PlexMedia().MediaTitle(downloadTaskType))
            .RuleFor(x => x.FullTitle, (_, x) => x.Title)
            .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Queued)
            .RuleFor(x => x.CreatedAt, _ => DateTime.UtcNow)
            .Ignore(x => x.PlexServerId)
            .Ignore(x => x.PlexServer)
            .Ignore(x => x.PlexLibraryId)
            .Ignore(x => x.PlexLibrary);
    }

    private static Faker<T> ApplyDownloadTaskParentBase<T>(this Faker<T> faker, DownloadTaskType downloadTaskType)
        where T : DownloadTaskParentBase
    {
        return faker
            .ApplyDownloadTaskBase(downloadTaskType)
            .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
            .Ignore(x => x.FileTransferSpeed)
            .Ignore(x => x.DataReceived)
            .Ignore(x => x.FileDataTransferred)
            .Ignore(x => x.DownloadSpeed);
    }

    private static Faker<T> ApplyDownloadTaskFileBase<T>(this Faker<T> faker, DownloadTaskType downloadTaskType)
        where T : DownloadTaskFileBase
    {
        return faker
            .ApplyDownloadTaskBase(downloadTaskType)
            .Ignore(x => x.HashId)
            .Ignore(x => x.DataReceived)
            .Ignore(x => x.DownloadSpeed)
            .Ignore(x => x.FileTransferSpeed)
            .Ignore(x => x.FileDataTransferred)
            .Ignore(x => x.CurrentFileTransferBytesOffset)
            .Ignore(x => x.DestinationFolderPathId)
            .RuleFor(
                x => x.Quality,
                f => f.PickRandom(VideoQuality.SD, VideoQuality.HD, VideoQuality.FullHD, VideoQuality.UHD_4K)
            )
            .RuleFor(
                x => x.FileName,
                (_, x) => $"{x.MediaType.ToPlexMediaTypeString()}-{x.Title.SanitizeFolderName()}.[{x.Quality}].file.mp4"
            )
            .RuleFor(x => x.FileLocationUrl, _ => DownloadFileUrl)
            .RuleFor(
                x => x.DirectoryMeta,
                (_, x) =>
                    new DownloadTaskDirectory
                    {
                        DestinationRootPath = x.MediaType.ToDefaultDestinationLocation(),
                        DownloadRootPath = PathProvider.DefaultDownloadsDestinationFolder,
                        MovieFolder = x.Title,
                        TvShowFolder = string.Empty,
                        SeasonFolder = string.Empty,
                        KeepCompletedInDownloadFolder = false,
                    }
            )
            .Ignore(x => x.DownloadWorkerTasks);
    }
}
