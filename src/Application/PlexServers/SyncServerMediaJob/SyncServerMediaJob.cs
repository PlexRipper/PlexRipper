using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Reaparr.WebAPI.Contracts;
using Serilog;

namespace Reaparr.Application;

public class SyncServerMediaJob : IJob
{
    private readonly Serilog.ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public static string PlexServerIdParameter => "plexServerId";
    public static string ForceSyncParameter => "forceSync";

    public static JobKey GetJobKey(int id) => new($"{PlexServerIdParameter}_{id}", nameof(SyncServerMediaJob));

    public SyncServerMediaJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContext dbContext,
        ISignalRService signalRService
    )
    {
        _log = log.ForContext<SyncServerMediaJob>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        var forceSync = dataMap.GetBooleanValue(ForceSyncParameter);

        _log.Here()
            .Debug(
                "Executing job: {SyncServerMediaJobName)} for {PlexServerName)}: {PlexServerId}",
                nameof(SyncServerMediaJob),
                nameof(PlexServer),
                plexServerId
            );

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var plexServer = await _dbContext.PlexServers.IncludeLibraries().GetAsync(plexServerId);
            if (plexServer is null)
            {
                ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId).LogError();
                return;
            }

            if (!plexServer.IsEnabled)
            {
                var plexServerName = await _dbContext.GetPlexServerNameById(plexServerId);
                ResultExtensions
                    .ServerIsNotEnabled(plexServerName, plexServerId, nameof(SyncServerMediaJob))
                    .LogError();
                return;
            }

            var plexLibraries = forceSync
                ? plexServer.PlexLibraries
                : plexServer
                    .PlexLibraries.Where(x =>
                        x is { Outdated: true, Type: PlexMediaType.Movie or PlexMediaType.TvShow }
                    )
                    .ToList();

            if (!plexLibraries.Any())
            {
                _log.Here()
                    .Information(
                        "PlexServer {PlexServerName} with id {PlexServerId} has no libraries to sync",
                        plexServer.Name,
                        plexServer.Id
                    );
                return;
            }

            // Send progress on every library update
            var progressList = new List<LibraryProgress>();

            // Initialize list
            progressList.AddRange(
                plexLibraries.Select(x => new LibraryProgress
                {
                    Id = x.Id,
                    Step = 0,
                    Received = 0,
                    Total = x.MediaCount,
                    TotalSteps = 1,
                    TimeRemaining = TimeSpan.Zero,
                })
            );

            var progress = new Action<LibraryProgress>(libraryProgress =>
            {
                var i = progressList.FindIndex(x => x.Id == libraryProgress.Id);
                if (i != -1)
                    progressList[i] = libraryProgress;
                else
                    progressList.Add(libraryProgress);

                _signalRService.SendServerSyncProgressUpdateAsync(
                    new SyncServerMediaProgress { ServerId = plexServerId, LibraryProgresses = progressList }
                );
            });

            var results = new List<Result>();

            // Sync movie type libraries first because it is a lot quicker than TvShows.
            // Also, no parallel execution because its the same server and we don't want to overload it.
            foreach (var library in plexLibraries.Where(x => x.Type == PlexMediaType.Movie))
            {
                var result = await _commandExecutor.Send(new RefreshLibraryMediaCommand(library.Id, progress));
                if (result.IsFailed)
                    results.Add(result.ToResult());
            }

            foreach (var library in plexLibraries.Where(x => x.Type == PlexMediaType.TvShow))
            {
                var result = await _commandExecutor.Send(new RefreshLibraryMediaCommand(library.Id, progress));
                if (result.IsFailed)
                    results.Add(result.ToResult());
            }

            // Send a refresh notification to all clients
            await _signalRService.SendRefreshNotificationAsync(RefreshDataType.PlexLibrary);

            if (results.Any())
            {
                var failedResult = Result.Fail($"Some libraries failed to sync in PlexServer: {plexServer.Name}");
                Result.Merge(failedResult, results.Merge()).LogError();
                return;
            }

            _log.Here()
                .Information(
                    "Successfully synced server \"{PlexServerName}\" with id {PlexServerId} has no libraries to sync",
                    plexServer.Name,
                    plexServer.Id
                );
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
        }
    }
}
