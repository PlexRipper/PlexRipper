namespace Reaparr.BaseTests.UnitTests;

public class BasicIDEUnitTests
{
    private readonly ITestOutputHelper _output = new TUnitTestOutputHelper();

    [Test]
    public void ShouldCompleteImmediately_WhenEmptyTestWithOnlyAnAssertion()
    {
        // Assert
        true.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldStopOnException_WhenAnUserUnhandeledExceptionIsThrown()
    {
        // Act
        Exception? exception = null;
        try
        {
            await Task.Run(() => throw new Exception("Test Exception"));
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        exception.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldStopOnException_WhenAnUserHandledExceptionIsThrown()
    {
        // Act
        Exception? exception = null;
        try
        {
            await Task.Run(() =>
            {
                try
                {
                    throw new Exception("Test Exception");
                }
                catch (Exception e)
                {
                    _output.WriteLine(e.Message);
                }
            });
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        exception.ShouldBeNull();
    }
}
