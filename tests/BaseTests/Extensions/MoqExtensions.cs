using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using AutoMapper;
using MediatR;
using Moq;
using Moq.Language.Flow;
using PlexRipper.WebAPI.Config;

namespace PlexRipper.BaseTests.Extensions
{
    public static class MoqExtensions
    {
        public static ISetup<IMediator, Task<TResult>> SetupMediator<TResult>(this AutoMock mock, Func<IRequest<TResult>> request)
        {
            return mock.Mock<IMediator>().Setup(m => m.Send(request.Invoke(), It.IsAny<CancellationToken>()));
        }

        public static AutoMock AddMapper(this AutoMock mock)
        {
            return AutoMock.GetStrict(builder => builder.RegisterInstance(MapperSetup.CreateMapper()).As<IMapper>().SingleInstance());
        }
    }
}