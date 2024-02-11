using FluentResults;

namespace BackgroundServices.Contracts;

public interface IInspectServerScheduler
{
    Task<Result> QueueRefreshAccessiblePlexServersJob(int plexAccountId);

    Task<Result> QueueInspectPlexServerByPlexAccountIdJob(int plexAccountId);
}