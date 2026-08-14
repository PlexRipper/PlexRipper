namespace Reaparr.Data.UnitTests;

public class ReaparrDbContextConcurrencyUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldSerializeBulkAndBatchWrites_WhenRunConcurrently()
    {
        // Arrange
        await SetupDatabase(
            91234,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
            }
        );

        using var setupContext = IDbContext;
        var libraryId = await setupContext.PlexLibraries.Select(x => x.Id).FirstAsync(CancellationToken);

        var actors = Enumerable
            .Range(1, 50)
            .Select(x => new PlexActor { Name = $"Concurrent actor {x}", Key = $"concurrent-actor-{x}" })
            .ToList();
        var notifications = Enumerable
            .Range(1, 25)
            .Select(x => new Notification
            {
                Level = NotificationLevel.Information,
                Message = $"Concurrent notification {x}",
                CreatedAt = DateTime.UtcNow,
            })
            .ToList();

        // Act
        var startBarrier = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var readyCount = 0;

        async Task WaitForConcurrentStart()
        {
            if (Interlocked.Increment(ref readyCount) == 3)
                startBarrier.TrySetResult();

            await startBarrier.Task.WaitAsync(CancellationToken);
        }

        await Task.WhenAll(
            Task.Run(
                async () =>
                {
                    using var dbContext = IDbContext;
                    await WaitForConcurrentStart();
                    await dbContext.BulkInsertAsync(actors, cancellationToken: CancellationToken);
                },
                CancellationToken
            ),
            Task.Run(
                async () =>
                {
                    using var dbContext = IDbContext;
                    dbContext.Notifications.AddRange(notifications);
                    await WaitForConcurrentStart();
                    await dbContext.SaveChangesAsync(CancellationToken);
                },
                CancellationToken
            ),
            Task.Run(
                async () =>
                {
                    using var dbContext = IDbContext;
                    await WaitForConcurrentStart();
                    await dbContext
                        .PlexLibraries.Where(x => x.Id == libraryId)
                        .ExecuteUpdateAsync(x => x.SetProperty(y => y.MovieCount, 42), CancellationToken);
                },
                CancellationToken
            )
        );

        // Assert
        using var assertContext = IDbContext;
        var actorCount = await assertContext.PlexActors.CountAsync(
            x => x.Key.StartsWith("concurrent-actor-"),
            CancellationToken
        );
        var notificationCount = await assertContext.Notifications.CountAsync(
            x => x.Message.StartsWith("Concurrent notification"),
            CancellationToken
        );
        var movieCount = await assertContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Select(x => x.MovieCount)
            .SingleAsync(CancellationToken);

        actorCount.ShouldBe(actors.Count);
        notificationCount.ShouldBe(notifications.Count);
        movieCount.ShouldBe(42);
    }

    [Test]
    public async Task ShouldRollbackBulkInsert_WhenSerializedTransactionFails()
    {
        // Arrange
        await SetupDatabase(91235);
        var actors = Enumerable
            .Range(1, 10)
            .Select(x => new PlexActor { Name = $"Rollback actor {x}", Key = $"rollback-actor-{x}" })
            .ToList();

        // Act
        using var dbContext = IDbContext;
        var result = await dbContext.ExecuteSerializedTransactionAsync(
            async (context, ct) =>
            {
                await context.BulkInsertAsync(actors, cancellationToken: ct);
                throw new InvalidOperationException("rollback test");
            },
            CancellationToken
        );

        // Assert
        using var assertContext = IDbContext;
        var actorCount = await assertContext.PlexActors.CountAsync(
            x => x.Key.StartsWith("rollback-actor-"),
            CancellationToken
        );

        result.IsFailed.ShouldBeTrue();
        result.HasException<InvalidOperationException>().ShouldBeTrue();
        actorCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnCancelledResult_WhenSerializedTransactionIsCancelled()
    {
        // Arrange
        await SetupDatabase(91236);
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        // Act
        using var dbContext = IDbContext;
        var result = await dbContext.ExecuteSerializedTransactionAsync(
            (_, ct) => Task.Delay(Timeout.InfiniteTimeSpan, ct),
            cancellationTokenSource.Token
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.IsCancelled.ShouldBeTrue();
    }
}
