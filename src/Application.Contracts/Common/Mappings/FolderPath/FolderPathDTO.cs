using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public record FolderPathDTO
{
    public required int Id { get; set; }

    public required FolderType FolderType { get; set; }

    public required PlexMediaType MediaType { get; set; }

    public required string DisplayName { get; set; }

    public required string Directory { get; set; }

    public required bool IsValid { get; set; }

    public required bool IsDefault { get; set; }
}
