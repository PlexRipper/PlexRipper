using Autofac;
using AutoMapper;
using Moq.Language.Flow;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests;

public static class MoqExtensions
{
    // TODO rename to SendMediator
    public static ISetup<IMediator, Task<TResult>> SetupMediator<TResult>(
        this AutoMock mock,
        Func<IRequest<TResult>> request,
        bool isVerifiable = false
    )
    {
        var result = mock.Mock<IMediator>().Setup(m => m.Send(request.Invoke(), It.IsAny<CancellationToken>()));
        if (isVerifiable)
            result.Verifiable();
        return result;
    }

    public static ISetup<IMediator, Task> SetupMediator(
        this AutoMock mock,
        Func<IRequest> request,
        bool isVerifiable = false
    )
    {
        var result = mock.Mock<IMediator>().Setup(m => m.Send(request.Invoke(), It.IsAny<CancellationToken>()));
        if (isVerifiable)
            result.Verifiable();
        return result;
    }

    public static ISetup<IMediator, Task> PublishMediator(this AutoMock mock, Func<INotification> request)
    {
        return mock.Mock<IMediator>().Setup(x => x.Publish(request.Invoke(), It.IsAny<CancellationToken>()));
    }

    #region Verify

    // public static void VerifyMediator(this AutoMock mock, IRequest request, Func<Times> times)
    // {
    //     mock.Mock<IMediator>().Verify(x => x.Send(request, It.IsAny<CancellationToken>()), times);
    // }

    public static void VerifyMediator(this AutoMock mock, Func<IRequest> request, Func<Times> times)
    {
        mock.Mock<IMediator>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    // public static void VerifyMediator<T>(this AutoMock mock, IRequest<Result<T>> request, Func<Times> times)
    // {
    //     mock.Mock<IMediator>().Verify(x => x.Send(request, It.IsAny<CancellationToken>()), times);
    // }
    //
    // public static void VerifyMediator(this AutoMock mock, IRequest<Result> request, Func<Times> times)
    // {
    //     mock.Mock<IMediator>().Verify(x => x.Send(request, It.IsAny<CancellationToken>()), times);
    // }

    public static void VerifyMediator<T>(this AutoMock mock, Func<IRequest<T>> request, Func<Times> times)
    {
        mock.Mock<IMediator>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyMediator<T>(this AutoMock mock, Func<IRequest<T>> request, Times times)
    {
        mock.Mock<IMediator>().Verify(x => x.Send(request.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    #region Publish/Notification

    public static void VerifyNotification(this AutoMock mock, Func<INotification> notification, Func<Times> times)
    {
        mock.Mock<IMediator>().Verify(x => x.Publish(notification.Invoke(), It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyNotification(this AutoMock mock, INotification notification, Func<Times> times)
    {
        mock.Mock<IMediator>().Verify(x => x.Publish(notification, It.IsAny<CancellationToken>()), times);
    }

    #endregion

    #endregion

    public static AutoMock AddMapper(this AutoMock mock)
    {
        return AutoMock.GetStrict(builder =>
            builder.RegisterInstance(MapperSetup.CreateMapper()).As<IMapper>().SingleInstance()
        );
    }

    public static IReturnsResult<T> ReturnOk<T>(this ISetup<T, Task<Result>> mock)
        where T : class => mock.ReturnsAsync(Result.Ok());

    public static IReturnsResult<T> ReturnOk<T>(this ISetup<T, Task<Result<T>>> mock)
        where T : class => mock.ReturnsAsync(Result.Ok());

    public static IReturnsResult<IMediator> ReturnOk(this IReturnsThrows<IMediator, Task<Result>> mock) =>
        mock.ReturnsAsync(Result.Ok());
}
