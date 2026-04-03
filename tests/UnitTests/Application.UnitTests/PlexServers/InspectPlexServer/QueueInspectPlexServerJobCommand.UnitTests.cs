namespace Reaparr.Application.UnitTests;

public class QueueInspectPlexServerJobCommandUnitTests
{
    [Test]
    public void ShouldRejectNullPlexServerIds_WhenQueueingInspectJob()
    {
        // Arrange
        var validator = new QueueInspectPlexServerJobCommandValidator();
        var command = new QueueInspectPlexServerJobCommand(null!);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(QueueInspectPlexServerJobCommand.PlexServerIds));
    }

    [Test]
    public void ShouldRejectEmptyPlexServerIds_WhenQueueingInspectJob()
    {
        // Arrange
        var validator = new QueueInspectPlexServerJobCommandValidator();
        var command = new QueueInspectPlexServerJobCommand([]);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(QueueInspectPlexServerJobCommand.PlexServerIds));
    }
}
