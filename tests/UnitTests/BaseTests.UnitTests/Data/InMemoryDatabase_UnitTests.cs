namespace Reaparr.BaseTests.UnitTests.Data;

public class InMemoryDatabaseUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldAddNotificationToInMemoryDatabase_WhenNotificationIsAdded()
    {
        // Arrange
        var notification = new Notification
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        var dbContext = IDbContext;
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(CancellationToken);
        var notifications = await dbContext.Notifications.ToListAsync(CancellationToken);

        // Assert
        notifications.Count.ShouldBe(1);
    }

    [Test]
    public async Task ShouldAddAndRemoveNotificationToInMemoryDatabase_WhenNotificationIsAddedAndRemoved()
    {
        // Arrange
        var notification = new Notification
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        var dbContext = IDbContext;
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync(CancellationToken);
        dbContext.Notifications.Remove(notification);
        await dbContext.SaveChangesAsync(CancellationToken);
        var notifications = await dbContext.Notifications.ToListAsync(CancellationToken);

        // Assert
        notifications.Count.ShouldBe(0);
    }
}
