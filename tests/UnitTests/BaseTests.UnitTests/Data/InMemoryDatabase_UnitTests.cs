using Microsoft.EntityFrameworkCore;

namespace Reaparr.BaseTests.UnitTests.Data;

public class InMemoryDatabaseUnitTests : BaseUnitTest
{
    public InMemoryDatabaseUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldAddNotificationToInMemoryDatabase_WhenNotificationIsAdded()
    {
        // Arrange
        var dbContext = MockDatabase.GetMemoryDbContext();
        var (context, _) = dbContext;
        var notification = new Notification
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken);
        var notifications = await context.Notifications.ToListAsync(CancellationToken);

        // Assert
        notifications.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldAddAndRemoveNotificationToInMemoryDatabase_WhenNotificationIsAddedAndRemoved()
    {
        // Arrange
        var dbContext = MockDatabase.GetMemoryDbContext();
        var (context, _) = dbContext;
        var notification = new Notification
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        context.Notifications.Add(notification);
        await context.SaveChangesAsync(CancellationToken);
        context.Notifications.Remove(notification);
        await context.SaveChangesAsync(CancellationToken);
        var notifications = await context.Notifications.ToListAsync(CancellationToken);

        // Assert
        notifications.Count.ShouldBe(0);
    }
}
