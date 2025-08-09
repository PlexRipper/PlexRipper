using System.Net;
using System.Text;
using Application.Contracts;
using FastEndpoints;
using Moq.Contrib.HttpClient;
using Moq.Language.Flow;
using Newtonsoft.Json;

namespace PlexRipper.BaseTests;

public static class MoqExtensions
{
    public static ISetup<ICommandExecutor, Task<TResult>> SetupCommand<TResult>(
        this AutoMock mock,
        Func<ICommand<TResult>> request,
        bool isVerifiable = false
    )
    {
        var result = mock.Mock<ICommandExecutor>().Setup(m => m.Send(request.Invoke(), It.IsAny<CancellationToken>()));
        if (isVerifiable)
            result.Verifiable();
        return result;
    }

    public static IReturnsResult<ISignalRService> SendRefreshNotification(this AutoMock mock, bool isVerifiable = false)
    {
        var result = mock.Mock<ISignalRService>()
            .Setup(m =>
                m.SendRefreshNotificationAsync(It.IsAny<List<RefreshDataType>>(), It.IsAny<CancellationToken>())
            )
            .Returns(Task.CompletedTask);

        if (isVerifiable)
            result.Verifiable();
        return result;
    }

    public static ISetup<IEventPublisher, Task> PublishMediator(this AutoMock mock, Func<IEvent> request)
    {
        return mock.Mock<IEventPublisher>().Setup(x => x.PublishAsync(request.Invoke(), It.IsAny<CancellationToken>()));
    }

    #region Verify

    public static void VerifyMediator(this AutoMock mock, Func<IEvent> notification, Times times)
    {
        mock.Mock<IEventPublisher>()
            .Verify(x => x.PublishAsync(notification.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyMediator(this AutoMock mock, Func<IEvent> notification, Func<Times> times)
    {
        mock.Mock<IEventPublisher>()
            .Verify(x => x.PublishAsync(notification.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyMediator(this AutoMock mock, Func<ICommand> request, Times times)
    {
        mock.Mock<ICommandExecutor>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyMediator(this AutoMock mock, Func<ICommand> request, Func<Times> times)
    {
        mock.Mock<ICommandExecutor>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyMediator<T>(this AutoMock mock, Func<ICommand<T>> request, Func<Times> times)
    {
        mock.Mock<ICommandExecutor>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyMediator<T>(this AutoMock mock, Func<ICommand<T>> request, Times times)
    {
        mock.Mock<ICommandExecutor>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    #region Publish/Notification

    public static void VerifyNotification(this AutoMock mock, Func<IEvent> notification, Func<Times> times)
    {
        mock.Mock<IEventPublisher>()
            .Verify(x => x.PublishAsync(notification.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyNotification(this AutoMock mock, IEvent notification, Func<Times> times)
    {
        mock.Mock<IEventPublisher>().Verify(x => x.PublishAsync(notification, It.IsAny<CancellationToken>()), times);
    }

    #endregion

    #endregion

    public static IReturnsResult<T> ReturnOk<T>(this ISetup<T, Task<Result>> mock)
        where T : class => mock.ReturnsAsync(Result.Ok());

    public static IReturnsResult<T> ReturnOk<T>(this ISetup<T, Task<Result<T>>> mock)
        where T : class => mock.ReturnsAsync(Result.Ok());

    public static void SetupIdentityRequest(this Mock<HttpMessageHandler> mock, Seed seed, string uri = "")
    {
        if (string.IsNullOrEmpty(uri))
        {
            mock.SetupRequest(r => r.RequestUri!.AbsoluteUri.Contains("identity"))
                .ReturnsAsync(
                    (HttpRequestMessage req, CancellationToken _) =>
                        FakePlexApiData.GetPlexServerIdentityResponse(HttpStatusCode.OK, seed, req).RawResponse
                );
            return;
        }

        var uriBuilder = new UriBuilder(uri) { Path = "/identity" };
        mock.SetupRequest(HttpMethod.Get, uriBuilder.Uri)
            .ReturnsAsync(
                (HttpRequestMessage req, CancellationToken _) =>
                    FakePlexApiData.GetPlexServerIdentityResponse(HttpStatusCode.OK, seed, req).RawResponse
            );
    }

    public static HttpResponseMessage ToJsonHttpResponse(
        this object? responseBody,
        HttpRequestMessage request,
        HttpStatusCode statusCode
    )
    {
        var json = JsonConvert.SerializeObject(responseBody, MockPlexApiJsonSerializer.Settings);
        var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

        return new HttpResponseMessage(statusCode) { RequestMessage = request, Content = jsonContent };
    }

    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestAnyQuery(
        this Mock<HttpMessageHandler> handler,
        HttpMethod method,
        Uri requestUri
    )
    {
        return handler.SetupRequest(req =>
            req.Method == method
            && req.RequestUri?.AbsolutePath.Equals(requestUri.AbsolutePath, StringComparison.OrdinalIgnoreCase) == true
        );
    }

    public static void SetupDownloadFile(this Mock<HttpMessageHandler> mock, int fileSizeInMb)
    {
        mock.SetupRequest(r => r.RequestUri!.AbsoluteUri.Contains("file.mp4"))
            .ReturnsAsync(
                (HttpRequestMessage _, CancellationToken _) =>
                {
                    var downloadFile = FakeData.GetDownloadFile(fileSizeInMb);
                    var fileContent = new ByteArrayContent(downloadFile); // example byte array

                    // Simulate headers for a file download
                    fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue(
                        "attachment"
                    )
                    {
                        FileName = "file.mp4",
                    };

                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                        "application/octet-stream"
                    );

                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = fileContent };
                }
            );
    }
}
