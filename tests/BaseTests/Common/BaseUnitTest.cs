namespace PlexRipper.BaseTests;

public class BaseUnitTest<TUnitTestClass> : IDisposable where TUnitTestClass : class
{
    protected readonly TUnitTestClass _sut;

    protected readonly AutoMock mock = AutoMock.GetStrict();

    protected BaseUnitTest(ITestOutputHelper output, bool disableMockCreate = false)
    {
        Log.SetupTestLogging(output);
        if (!disableMockCreate)
            _sut = mock.Create<TUnitTestClass>();
    }

    public void Dispose()
    {
        mock.Dispose();
    }
}