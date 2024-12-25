using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

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
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController + "/check";

    public IsUsernameAvailableEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<bool>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(IsUsernameAvailableEndpointRequest req, CancellationToken ct)
    {
        var isUsernameAvailable = await _dbContext.IsUsernameAvailable(req.Username, ct);
        await SendFluentResult(Result.Ok(isUsernameAvailable), x => x, ct);
    }
}
