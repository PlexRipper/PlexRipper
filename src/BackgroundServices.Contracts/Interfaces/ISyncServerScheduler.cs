using FluentResults;
using PlexRipper.Domain;

namespace BackgroundServices.Contracts;

public interface ISyncServerScheduler
{
    /// <summary>
    /// Take all <see cref="PlexLibrary">PlexLibraries.</see> and retrieve all data to then store in the database.
    /// </summary>
    /// <param name="plexServerId">The id of the <see cref="PlexServer"/> to use.</param>
    /// <param name="forceSync">By default, the libraries which have been synced less than 6 hours ago will be skipped. </param>
    /// <returns><see cref="Result"/></returns>
    Task<Result> QueueSyncPlexServerJob(int plexServerId, bool forceSync = false);
}