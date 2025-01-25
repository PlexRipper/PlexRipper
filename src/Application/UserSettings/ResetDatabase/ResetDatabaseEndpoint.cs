using Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class ResetDatabaseEndpoint : BaseEndpointWithoutRequest<BaseResultDTO>
{
    public override string EndpointPath => ApiRoutes.SettingsController + "/resetdb";

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = Result.Ok();

        await SendFluentResult(result, ct);

        throw new NotImplementedException();
    }
}
