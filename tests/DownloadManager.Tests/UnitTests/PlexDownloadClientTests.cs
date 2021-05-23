using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
using Shouldly;
using WireMock.Server;
using Xunit;
using Xunit.Abstractions;

namespace DownloadManager.Tests.UnitTests
{
    public class PlexDownloadClientTests
    {
        private BaseContainer Container { get; }

        private readonly WireMockServer _server;

        public PlexDownloadClientTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
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

        private Result<PlexDownloadClient> CreatePlexDownloadClient(MemoryStream memoryStream, int downloadSpeedLimitInKb = 0)
        {
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            var downloadTask = GetTestDownloadTask();
            downloadTask.DownloadWorkerTasks = Container.GetPlexDownloadTaskFactory.GenerateDownloadWorkerTasks(downloadTask, 4);

            return PlexDownloadClient.Create(downloadTask, _filesystem.Object, Container.GetPlexRipperHttpClient, downloadSpeedLimitInKb);
        }

        [Fact]
        public async Task StartAsync_ShouldDownloadValidFile_WhenValidUrlIsGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadClient = CreatePlexDownloadClient(memoryStream);
            downloadClient.IsFailed.Should().BeFalse();
            var mediaFile = MockServer.GetMockMediaData().First();

            // Act
            downloadClient.Value.Start();
            await downloadClient.Value.DownloadProcessTask;

            // Assert
            mediaFile.Md5.Should().Be(DataFormat.CalculateMD5(memoryStream));
        }

        [Fact]
        public async Task StartAsync_ShouldDownloadValidFile_WhenPausedAndResumed()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadClient = CreatePlexDownloadClient(memoryStream, 1000);
            downloadClient.IsFailed.Should().BeFalse();
            var mediaFile = MockServer.GetMockMediaData().First();

            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            // Act
            downloadClient.Value.Start();
            await Task.Delay(3000);
            //// Stop task
            var downloadTask = await downloadClient.Value.StopAsync();
            downloadTask.IsSuccess.ShouldBeTrue();
            //// Create new client and restart
            var downloadClient2 = PlexDownloadClient.Create(downloadTask.Value, _filesystem.Object, Container.GetPlexRipperHttpClient, 1000);
            downloadClient2.Value.Start();
            await downloadClient2.Value.DownloadProcessTask;

            // Assert
            mediaFile.Md5.Should().Be(DataFormat.CalculateMD5(memoryStream));
        }

        [Fact]
        public void Create_ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            // Act
            var downloadClient = PlexDownloadClient.Create(null, _filesystem.Object, Container.GetPlexRipperHttpClient);

            // Assert
            downloadClient.IsFailed.Should().BeTrue();
            downloadClient.Errors.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Create_ShouldReturnFailedResult_WhenNullDownloadTaskWorkersIsGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            // Act
            var downloadClient = PlexDownloadClient.Create(new DownloadTask(), _filesystem.Object, Container.GetPlexRipperHttpClient);

            // Assert
            downloadClient.IsFailed.Should().BeTrue();
            downloadClient.Errors.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Create_ShouldReturnValidPlexDownloadClientResult_WhenValidDownloadTaskIsGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();

            // Act
            var downloadClient = CreatePlexDownloadClient(memoryStream);

            // Assert
            downloadClient.IsFailed.Should().BeFalse();
            downloadClient.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_ShouldBeDownloadSpeedLimited_WhenDownloadSpeedLimitIsGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadClient = CreatePlexDownloadClient(memoryStream, 500);
            var updates = new List<DownloadClientUpdate>();
            downloadClient.Value.DownloadClientUpdate.Subscribe(update => updates.Add(update));

            // Act
            downloadClient.Value.Start();
            await downloadClient.Value.DownloadProcessTask;

            // Assert
            updates.ShouldAllBe(x => x.DownloadSpeed / 1024F > 500 - 100 && x.DownloadSpeed / 1024F < 500 + 100);
        }
    }
}