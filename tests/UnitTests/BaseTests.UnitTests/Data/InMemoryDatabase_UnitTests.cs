using Microsoft.EntityFrameworkCore;

namespace BaseTests.UnitTests.Data;

public class InMemoryDatabase_UnitTests : BaseUnitTest
{
    public InMemoryDatabase_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldAddNotificationToInMemoryDatabase_WhenNotificationIsAdded()
    {
        // Arrange
        await using var context = MockDatabase.GetMemoryDbContext();
        var notification = new Notification()
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);
        var notifications = await context.Notifications.ToListAsync(CancellationToken.None);

        // Assert
        notifications.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldAddAndRemoveNotificationToInMemoryDatabase_WhenNotificationIsAddedAndRemoved()
    {
        // Arrange
        await using var context = MockDatabase.GetMemoryDbContext();
        var notification = new Notification()
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken.None);
        context.Notifications.Remove(notification);
        await context.SaveChangesAsync(CancellationToken.None);
        var notifications = await context.Notifications.ToListAsync(CancellationToken.None);

        // Assert
        notifications.Count.ShouldBe(0);
    }
}
