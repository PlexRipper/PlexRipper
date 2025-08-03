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
        var dbContext = MockDatabase.GetMemoryDbContext();
        var (plexRipperContext, _) = dbContext;
        var notification = new Notification
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        plexRipperContext.Notifications.Add(notification);
        await plexRipperContext.SaveChangesAsync(CancellationToken);
        var notifications = await plexRipperContext.Notifications.ToListAsync(CancellationToken);

        // Assert
        notifications.Count.ShouldBe(1);
    }

    [Fact]
    public async Task ShouldAddAndRemoveNotificationToInMemoryDatabase_WhenNotificationIsAddedAndRemoved()
    {
        // Arrange
        var dbContext = MockDatabase.GetMemoryDbContext();
        var (plexRipperContext, _) = dbContext;
        var notification = new Notification
        {
            Hidden = false,
            Level = NotificationLevel.Information,
            Message = "Test Notification",
            CreatedAt = DateTime.UtcNow,
        };

        // Act
        plexRipperContext.Notifications.Add(notification);
        await plexRipperContext.SaveChangesAsync(CancellationToken);
        plexRipperContext.Notifications.Remove(notification);
        await plexRipperContext.SaveChangesAsync(CancellationToken);
        var notifications = await plexRipperContext.Notifications.ToListAsync(CancellationToken);

        // Assert
        notifications.Count.ShouldBe(0);
    }
}
