using System.Collections.Concurrent;
using Reaparr.SignalR.Contracts;

namespace Reaparr.BaseTests;

public class MockNotificationHubService : INotificationHubService
{
    private readonly ILogger _log;

    public BlockingCollection<RefreshDataType> RefreshNotificationList { get; } = new();

    public MockNotificationHubService(ILogger log)
    {
        _log = log.ForContext<MockNotificationHubService>();
    }

    public Task SendNotificationAsync(Notification notification) => Task.CompletedTask;

    public Task SendRefreshNotificationAsync(RefreshDataType dataType, CancellationToken cancellationToken = default)
    {
        RefreshNotificationList.Add(dataType, cancellationToken);
        _log.Here().Verbose("{ClassName} => {@DataType}", nameof(MockNotificationHubService), dataType);

        return Task.CompletedTask;
    }

    public async Task SendRefreshNotificationAsync(
        List<RefreshDataType> dataTypes,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var dataType in dataTypes)
            await SendRefreshNotificationAsync(dataType, cancellationToken);
    }
}
