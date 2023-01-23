using Application.Contracts;
using Data.Contracts;
using PlexRipper.Application;
using Quartz;
using WebAPI.Contracts;

namespace BackgroundServices.SyncServer;

public class SyncServerJob : IJob
{
    private readonly IMediator _mediator;
    private readonly IPlexServerService _plexServerService;
    private readonly IPlexLibraryService _plexLibraryService;
    private readonly ISignalRService _signalRService;

    public static string PlexServerIdParameter => "plexServerId";
    public static string ForceSyncParameter => "forceSync";

    public SyncServerJob(IMediator mediator, IPlexServerService plexServerService, IPlexLibraryService plexLibraryService, ISignalRService signalRService)
    {
        _mediator = mediator;
        _plexServerService = plexServerService;
        _plexLibraryService = plexLibraryService;
        _signalRService = signalRService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var plexServerId = dataMap.GetIntValue(PlexServerIdParameter);
        var forceSync = dataMap.GetBooleanValue(ForceSyncParameter);
        Log.Debug($"Executing job: {nameof(SyncServerJob)} for {nameof(PlexServer)}: {plexServerId}");

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            var plexServerResult = await _mediator.Send(new GetPlexServerByIdQuery(plexServerId, includeLibraries: true));
            if (plexServerResult.IsFailed)
            {
                plexServerResult.LogError();
                return;
            }

            var plexServer = plexServerResult.Value;
            var results = new List<Result>();

            var plexLibraries = forceSync
                ? plexServer.PlexLibraries
                : plexServer.PlexLibraries.FindAll(
                    x => x.Outdated
                         && x.Type is PlexMediaType.Movie or PlexMediaType.TvShow);

            if (!plexLibraries.Any())
            {
                Log.Information($"PlexServer {plexServer.Name} with id {plexServer.Id} has no libraries to sync");
                return;
            }

            // Send progress on every library update
            var progressList = new List<LibraryProgress>();

            // Initialize list
            plexLibraries.ForEach(x => progressList.Add(new LibraryProgress(x.Id, 0, x.MediaCount)));

            var progress = new Action<LibraryProgress>(libraryProgress =>
            {
                var i = progressList.FindIndex(x => x.Id == libraryProgress.Id);
                if (i != -1)
                    progressList[i] = libraryProgress;
                else
                    progressList.Add(libraryProgress);

                _signalRService.SendServerSyncProgressUpdate(new SyncServerProgress(plexServerId, progressList));
            });

            // Sync movie type libraries first because it is a lot quicker than TvShows.
            foreach (var library in plexLibraries.FindAll(x => x.Type == PlexMediaType.Movie))
            {
                var result = await _plexLibraryService.RefreshLibraryMediaAsync(library.Id, progress);
                if (result.IsFailed)
                    results.Add(result.ToResult());
            }

            foreach (var library in plexLibraries.FindAll(x => x.Type == PlexMediaType.TvShow))
            {
                var result = await _plexLibraryService.RefreshLibraryMediaAsync(library.Id, progress);
                if (result.IsFailed)
                    results.Add(result.ToResult());
            }

            if (results.Any())
            {
                var failedResult = Result.Fail($"Some libraries failed to sync in PlexServer: {plexServer.Name}");
                results.ForEach(x => { failedResult.AddNestedErrors(x.Errors); });
                failedResult.LogError();
                return;
            }

            Log.Information($"Successfully synced server {plexServerId}");
        }
        catch (Exception e)
        {
            Log.Error(e);
        }
    }
}