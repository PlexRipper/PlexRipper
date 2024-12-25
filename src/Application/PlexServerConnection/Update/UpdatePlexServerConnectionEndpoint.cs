using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record UpdatePlexServerConnectionEndpointRequest()
{
    public required int Id { get; init; }

    public required string Url { get; init; }

    public required string Protocol { get; init; }

    public required string Address { get; init; }

    public required int Port { get; init; }

    public required int PlexServerId { get; set; }
};

public class UpdatePlexServerConnectionEndpointRequestValidator : Validator<UpdatePlexServerConnectionEndpointRequest>
{
    public UpdatePlexServerConnectionEndpointRequestValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.Url).NotEmpty();
        RuleFor(x => x.Protocol).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.Port).GreaterThan(0);
    }
}

public class UpdatePlexServerConnectionEndpoint
    : BaseEndpoint<UpdatePlexServerConnectionEndpointRequest, ResultDTO<PlexServerConnectionDTO>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController;

    public UpdatePlexServerConnectionEndpoint(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Patch(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexServerConnectionDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status404NotFound, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(UpdatePlexServerConnectionEndpointRequest req, CancellationToken ct)
    {
        var connection = new PlexServerConnection
        {
            Id = req.Id,
            Protocol = req.Protocol.ToLower(),
            Address = req.Address,
            Port = req.Port,
            Uri = req.Url,
            Local = req.Address.IsLocalUrl(),
            Relay = req.Address.Contains("plex.direct"),
            IPv4 = req.Address.IsIpv4(),
            IPv6 = req.Address.IsIPv6(),
            PlexServerId = req.PlexServerId,
            IsCustom = true,
        };

        _dbContext.PlexServerConnections.Update(connection);

        await _dbContext.SaveChangesAsync(ct);

        var connectionDb = await _dbContext.PlexServerConnections.GetAsync(req.Id, ct);

        var result = ResultExtensions.Create200OkResult(connectionDb!.ToDTO());
        await SendFluentResult(result, ct);
    }
}
