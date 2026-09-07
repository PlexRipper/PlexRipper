namespace Reaparr.Application;

public record SonarrIntegrationDTO
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required string ApiKey { get; init; }
    public required string Category { get; init; }
    public int? DownloadFolderId { get; init; }
    public required IntegrationProvisioningState ProvisioningState { get; init; }
    public TestConnectionStatus LastConnectionTestStatus { get; init; }
    public int? LastConnectionTestHttpStatusCode { get; init; }
    public string? LastConnectionTestErrorMessage { get; init; }
    public DateTime? LastConnectionTestedAt { get; init; }
}
