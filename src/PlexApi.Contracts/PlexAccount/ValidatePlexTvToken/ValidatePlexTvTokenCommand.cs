using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
///  /// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to log in.
/// </summary>
/// <param name="PlexAccount"> The <see cref="PlexAccount"/> to validate.</param>
public record ValidatePlexTokenCommand(PlexAccount PlexAccount) : ICommand<Result<PlexAccount>>;
