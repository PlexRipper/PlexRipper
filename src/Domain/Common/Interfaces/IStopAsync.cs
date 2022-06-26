namespace PlexRipper.Domain;

public interface IStopAsync
{
    public Task<Result> StopAsync(bool gracefully = true);
}