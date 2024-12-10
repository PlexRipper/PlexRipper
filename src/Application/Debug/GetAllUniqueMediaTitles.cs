using System.Text.Json;
using System.Text.RegularExpressions;
using Application.Contracts;
using Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PlexRipper.Application;

public record GetAllUniqueMediaTitlesEndpointRequest
{
    [FromQuery]
    public PlexMediaType Type { get; init; } = PlexMediaType.TvShow;

    [FromQuery]
    public int Count { get; init; } = 0;
}

public class GetAllUniqueMediaTitlesEndpoint : BaseEndpoint<GetAllUniqueMediaTitlesEndpointRequest, List<string>>
{
    private readonly IPlexRipperDbContext _dbContext;

    private readonly Random _random = new();

    public override string EndpointPath => ApiRoutes.DebugController + "/unique-media-titles";

    public GetAllUniqueMediaTitlesEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(List<string>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllUniqueMediaTitlesEndpointRequest req, CancellationToken ct)
    {
        var query = req.Type switch
        {
            PlexMediaType.Movie => _dbContext.PlexMovies.Select(x => x.Title),
            PlexMediaType.TvShow => _dbContext.PlexTvShows.Select(x => x.Title),
            _ => throw new NotSupportedException($"Type {req.Type} is not supported."),
        };

        var result = query
            .Where(x => Regex.IsMatch(x, @"^[a-zA-Z0-9\-: ]+$"))
            .AsEnumerable()
            .OrderByNatural(x => x)
            .Distinct()
            .Select(x => x.Trim())
            .ToList();

        var resultCount = result.Count;
        string json;

        if (req.Count > 0)
        {
            var takenList = new HashSet<string>();
            for (var i = 0; i < req.Count; i++)
            {
                string selected;
                do
                {
                    var index = _random.Next(0, resultCount);
                    selected = result[index];
                }
                // Retry if the value is already in the list
                while (takenList.Contains(selected));

                // Add the unique result to the HashSet
                takenList.Add(selected);
            }

            var orderedList = takenList.OrderByNatural(x => x).ToList();
            json = Regex.Unescape(JsonSerializer.Serialize(orderedList, DefaultJsonSerializerOptions.ConfigStandard));
        }
        else
        {
            json = Regex.Unescape(JsonSerializer.Serialize(result, DefaultJsonSerializerOptions.ConfigStandard));
        }

        await SendStringAsync(json, cancellation: ct);
    }
}
