namespace Reaparr.Domain;

public interface IStopAsync
{
    Task<Result> StopAsync(CancellationToken cancellationToken = default);
}
