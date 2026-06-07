namespace Reaparr.Application;

public class ResetDatabaseEndpoint : BaseEndpointWithoutRequest<BaseResultDTO>
{
    public override void Configure()
    {
        Get(ApiRoutes.SettingsController + "/resetdb");

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = Result.Ok();

        await Send.FluentResult(result, ct);

        throw new NotImplementedException();
    }
}
