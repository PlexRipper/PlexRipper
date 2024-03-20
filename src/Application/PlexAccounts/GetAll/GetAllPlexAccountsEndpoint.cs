using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
/// </summary>
/// <param name="EnabledOnly">Should only return enabled <see cref="PlexAccount">PlexAccounts</see>.</param>
/// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
public record GetAllPlexAccountsEndpointRequest(bool EnabledOnly = false);

public class GetAllPlexAccountsEndpoint : BaseCustomEndpoint<GetAllPlexAccountsEndpointRequest, List<PlexAccountDTO>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexAccountController;

    public GetAllPlexAccountsEndpoint(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<PlexAccountDTO>>))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(GetAllPlexAccountsEndpointRequest req, CancellationToken ct)
    {
        var query = _dbContext.PlexAccounts.AsQueryable();
        if (req.EnabledOnly)
            query = query.Where(x => x.IsEnabled);

        var plexAccounts = await query.ToListAsync(ct);

        if (!plexAccounts.Any() && req.EnabledOnly)
            _log.WarningLine("Could not find any enabled accounts");
        else
            _log.Debug("Returned {PlexAccountCount} accounts", plexAccounts.Count);

        await SendFluentResult(Result.Ok(plexAccounts), x => x.ToDTO(), ct);
    }
}