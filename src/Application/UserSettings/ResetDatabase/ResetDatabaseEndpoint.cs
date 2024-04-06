using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class ResetDatabaseEndpoint : BaseEndpointWithoutRequest<ResultDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.SettingsController + "/resetdb";

    public ResetDatabaseEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x => x
            .Produces(StatusCodes.Status200OK, typeof(ResultDTO))
            .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = Result.Ok();

        await SendFluentResult(result, ct);

        throw new NotImplementedException();
    }
}