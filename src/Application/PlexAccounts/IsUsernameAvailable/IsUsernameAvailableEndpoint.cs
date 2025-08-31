using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

/// <summary>
/// Checks if an <see cref="PlexAccount"/> with the same username already exists.
/// </summary>
/// <returns>true if the username is available.</returns>
public class IsUsernameAvailableEndpointRequest
{
    [QueryParam, BindFrom("username")]
    public required string Username { get; init; }
}

public class IsUsernameAvailableEndpointRequestValidator : Validator<IsUsernameAvailableEndpointRequest>
{
    public IsUsernameAvailableEndpointRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Username).MinimumLength(5);
    }
}

public class IsUsernameAvailableEndpoint : BaseEndpoint<IsUsernameAvailableEndpointRequest, bool>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/check";

    public IsUsernameAvailableEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<bool>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(IsUsernameAvailableEndpointRequest req, CancellationToken ct)
    {
        var isUsernameAvailable = await _dbContext.IsUsernameAvailable(req.Username, ct);
        await SendFluentResult(Result.Ok(isUsernameAvailable), x => x, ct);
    }
}
