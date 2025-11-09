namespace Reaparr.PublicAPI;

public interface IDownloadClientSessionManager
{
    SessionInfo CreateSession(string username);
    bool IsValidSession(string sid);

    void Remove(string sid);

    void CleanupExpired();
}