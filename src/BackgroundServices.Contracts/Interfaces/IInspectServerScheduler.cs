using FluentResults;

namespace BackgroundServices.Contracts;

public interface IInspectServerScheduler
{
    Task<Result> QueueInspectPlexServerJob(int plexServerId);

    Task<Result> QueueRefreshAccessiblePlexServersJob(int plexAccountId);

    Task<Result> QueueInspectPlexServerByPlexAccountIdJob(int plexAccountId);
}