using System.Collections.Concurrent;

namespace Reaparr.PublicAPI;

public class DownloadClientSessionManager : IDownloadClientSessionManager
{
    private static readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
    private static readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(30);

    public SessionInfo CreateSession(string username)
    {
        var sid = Guid.NewGuid().ToString("N");
        var session = new SessionInfo
        {
            Sid = sid,
            ExpiresAt = DateTimeOffset.UtcNow.Add(_defaultTtl),
            Username = username
        };
        _sessions[sid] = session;
        return session;
    }

    public bool IsValidSession(string sid)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return false;

        if (!_sessions.TryGetValue(sid, out var session))
            return false;

        if (session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _sessions.TryRemove(sid, out _);
            return false;
        }

        // sliding expiration
        session.ExpiresAt = DateTimeOffset.UtcNow.Add(_defaultTtl);
        return true;
    }

    public void Remove(string sid)
    {
        if (string.IsNullOrWhiteSpace(sid))
            return;

        _sessions.TryRemove(sid, out _);
    }

    public void CleanupExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var kv in _sessions)
        {
            if (kv.Value.ExpiresAt <= now)
                _sessions.TryRemove(kv.Key, out _);
        }
    }
}

public sealed class SessionInfo
{
    public required string Sid { get; init; }
    public required DateTimeOffset ExpiresAt { get; set; }
    public string? Username { get; init; }
}