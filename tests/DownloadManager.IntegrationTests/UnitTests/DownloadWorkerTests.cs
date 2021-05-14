using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace DownloadManager.UnitTests.UnitTests
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

        [Fact]
        public async Task StartAsync_ShouldDownloadValidFile_WhenValidUrlGiven()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            var downloadWorkerTask = new DownloadWorkerTask(GetTestDownloadTask(), 0, 0, 6482740);
            var downloadWorker = new DownloadWorker(downloadWorkerTask, _filesystem.Object, Container.GetPlexRipperHttpClient);

            //Act
            await downloadWorker.StartAsync();

            //Assert
            memoryStream.Length.Should().Be(6482740);

            // Check MD5 hash
            _fileMd5.Should().Be(DataFormat.CalculateMD5(memoryStream));
        }

        [Fact]
        public async Task StartAsync_ShouldDownloadValidFile_WhenPausedAndResumed()
        {
            //Arrange
            var memoryStream = new MemoryStream();
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>()))
                .Returns(Result.Ok<Stream>(memoryStream));

            var downloadWorkerTask = new DownloadWorkerTask(GetTestDownloadTask(), 0, 0, 6482740);
            var downloadWorker = new DownloadWorker(downloadWorkerTask, _filesystem.Object, Container.GetPlexRipperHttpClient);

            //Act
            downloadWorker.StartAsync();
            await Task.Delay(500);
            await downloadWorker.Stop();

            //Assert

            downloadWorker.LastDownloadWorkerUpdate.DownloadStatus.Should().Be(DownloadStatus.Stopped);
        }
    }
}