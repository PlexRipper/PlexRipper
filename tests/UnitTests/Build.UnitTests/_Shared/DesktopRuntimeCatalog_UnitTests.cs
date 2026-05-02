namespace Reaparr.Build.UnitTests;

public class DesktopRuntimeCatalogUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldThrowHelpfulMessage_WhenRuntimeIdentifierIsMissing()
    {
        // Act
        var action = () => DesktopRuntimeCatalog.Get(string.Empty);

        // Assert
        var exception = action.ShouldThrow<ArgumentException>();
        exception.Message.ShouldContain("A runtime identifier is required");
        exception.Message.ShouldContain("--rid <RID>");
        exception.Message.ShouldContain("linux-x64");
    }
}
