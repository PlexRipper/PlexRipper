using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

public record ValidatePlexTokenCommand(string AuthenticationToken) : ICommand<Result<ValidatePlexTokenCommandResult>>;
