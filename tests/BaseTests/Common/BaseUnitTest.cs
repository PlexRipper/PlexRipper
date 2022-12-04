namespace PlexRipper.BaseTests;

public class BaseUnitTest
{
    protected BaseUnitTest(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }
}

public class BaseUnitTest<TUnitTestClass> : BaseUnitTest, IDisposable where TUnitTestClass : class
{
    protected readonly TUnitTestClass _sut;

    protected readonly AutoMock mock = AutoMock.GetStrict();

    protected BaseUnitTest(ITestOutputHelper output, bool disableMockCreate = false) : base(output)
    {
        if (!disableMockCreate)
            _sut = mock.Create<TUnitTestClass>();
    }

    public void Dispose()
    {
        mock?.Dispose();
    }
}