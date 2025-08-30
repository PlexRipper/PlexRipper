using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Retrieves all accessible <see cref="PlexLibrary"/> from this <see cref="PlexServer"/> by the given <see cref="PlexAccount"/>.
/// </summary>
/// <param name="PlexServerId"> The <see cref="PlexServer"/> to use.</param>
/// <param name="PlexAccountId"> The <see cref="PlexAccount"/> to use.</param>
/// <returns>List of accessible <see cref="PlexLibrary"/>.</returns>
public record GetLibrarySectionsCommand(int PlexServerId, int PlexAccountId = 0) : ICommand<Result<List<PlexLibrary>>>;
