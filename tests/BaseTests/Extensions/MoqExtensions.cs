using Autofac;
using AutoMapper;
using Moq.Language.Flow;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests;

public static class MoqExtensions
{
    // TODO rename to SendMediator
    public static ISetup<IMediator, Task<TResult>> SetupMediator<TResult>(this AutoMock mock, Func<IRequest<TResult>> request)
    {
        return mock.Mock<IMediator>().Setup(m => m.Send(request.Invoke(), It.IsAny<CancellationToken>()));
    }

    public static ISetup<IMediator, Task> PublishMediator(this AutoMock mock, Func<INotification> request)
    {
        return mock.Mock<IMediator>().Setup(x => x.Publish(request.Invoke(), It.IsAny<CancellationToken>()));
    }

    public static AutoMock AddMapper(this AutoMock mock)
    {
        return AutoMock.GetStrict(builder => builder.RegisterInstance(MapperSetup.CreateMapper()).As<IMapper>().SingleInstance());
    }


    public static IReturnsResult<T> ReturnOk<T>(this ISetup<T, Task<Result>> mock) where T : class
    {
        return mock.ReturnsAsync(Result.Ok());
    }

    public static IReturnsResult<IMediator> ReturnOk(this IReturnsThrows<IMediator, Task<Result>> mock)
    {
        return mock.ReturnsAsync(Result.Ok());
    }
}