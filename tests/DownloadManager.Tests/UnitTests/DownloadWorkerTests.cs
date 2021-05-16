using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using MockPlexApiServer;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
using Shouldly;
using WireMock.Server;
using Xunit;

namespace DownloadManager.Tests.UnitTests
{
    public class DownloadWorkerTests
    {
        private readonly WireMockServer _server;

        private BaseContainer Container { get; }

        public DownloadWorkerTests()
        {
            Container = new BaseContainer();

            _server = MockServer.GetPlexMockServer();

            Log.Debug($"Server running at: {_server.Urls[0]}");
        }

        private DownloadTask GetTestDownloadTask()
        {
            var uri = new Uri(_server.Urls[0]);
            var mediaFile = MockServer.GetDefaultMovieMockMediaData();

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
                ServerToken = "AAABBBCCCDDDEEEFFFGGG",
                MetaData = new()
                {
                    MovieTitle = mediaFile.ParentFolderName,
                    MediaData = new List<PlexMediaData>
                    {
                        new()
                        {
                            Parts = new List<PlexMediaDataPart>
                            {
                                new()
                                {
                                    Size = mediaFile.ByteSize,
                                    ObfuscatedFilePath = mediaFile.RelativeUrl,
                                    File = mediaFile.FileName,
                                },
                            },
                        },
                    },
                },
                DownloadFolder = new FolderPath
                {
                    DirectoryPath = FileSystemPaths.RootDirectory,
                },
                DownloadFolderId = 1,
                DestinationFolder = new FolderPath
                {
                    DirectoryPath = FileSystemPaths.RootDirectory,
                },
                DestinationFolderId = 1,
            };
        }

        private DownloadWorker GetDownloadWorker(MemoryStream memoryStream, int downloadSpeedLimitInKb = 0)
        {
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));
            var downloadWorkerTask = new DownloadWorkerTask(GetTestDownloadTask(), 0, 0, 6482740)
            {
                Id = 1,
            };
            return new DownloadWorker(downloadWorkerTask, _filesystem.Object, Container.GetPlexRipperHttpClient, downloadSpeedLimitInKb);
        }

        [Fact]
        public async Task Start_ShouldDownloadValidFile_WhenValidUrlGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);
            var mediaFile = MockServer.GetDefaultMovieMockMediaData();

            //Act
            downloadWorker.Start();
            await downloadWorker.DownloadProcessTask;

            //Assert
            mediaFile.Md5.Should().Be(DataFormat.CalculateMD5(memoryStream));
        }

        [Fact]
        public async Task Start_ShouldHaveStatusDownloading_WhenStarted()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);

            //Act
            downloadWorker.Start();

            //Assert
            downloadWorker.LastDownloadWorkerUpdate.DownloadStatus.Should().Be(DownloadStatus.Downloading);
        }

        [Fact]
        public async Task Start_ShouldHaveStatusStopped_WhenStopped()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);

            //Act
            downloadWorker.Start();
            await Task.Delay(500);
            await downloadWorker.Stop();

            //Assert
            downloadWorker.LastDownloadWorkerUpdate.DownloadStatus.Should().Be(DownloadStatus.Stopped);
        }

        [Fact]
        public async Task Start_ShouldHaveMoreThan0Updates_WhenValidDownload()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);

            List<DownloadWorkerUpdate> downloadWorkerUpdates = new();
            downloadWorker.DownloadWorkerUpdate.Subscribe(downloadWorkerUpdate => downloadWorkerUpdates.Add(downloadWorkerUpdate));

            //Act
            downloadWorker.Start();
            await downloadWorker.DownloadProcessTask;

            //Assert
            downloadWorkerUpdates.Count.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(2000)]
        [InlineData(100)]
        public async Task Start_ShouldBeDownloadSpeedLimited_WhenDownloadSpeedLimitIsGiven(int downloadSpeedLimit)
        {
            //Arrange
            var marginOfErrorInKb = 100;
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream, downloadSpeedLimit);

            List<DownloadWorkerUpdate> downloadWorkerUpdates = new();
            downloadWorker.DownloadWorkerUpdate.Subscribe(downloadWorkerUpdate => downloadWorkerUpdates.Add(downloadWorkerUpdate));

            //Act
            downloadWorker.Start();
            await downloadWorker.DownloadProcessTask;

            //Assert
            (downloadWorkerUpdates.Last().DownloadSpeedAverage / 1024f)
                .ShouldBeInRange(downloadSpeedLimit - marginOfErrorInKb, downloadSpeedLimit + marginOfErrorInKb);
        }
    }
}