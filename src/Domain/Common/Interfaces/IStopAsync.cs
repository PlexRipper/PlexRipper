namespace Reaparr.Domain;

public interface IStopAsync
{
    public Task<Result> StopAsync();
}
