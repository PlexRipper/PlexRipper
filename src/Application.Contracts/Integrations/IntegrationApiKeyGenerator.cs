using System.Security.Cryptography;

namespace Reaparr.Application.Contracts;

public static class IntegrationApiKeyGenerator
{
    private const string Q_BITTORRENT_ALPHABET = "23456789ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";

    public static string GenerateQBittorrentApiKey() =>
        $"qbt_{RandomNumberGenerator.GetString(Q_BITTORRENT_ALPHABET, 28)}";

    public static string GenerateTorznabApiKey() => Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
}
