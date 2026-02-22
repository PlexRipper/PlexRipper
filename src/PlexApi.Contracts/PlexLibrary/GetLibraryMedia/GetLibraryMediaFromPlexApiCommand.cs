using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Retrieves all media metadata from the PlexApi for a given <see cref="PlexLibrary"/> and returns it as a <see cref="LibraryMetadata"/>.
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public record GetLibraryMediaFromPlexApiCommand(PlexLibrary PlexLibrary) : ICommand<Result<LibraryMetadata>>;
