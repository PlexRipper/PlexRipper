namespace PlexRipper.Domain;

public interface ISetupAsync
{
    /// <summary>
    /// Called on application startup to start, resume work or setup services.
    /// </summary>
    /// <returns>Result.</returns>
    public Task<Result> SetupAsync();
}