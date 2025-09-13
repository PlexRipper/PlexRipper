using FastEndpoints;
using FluentValidation;
using Reaparr.PublicAPI.SearchTvShow;

public record SearchTvShowCommand : ICommand<TorznabMediaSearchResponseDTO>
{
    public required string Query { get; init; }

    public required int Season { get; init; }

    public required int Episode { get; init; }

    public required int TVDB_ID { get; init; }
}

public class SearchTvShowCommandValidator : AbstractValidator<SearchTvShowCommand>
{
    public SearchTvShowCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class SearchTvShowCommandHandler : ICommandHandler<SearchTvShowCommand, TorznabMediaSearchResponseDTO>
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly IEventPublisher _eventPublisher;

    public SearchTvShowCommandHandler(ICommandExecutor commandExecutor, IEventPublisher eventPublisher)
    {
        _commandExecutor = commandExecutor;
        _eventPublisher = eventPublisher;
    }

    public async Task<TorznabMediaSearchResponseDTO> ExecuteAsync(
        SearchTvShowCommand command,
        CancellationToken cancellationToken)
    {

        await Task.CompletedTask;
        return new TorznabMediaSearchResponseDTO
        {
            Channel = new TorznabChannel
            {
                Title = "PlexRipper Torznab",
                Description = $"TV Search results for {command.Query}",
                Items =
                {
                    new TorznabItem
                    {
                        Title = $"{command.Query}.S{command.Season:00}E{command.Episode:00}.1080p.WEBRip",
                        Guid = new TorznabGuid { Value = Guid.NewGuid().ToString() },
                        Link = $"http://localhost:5000/api/public/download/{Guid.NewGuid()}",
                        PubDate = DateTime.UtcNow.ToString("R"),
                        Size = 2147483648,
                        Enclosure = new TorznabEnclosure
                        {
                            Url = $"http://localhost:5000/api/public/download/{Guid.NewGuid()}",
                            Length = 2147483648,
                            Type = "application/x-bittorrent",
                        },
                        Attributes =
                        {
                            new TorznabAttr { Name = "category", Value = "5030" },
                            new TorznabAttr { Name = "infohash", Value = "ABCDEF123456..." },
                            new TorznabAttr { Name = "seeders", Value = "100" },
                            new TorznabAttr { Name = "peers", Value = "120" },
                            new TorznabAttr { Name = "language", Value = "English" },
                            new TorznabAttr { Name = "downloadvolumefactor", Value = "1" },
                            new TorznabAttr { Name = "uploadvolumefactor", Value = "1" },
                            new TorznabAttr { Name = "tvdbid", Value = "81189" }
                        },
                    },
                },
            },
        };
    }
}