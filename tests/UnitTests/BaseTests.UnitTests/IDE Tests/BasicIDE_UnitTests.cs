namespace Reaparr.BaseTests.UnitTests.IDE_Tests;

public class NormalEmptyUnitTests
{
    private readonly ITestOutputHelper _output;

    public NormalEmptyUnitTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ShouldCompleteImmediately_WhenEmptyTestWithOnlyAnAssertion()
    {
        // Assert
        true.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldStopOnException_WhenAnUserUnhandeledExceptionIsThrown()
    {
        // Act
        var exception = await Record.ExceptionAsync(() => throw new Exception("Test Exception"));

        // Assert
        exception.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldStopOnException_WhenAnUserHandledExceptionIsThrown()
    {
        // Act
        var exception = await Record.ExceptionAsync(() =>
        {
            try
            {
                throw new Exception("Test Exception");
            }
            catch (Exception e)
            {
                _output.WriteLine(e.Message);
                return Task.CompletedTask;
            }
        });

        // Assert
        exception.ShouldBeNull();
    }
}
