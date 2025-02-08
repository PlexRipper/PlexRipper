using Quartz;

namespace PlexRipper.Application.SyncMediaMetadata;

public class SyncMediaMetadata : IJob
{
    public SyncMediaMetadata() { }

    public async Task Execute(IJobExecutionContext context)
    {
        await Task.CompletedTask;

        // var detailMedia = await _plexApiWrapper.GetMediaMetadata(
        //     plexServerConnection,
        //     tokenResult.Value,
        //     ratingKeys,
        //     SendProgress
        // );
    }
}
