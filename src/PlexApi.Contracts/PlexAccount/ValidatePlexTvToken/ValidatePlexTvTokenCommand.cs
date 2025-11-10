using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record ValidatePlexTokenCommand(string AuthenticationToken) : ICommand<Result<ValidatePlexTokenCommandResult>>;
