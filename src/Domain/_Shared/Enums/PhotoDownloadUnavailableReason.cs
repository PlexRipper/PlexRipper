namespace Reaparr.Domain;

/// <summary>Explains why a catalog photo cannot currently be selected as a direct download.</summary>
public enum PhotoDownloadUnavailableReason
{
    /// <summary>The asset has no placement below an active configured source root.</summary>
    Unmapped = 0,
}
