using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using MediatR;
using Moq;
using Moq.Language.Flow;

namespace PlexRipper.BaseTests.Extensions
{
    public static class MoqExtensions
    {
        public static ISetup<IMediator, Task<TResult>> SetupMediator<TResult>(this AutoMock mock, Func<IRequest<TResult>> request)
        {
            return mock.Mock<IMediator>().Setup(m => m.Send(request.Invoke(), It.IsAny<CancellationToken>()));
        }
    }
}