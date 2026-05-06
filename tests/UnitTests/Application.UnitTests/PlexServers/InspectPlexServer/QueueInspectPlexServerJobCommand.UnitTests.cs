namespace Reaparr.Application.UnitTests;

public class QueueInspectPlexServerJobCommandUnitTests : BaseUnitTest<QueueInspectPlexServerJobCommandHandler>
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

    [Test]
    public async Task ShouldFail_WhenAnyRequestedServerIsDisabled()
    {
        // Arrange
        await SetupDatabase(91101, config => config.PlexServerCount = 2);

        var dbContext = IDbContext;
        var servers = await dbContext.PlexServers.IgnoreIsEnabledFilter().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        var enabledServerId = servers[0].Id;
        var disabledServerId = servers[1].Id;

        await dbContext.PlexServers.IgnoreIsEnabledFilter()
            .Where(x => x.Id == disabledServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Never());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand([enabledServerId, disabledServerId]), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains(disabledServerId.ToString()));
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldReturnFailedResult_WhenNoRequestedServersExist()
    {
        // Arrange
        await SetupDatabase(91108, config => config.PlexServerCount = 1);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Never());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand([9991, 9992]), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("No Plex servers", StringComparison.OrdinalIgnoreCase));
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldQueueOnlyExistingEnabledServers_WhenSomeRequestedServersAreMissing()
    {
        // Arrange
        await SetupDatabase(91109, config => config.PlexServerCount = 1);

        var dbContext = IDbContext;
        var existingServerId = (await dbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken)).Id;

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand([existingServerId, 9999]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldQueueJob_WhenAllRequestedServersExistAndAreEnabled()
    {
        // Arrange
        await SetupDatabase(91121, config => config.PlexServerCount = 2);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Once());

        var serverIds = await IDbContext.PlexServers.IgnoreIsEnabledFilter().Select(x => x.Id).ToListAsync(CancellationToken);

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand(serverIds), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldNotScheduleJob_WhenAllRequestedServersAreDisabled()
    {
        // Arrange
        await SetupDatabase(91122, config => config.PlexServerCount = 2);

        var ids = await IDbContext.PlexServers.IgnoreIsEnabledFilter().Select(x => x.Id).ToListAsync(CancellationToken);
        await IDbContext.PlexServers.IgnoreIsEnabledFilter().Where(x => ids.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Never());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand(ids), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldNotScheduleJob_WhenAtLeastOneRequestedServerIsDisabledEvenIfOthersEnabled()
    {
        // Arrange
        await SetupDatabase(91123, config => config.PlexServerCount = 3);

        var ids = await IDbContext.PlexServers.IgnoreIsEnabledFilter().OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(CancellationToken);
        await IDbContext.PlexServers.IgnoreIsEnabledFilter().Where(x => x.Id == ids[1])
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Never());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand(ids), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldIgnoreMissingIdsAndStillQueue_WhenAtLeastOneEnabledServerExists()
    {
        // Arrange
        await SetupDatabase(91124, config => config.PlexServerCount = 1);

        var existingId = (await IDbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken)).Id;

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Once());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand([existingId, 7777, 8888]), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify();
    }

    [Test]
    public async Task ShouldNotQueueWhenOnlyMissingAndDisabledIdsAreProvided()
    {
        // Arrange
        await SetupDatabase(91125, config => config.PlexServerCount = 1);

        var disabledId = (await IDbContext.PlexServers.IgnoreIsEnabledFilter().FirstAsync(CancellationToken)).Id;
        await IDbContext.PlexServers.IgnoreIsEnabledFilter().Where(x => x.Id == disabledId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        Mock.Mock<IScheduler>()
            .Setup(x => x.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .Verifiable(Times.Never());

        Mock.Mock<IScheduler>()
            .Setup(x => x.ScheduleJob(It.IsAny<IJobDetail>(), It.IsAny<ITrigger>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DateTimeOffset.UtcNow)
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new QueueInspectPlexServerJobCommand([disabledId, 9998]), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IScheduler>().Verify();
    }
}

