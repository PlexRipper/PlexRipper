using FluentResults;

namespace BackgroundServices.Contracts;

public interface IInspectServerScheduler
{
    Task<Result> QueueRefreshAccessiblePlexServersJob(int plexAccountId);
}