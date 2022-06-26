namespace PlexRipper.Application;

public interface IPlexRipperDatabaseService : ISetupAsync
{
    Result BackUpDatabase();

    Task<Result> ResetDatabase();
}