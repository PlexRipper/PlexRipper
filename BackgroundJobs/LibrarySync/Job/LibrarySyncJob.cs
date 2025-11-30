using Quartz;
using Reaparr.Application;
using Reaparr.Application.Contracts;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Quartz job that syncs a single library and chains to the next library if provided.
/// Uses per-server locking to ensure only one library sync runs per server at a time.
/// </summary>
[DisallowConcurrentExecution]
public class LibrarySyncJob : IJob
{
    public const string ServerIdParameter = nameof(ServerIdParameter);
    public const string LibraryIdParameter = nameof(LibraryIdParameter);

    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISignalRService _signalRService;

    public LibrarySyncJob(ILogger log, ICommandExecutor commandExecutor, ISignalRService signalRService)
    {
        _log = log.ForContext<LibrarySyncJob>();
        _commandExecutor = commandExecutor;
        _signalRService = signalRService;
    }

    public static JobKey GetJobKey(int serverId, int libraryId) =>
        new($"LibrarySync_{serverId}_{libraryId}", "LibrarySync");

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;

        if (!dataMap.ContainsKey(ServerIdParameter) || !dataMap.ContainsKey(LibraryIdParameter))
        {
            _log.Here()
                .Error(
                    "Missing required parameters in job data map. ServerId: {ServerId}, LibraryId: {LibraryId}",
                    dataMap.ContainsKey(ServerIdParameter),
                    dataMap.ContainsKey(LibraryIdParameter)
                );
            return;
        }

        var serverId = dataMap.GetInt(ServerIdParameter);
        var libraryId = dataMap.GetInt(LibraryIdParameter);

        _log.Here()
            .Debug(
                "Executing job: {LibrarySyncJobName} for server {ServerId}, library {LibraryId}",
                nameof(LibrarySyncJob),
                serverId,
                libraryId
            );

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            // Create a progress action that sends individual library progress updates
            var progress = new Action<LibraryProgress>(libraryProgress =>
            {
                _signalRService.SendLibraryProgressUpdateAsync(libraryProgress);
            });

            // Execute the library sync command
            var result = await _commandExecutor.Send(
                new RefreshLibraryMediaCommand(libraryId, progress),
                context.CancellationToken
            );

            if (result.IsFailed)
            {
                result.LogError();
                _log.Here()
                    .Warning(
                        "Library sync failed for server {ServerId}, library {LibraryId}. Chain stopped.",
                        serverId,
                        libraryId
                    );
                return;
            }

            _log.Here()
                .Information("Successfully synced library {LibraryId} for server {ServerId}", libraryId, serverId);

            await _commandExecutor.Send(new QueueNextPlexLibraryToSyncCommand());
        }
        catch (OperationCanceledException)
        {
            _log.Here()
                .Information(
                    "{LibrarySyncJobName} for server {ServerId}, library {LibraryId} has been cancelled",
                    nameof(LibrarySyncJob),
                    serverId,
                    libraryId
                );
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
        }
    }
}
