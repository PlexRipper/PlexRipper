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
    /// <summary>
    /// Checks if an <see cref="PlexAccount"/> with the same username already exists.
    /// </summary>
    /// <param name="username">The username to check for.</param>
    /// <returns>true if the username is available.</returns>
    public IsUsernameAvailableEndpointRequest(string username)
    {
        Username = username;
    }

    [QueryParam]
    public string Username { get; init; }
}

public class IsUsernameAvailableEndpointRequestValidator : Validator<IsUsernameAvailableEndpointRequest>
{
    public IsUsernameAvailableEndpointRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Username).MinimumLength(5);
    }
}

public class IsUsernameAvailableEndpoint : BaseCustomEndpoint<IsUsernameAvailableEndpointRequest, ResultDTO<bool>>
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
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<bool>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(IsUsernameAvailableEndpointRequest req, CancellationToken ct)
    {
        await SendResult(Result.Ok(_dbContext.IsUsernameAvailable(req.Username, ct)), ct);
    }
}