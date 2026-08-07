namespace Reaparr.Domain;

public interface ISetupAsync
{
    /// <summary>
    /// Called on application startup to start, resume work or setup services.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Result.</returns>
    Task<Result> SetupAsync(CancellationToken cancellationToken = default);

}
