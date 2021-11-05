using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentResultExtensions.lib;
using FluentResults;
using Logging;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using Shouldly;
using WireMock.Server;
using Xunit;

namespace DownloadManager.IntegrationTests.DownloadWorker
{
    public class DownloadWorkerTests
    {
        private readonly WireMockServer _server;

        private BaseContainer Container { get; }

        public DownloadWorkerTests()
        {
            Container = new BaseContainer();

            _server = Container.MockServer.GetPlexMockServer();

            Log.Debug($"Server running at: {_server.Urls[0]}");
        }

        private DownloadTask GetTestDownloadTask()
        {
            var uri = new Uri(_server.Urls[0]);
            var mediaFile = Container.MockServer.GetDefaultMovieMockMediaData();

            return new DownloadTask
            {
                MediaType = PlexMediaType.Movie,
                Key = 9999,
                Created = DateTime.Now,
                PlexServer = new()
                {
                    Id = 1,
                    Scheme = uri.Scheme,
                    Address = uri.Host,
                    Host = uri.Host,
                    Port = uri.Port,
                },
                PlexLibrary = new PlexLibrary
                {
                    Id = 1,
                },
                PlexServerId = 1,
                PlexLibraryId = 1,
                DownloadFolder = new FolderPath
                {
                    DirectoryPath = Container.PathSystem.RootDirectory,
                },
                DownloadFolderId = 1,
                DestinationFolder = new FolderPath
                {
                    DirectoryPath = Container.PathSystem.RootDirectory,
                },
                DestinationFolderId = 1,
            };
        }

        private PlexRipper.DownloadManager.DownloadClient.DownloadWorker GetDownloadWorker(MemoryStream memoryStream, int downloadSpeedLimitInKb = 0)
        {
            var _filesystem = new Mock<IFileSystem>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));
            var downloadWorkerTask = new DownloadWorkerTask(GetTestDownloadTask(), 0, 0, 6482740)
            {
                Id = 1,
            };
            return new PlexRipper.DownloadManager.DownloadClient.DownloadWorker(downloadWorkerTask, _filesystem.Object, Container.GetPlexRipperHttpClient, downloadSpeedLimitInKb);
        }

        [Fact]
        public async Task Start_ShouldDownloadValidFile_WhenValidUrlGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);
            var mediaFile = Container.MockServer.GetDefaultMovieMockMediaData();

            //Act
            downloadWorker.Start();
            await downloadWorker.DownloadProcessTask;

            //Assert
            mediaFile.Md5.ShouldBe(DataFormat.CalculateMD5(memoryStream));
        }

        [Fact]
        public void Start_ShouldHaveStatusDownloading_WhenStarted()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);

            //Act
            downloadWorker.Start();

            //Assert
            downloadWorker.DownloadWorkerTask.DownloadStatus.ShouldBe(DownloadStatus.Downloading);
        }

        [Fact]
        public async Task Stop_ShouldHaveStatusStopped_WhenStopped()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);

            //Act
            downloadWorker.Start();
            await Task.Delay(500);
            await downloadWorker.StopAsync();

            //Assert
            downloadWorker.DownloadWorkerTask.DownloadStatus.ShouldBe(DownloadStatus.Stopped);
        }

        [Fact]
        public async Task Start_ShouldHaveAValidDownloadWorkerTask_WhenDownloadWorkerStopped()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream, 1000);

            //Act
            downloadWorker.Start();
            await Task.Delay(2000);
            var lastUpdate = await downloadWorker.StopAsync();

            //Assert
            lastUpdate.Value.ShouldNotBeNull();
            lastUpdate.Value.DownloadStatus.ShouldBe(DownloadStatus.Stopped);

        }

        [Fact]
        public async Task Start_ShouldHaveAValidDownloadFile_WhenDownloadWorkerStoppedAndRestarted()
        {
            //Arrange
            var mediaFile = Container.MockServer.GetDefaultMovieMockMediaData();
            var memoryStream = new MemoryStream();

            var _filesystem = new Mock<IFileSystem>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            var downloadWorkerTask = new DownloadWorkerTask(GetTestDownloadTask(), 0, 0, mediaFile.ByteSize)
            {
                Id = 1,
            };
            var downloadWorker = new PlexRipper.DownloadManager.DownloadClient.DownloadWorker(downloadWorkerTask, _filesystem.Object, Container.GetPlexRipperHttpClient, 1000);

            //Act
            downloadWorker.Start();
            await Task.Delay(2000);
            var lastUpdate = await downloadWorker.StopAsync();
            lastUpdate.IsSuccess.ShouldBeTrue();

            //// Recreate another download worker with a cloned stream as the original got closed
            var downloadWorker2 = new PlexRipper.DownloadManager.DownloadClient.DownloadWorker(lastUpdate.Value, _filesystem.Object, Container.GetPlexRipperHttpClient, 1000);
            downloadWorker2.Start();
            await downloadWorker2.DownloadProcessTask;

            //Assert
            mediaFile.Md5.ShouldBe(DataFormat.CalculateMD5(memoryStream));
        }

        [Theory]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(2000)]
        public async Task Start_ShouldBeDownloadSpeedLimited_WhenDownloadSpeedLimitIsGiven(int downloadSpeedLimit)
        {
            //Arrange
            var marginOfErrorInKb = 100;
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream, downloadSpeedLimit);

            List<DownloadWorkerTask> downloadWorkerUpdates = new();
            downloadWorker.DownloadWorkerTaskUpdate.Subscribe(downloadWorkerUpdate => downloadWorkerUpdates.Add(downloadWorkerUpdate));

            //Act
            downloadWorker.Start();
            await downloadWorker.DownloadProcessTask;

            //Assert
            var average = downloadWorkerUpdates.Select(x => x.DownloadSpeed).Average() / 1024f;
            average.ShouldBeInRange(downloadSpeedLimit - marginOfErrorInKb, downloadSpeedLimit + marginOfErrorInKb);
        }
    }
}