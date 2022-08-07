namespace PlexRipper.BaseTests.Asserts;

public static class ShouldDownloadTask
{
    /// <summary>
    /// Tests the source against the target to ensure both are the same.
    /// </summary>
    /// <param name="source">The result of test.</param>
    /// <param name="target">The initial input.</param>
    public static void ShouldTvShow(DownloadTask source, PlexTvShow target)
    {
        source.ShouldNotBeNull();
        source.Key.ShouldBe(target.Key);
        source.Title.ShouldBe(target.Title);
        source.FullTitle.ShouldBe(target.FullTitle);

        source.DataReceived.ShouldBe(0);
        source.DataTotal.ShouldBeGreaterThan(0);

        source.Year.ShouldBe(target.Year);
        source.Parent.ShouldBeNull();
        source.ParentId.ShouldBeNull();

        source.PlexLibrary.ShouldNotBeNull();
        source.PlexLibraryId.ShouldBe(target.PlexLibraryId);
        source.PlexServer.ShouldNotBeNull();
        source.PlexServerId.ShouldBe(target.PlexServerId);

        source.MediaType.ShouldBe(target.Type);
        source.FileLocationUrl.ShouldBeNull();

        source.DownloadTaskType.ShouldBe(DownloadTaskType.TvShow);
        source.DownloadStatus.ShouldBe(DownloadStatus.Queued);
        source.Created.ShouldBeGreaterThan(DateTime.MinValue);
        source.Created.ShouldBeLessThan(DateTime.UtcNow);

        source.Children.Count.ShouldBeGreaterThan(0);

        foreach (var sourceDownloadTask in source.Children)
        {
            var targetSeason = target.Seasons.FirstOrDefault(x => x.Key == sourceDownloadTask.Key);
            ShouldSeason(sourceDownloadTask, targetSeason);
        }
    }

    public static void ShouldSeason(DownloadTask source, PlexTvShowSeason target)
    {
        target.ShouldNotBeNull();

        source.Key.ShouldBe(target.Key);
        source.Title.ShouldBe(target.Title);
        source.FullTitle.ShouldBe(target.FullTitle);

        source.DataReceived.ShouldBe(0);
        source.DataTotal.ShouldBeGreaterThan(0);

        source.Year.ShouldBe(target.Year);

        source.PlexLibraryId.ShouldBe(source.PlexLibraryId);
        source.PlexLibrary.ShouldNotBeNull();
        source.PlexServerId.ShouldBe(source.PlexServerId);
        source.PlexServer.ShouldNotBeNull();

        source.MediaType.ShouldBe(target.Type);
        source.FileLocationUrl.ShouldBeNull();

        source.DownloadTaskType.ShouldBe(DownloadTaskType.Season);
        source.DownloadStatus.ShouldBe(DownloadStatus.Queued);
        source.Created.ShouldBeGreaterThan(DateTime.MinValue);
        source.Created.ShouldBeLessThan(DateTime.UtcNow);

        source.Children.Count.ShouldBeGreaterThan(0);

        foreach (var episodeDownloadTask in source.Children)
        {
            var targetEpisode = target.Episodes.FirstOrDefault(x => x.Key == episodeDownloadTask.Key);
            ShouldEpisode(episodeDownloadTask, targetEpisode);
        }
    }

    public static void ShouldEpisode(DownloadTask source, PlexTvShowEpisode target)
    {
        target.ShouldNotBeNull();

        source.Key.ShouldBe(target.Key);
        source.Title.ShouldBe(target.Title);
        source.FullTitle.ShouldBe(target.FullTitle);

        source.DataReceived.ShouldBe(0);
        source.DataTotal.ShouldBe(target.MediaSize);

        source.Year.ShouldBe(target.Year);

        source.PlexLibrary.ShouldNotBeNull();
        source.PlexLibraryId.ShouldBe(source.PlexLibraryId);
        source.PlexServer.ShouldNotBeNull();
        source.PlexServerId.ShouldBe(source.PlexServerId);

        source.MediaType.ShouldBe(target.Type);
        source.FileLocationUrl.ShouldNotBeEmpty();
        source.FileName.ShouldNotBeEmpty();

        source.DownloadTaskType.ShouldBe(DownloadTaskType.Episode);
        source.DownloadStatus.ShouldBe(DownloadStatus.Queued);
        source.Created.ShouldBeGreaterThan(DateTime.MinValue);
        source.Created.ShouldBeLessThan(DateTime.UtcNow);

        var plexMediaDataParts = target.EpisodeData.First().Parts;
        if (plexMediaDataParts.Count > 1)
        {
            source.Children.Count.ShouldBeGreaterThan(0);
            for (var m = 0; m < target.EpisodeData.Count; m++)
            {
                var tvShowEpisodeData = target.EpisodeData[m];

                for (var l = 0; l < plexMediaDataParts.Count; l++)
                {
                    var episodeDataPartDownloadTask = source.Children[m + l];

                    episodeDataPartDownloadTask.DownloadTaskType.ShouldBe(DownloadTaskType.EpisodePart);
                    episodeDataPartDownloadTask.DownloadStatus.ShouldBe(DownloadStatus.Queued);
                    episodeDataPartDownloadTask.Created.ShouldBeGreaterThan(DateTime.MinValue);
                    episodeDataPartDownloadTask.Created.ShouldBeLessThan(DateTime.UtcNow);
                    episodeDataPartDownloadTask.DownloadWorkerTasks.ShouldBeEmpty();
                }
            }
        }
    }
}