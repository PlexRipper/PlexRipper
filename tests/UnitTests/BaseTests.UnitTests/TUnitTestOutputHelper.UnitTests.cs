namespace Reaparr.BaseTests.UnitTests;

public class TUnitTestOutputHelperUnitTests
{
    [Test]
    public void Output_ShouldTreatPartialWritesAsSingleLineUntilWriteLineCompletesIt()
    {
        // Arrange
        var outputHelper = new TUnitTestOutputHelper();

        // Act
        outputHelper.Write("part 1");
        outputHelper.Write(" part 2");
        outputHelper.WriteLine(" done");

        // Assert
        outputHelper.Output.ShouldBe("part 1 part 2 done");
    }
}
