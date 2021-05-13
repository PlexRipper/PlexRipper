using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentResults;
using Moq;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;
using Xunit;

namespace DownloadManager.UnitTests.UnitTests
{
    public class DownloadWorkerTests
    {
        [Fact]
        public async Task ShouldResturnValidDownloadStream()
        {
            //Arrange
            var _filesystem = new Mock<IFileSystemCustom>();
            _filesystem.Setup(x => x.DownloadWorkerTempFileStream("", "TestFile.mkv", 0)).Returns(Result.Ok<Stream>(new MemoryStream()));

            var downloadTask = new DownloadTask
            {
                MediaType = PlexMediaType.Movie,
                PlexServer = new()
                {
                    Scheme = "http",
                    Address = "localhost",
                    Host = "8.8.8.8",
                    Port = 8080,
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
                                    ObfuscatedFilePath = "/movie/media/path/file.mkv",
                                },
                            },
                        },
                    },
                },
            };

            var handlerMock = new Mock<HttpMessageHandler>();

            var client = new Mock<IPlexRipperHttpClient>(MockBehavior.Default);

            client.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>(), HttpCompletionOption.ResponseHeadersRead))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{ ""id"": 101 }"),
                });

            var downloadWorkerTask = new DownloadWorkerTask(downloadTask, 0, 0, 1000);
            var downloadWorker = new DownloadWorker(downloadWorkerTask, _filesystem.Object, client.Object);
            downloadWorker.DownloadWorkerLog.Subscribe(downloadWorkerLog => downloadWorkerLog.Log());

            //Act
            downloadWorker.Start();

            //Assert
        }
    }
}