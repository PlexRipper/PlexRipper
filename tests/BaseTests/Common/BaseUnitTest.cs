using Autofac.Extras.Moq;
using Logging;
using Xunit.Abstractions;

namespace PlexRipper.BaseTests
{
    public class BaseUnitTest<TUnitTestClass> : IDisposable where TUnitTestClass : class
    {
        protected readonly TUnitTestClass _sut;

        protected readonly AutoMock mock = AutoMock.GetStrict();

        protected BaseUnitTest(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
            _sut = mock.Create<TUnitTestClass>();
        }

        public void Dispose()
        {
            mock.Dispose();
        }
    }
}