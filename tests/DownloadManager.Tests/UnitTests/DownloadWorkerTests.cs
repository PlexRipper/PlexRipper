using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace DownloadManager.Tests.UnitTests
{
    public class DownloadWorkerTests
    {
        private readonly WireMockServer _server;

        private static readonly string _filePath = @$"{FileSystemPaths.RootDirectory}/resources/test-video.mp4";

        private static readonly string _fileMd5 = DataFormat.CalculateMD5(_filePath);

        private BaseContainer Container { get; }

        public DownloadWorkerTests()
        {
            Container = new BaseContainer();

            _server = WireMockServer.Start();

            Log.Debug($"Server running at: {_server.Urls[0]}");

            _server
                .Given(Request.Create().WithPath("/foo/file.mkv").UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(206)
                        .WithBodyFromFile(_filePath)
                );
        }

        private DownloadTask GetTestDownloadTask()
        {
            var uri = new Uri(_server.Urls[0]);

            return new DownloadTask
            {
                MediaType = PlexMediaType.Movie,
                PlexServer = new()
                {
                    Scheme = uri.Scheme,
                    Address = uri.Host,
                    Host = uri.Host,
                    Port = uri.Port,
                },
                ServerToken = "AAABBBCCCDDDEEEFFFGGG",
                MetaData = new()
                {
                    MovieTitle = "TestMovie",
                    MediaData = new List<PlexMediaData>
                    {
                        new()
                        {
                            Parts = new List<PlexMediaDataPart>
                            {
                                new()
                                {
                                    ObfuscatedFilePath = "/foo/file.mkv",
                                },
                            },
                        },
                    },
                },
            };
        }

        private DownloadWorker GetDownloadWorker(MemoryStream memoryStream)
        {
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));
            var downloadWorkerTask = new DownloadWorkerTask(GetTestDownloadTask(), 0, 0, 6482740)
            {
                Id = 1,
            };
            return new DownloadWorker(downloadWorkerTask, _filesystem.Object, Container.GetPlexRipperHttpClient);
        }

        [Fact]
        public async Task Start_ShouldDownloadValidFile_WhenValidUrlGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var downloadWorker = GetDownloadWorker(memoryStream);

            //Act
            downloadWorker.Start();
            await downloadWorker.DownloadProcessTask;

            //Assert
            memoryStream.Length.Should().Be(6482740);

            // Check MD5 hash
            _fileMd5.Should().Be(DataFormat.CalculateMD5(memoryStream));
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
    }
}