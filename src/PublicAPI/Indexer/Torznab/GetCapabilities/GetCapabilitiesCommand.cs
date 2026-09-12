namespace Reaparr.PublicAPI;

public record GetCapabilitiesCommand : ICommand<Result<TorznabCapsResponseDTO>>;

public class GetCapabilitiesCommandValidator : AbstractValidator<GetCapabilitiesCommand>
{
    public GetCapabilitiesCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class GetCapabilitiesCommandHandler : ICommandHandler<GetCapabilitiesCommand, Result<TorznabCapsResponseDTO>>
{
    public Task<Result<TorznabCapsResponseDTO>> ExecuteAsync(
        GetCapabilitiesCommand command,
        CancellationToken cancellationToken
    )
    {
        var response = new TorznabCapsResponseDTO
        {
            Xmlns = new System.Xml.Serialization.XmlSerializerNamespaces([
                new System.Xml.XmlQualifiedName("torznab", "http://torznab.com/schemas/2015/feed"),
            ]),
            Server = new TorznabServer { Version = "1.3", Title = "Reaparr Indexer" },
            Limits = new TorznabLimits { Max = int.MaxValue, Default = 50 },
            Searching = new TorznabSearching
            {
                Search = new TorznabSearch { Available = "yes", SupportedParams = "q,cat,limit,offset,extended,attrs" },
                TvSearch = new TorznabSearch
                {
                    Available = "yes",
                    SupportedParams = "q,season,ep,tvdbid,imdbid,tmdbid,extended,attrs,cat,limit,offset",
                },
                MovieSearch = new TorznabSearch
                {
                    Available = "yes",
                    SupportedParams = "q,imdbid,tmdbid,extended,attrs,cat,limit,offset",
                },
            },
            Categories =
            [
                // Movies
                new TorznabCategory((int)TorznabCategoryId.Movies, "Movies"),
                new TorznabCategory((int)TorznabCategoryId.Movies_Foreign, "Movies/Foreign"),
                new TorznabCategory((int)TorznabCategoryId.Movies_SD, "Movies/SD"),
                new TorznabCategory((int)TorznabCategoryId.Movies_HD, "Movies/HD"),
                new TorznabCategory((int)TorznabCategoryId.Movies_UHD, "Movies/UHD"),
                new TorznabCategory((int)TorznabCategoryId.Movies_BluRay, "Movies/BluRay"),
                new TorznabCategory((int)TorznabCategoryId.Movies_WEBDL, "Movies/WEBDL"),
                // TV
                new TorznabCategory((int)TorznabCategoryId.TV, "TV"),
                new TorznabCategory((int)TorznabCategoryId.TV_Foreign, "TV/Foreign"),
                new TorznabCategory((int)TorznabCategoryId.TV_SD, "TV/SD"),
                new TorznabCategory((int)TorznabCategoryId.TV_HD, "TV/HD"),
                new TorznabCategory((int)TorznabCategoryId.TV_UHD, "TV/UHD"),
                new TorznabCategory((int)TorznabCategoryId.TV_Sport, "TV/Sport"),
                new TorznabCategory((int)TorznabCategoryId.TV_Anime, "TV/Anime"),
                new TorznabCategory((int)TorznabCategoryId.TV_Documentary, "TV/Documentary"),
            ],
        };

        return Task.FromResult(Result.Ok(response));
    }
}
