using Data.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Data.UnitTests.Entities;

public class DownloadTaskExtensions_Calculate_UnitTests : BaseUnitTest
{
    public DownloadTaskExtensions_Calculate_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldUpdateDownloadProgressAcrossAllLayers_WhenCallingCalculateOnRootDownloadTask()
    {
        // Arrange
        await SetupDatabase(config =>
        {
            config.TvShowDownloadTasksCount = 5;
            config.TvShowSeasonDownloadTasksCount = 5;
            config.TvShowEpisodeDownloadTasksCount = 5;
        });

        var downloadTasks = await IDbContext.DownloadTaskTvShow.IncludeAll().ToListAsync();

        foreach (var downloadTaskTvShow in downloadTasks)
        foreach (var downloadTaskTvShowSeason in downloadTaskTvShow.Children)
        foreach (var downloadTaskTvShowEpisode in downloadTaskTvShowSeason.Children)
        foreach (var downloadTaskTvShowEpisodeFile in downloadTaskTvShowEpisode.Children)
        {
            downloadTaskTvShowEpisodeFile.DownloadSpeed = 20;
            downloadTaskTvShowEpisodeFile.FileTransferSpeed = 100;
            downloadTaskTvShowEpisodeFile.DataReceived = 5000;
            downloadTaskTvShowEpisodeFile.DataTotal = 10000;
            downloadTaskTvShowEpisodeFile.Percentage = 50;
        }

        // Act
        foreach (var downloadTaskTvShow in downloadTasks)
            downloadTaskTvShow.Calculate();

        // Assert
        downloadTasks.ShouldAllBe(x => x.DownloadSpeed == 20);
        downloadTasks.ShouldAllBe(x => x.FileTransferSpeed == 100);
        downloadTasks.ShouldAllBe(x => x.DataReceived == 125000);
        downloadTasks.ShouldAllBe(x => x.DataTotal == 250000);
        downloadTasks.ShouldAllBe(x => x.Percentage == 50);

        downloadTasks.SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadSpeed == 20);
        downloadTasks.SelectMany(x => x.Children).ShouldAllBe(x => x.FileTransferSpeed == 100);
        downloadTasks.SelectMany(x => x.Children).ShouldAllBe(x => x.DataReceived == 25000);
        downloadTasks.SelectMany(x => x.Children).ShouldAllBe(x => x.DataTotal == 50000);
        downloadTasks.SelectMany(x => x.Children).ShouldAllBe(x => x.Percentage == 50);

        downloadTasks.SelectMany(x => x.Children).SelectMany(x => x.Children).ShouldAllBe(x => x.DownloadSpeed == 20);
        downloadTasks
            .SelectMany(x => x.Children)
            .SelectMany(x => x.Children)
            .ShouldAllBe(x => x.FileTransferSpeed == 100);
        downloadTasks.SelectMany(x => x.Children).SelectMany(x => x.Children).ShouldAllBe(x => x.DataReceived == 5000);
        downloadTasks.SelectMany(x => x.Children).SelectMany(x => x.Children).ShouldAllBe(x => x.DataTotal == 10000);
        downloadTasks.SelectMany(x => x.Children).SelectMany(x => x.Children).ShouldAllBe(x => x.Percentage == 50);
    }
}
