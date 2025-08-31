using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record CreatePlexServerConnectionEndpointRequest
{
    public required string Url { get; init; }

    public required string Protocol { get; init; }

    public required string Address { get; init; }

    public required int Port { get; init; }

    public required int PlexServerId { get; set; }
};

public class CreatePlexServerConnectionEndpointRequestValidator : Validator<CreatePlexServerConnectionEndpointRequest>
{
    public CreatePlexServerConnectionEndpointRequestValidator()
    {
        RuleFor(x => x.Url).NotEmpty();
        RuleFor(x => x.Protocol).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.Port).GreaterThan(0);
        RuleFor(x => x.PlexServerId).GreaterThan(0);
    }
}

public class CreatePlexServerConnectionEndpoint
    : BaseEndpoint<CreatePlexServerConnectionEndpointRequest, ResultDTO<PlexServerConnectionDTO>>
{
    private readonly IReaparrDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexServerConnectionController;

    public CreatePlexServerConnectionEndpoint(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.ClearDefaultProduces()
                .Produces(StatusCodes.Status201Created, typeof(ResultDTO<PlexServerConnectionDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(CreatePlexServerConnectionEndpointRequest req, CancellationToken ct)
    {
        var connection = new PlexServerConnection
        {
            Id = 0,
            Protocol = req.Protocol.ToLower(),
            Address = req.Address,
            Port = req.Port,
            Url = req.Url,
            Local = req.Address.IsLocalUrl(),
            Relay = req.Address.Contains("plex.direct"),
            IPv4 = req.Address.IsIpv4(),
            IPv6 = req.Address.IsIPv6(),
            PlexServerId = req.PlexServerId,
            IsCustom = true,
        };
        _dbContext.PlexServerConnections.Add(connection);

        await _dbContext.SaveChangesAsync(ct);

        var result = ResultExtensions.Create201CreatedResult(connection.ToDTO());
        await SendFluentResult(result, ct);
    }
}
