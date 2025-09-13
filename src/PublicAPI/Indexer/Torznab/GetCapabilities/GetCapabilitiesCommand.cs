using FastEndpoints;
using FluentValidation;

namespace Reaparr.PublicAPI;

public record GetCapabilitiesCommand : ICommand<TorznabCapsResponseDTO>;


public class GetCapabilitiesCommandValidator : AbstractValidator<GetCapabilitiesCommand>
{
    public GetCapabilitiesCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class GetCapabilitiesCommandHandler : ICommandHandler<GetCapabilitiesCommand, TorznabCapsResponseDTO>
{
    public async Task<TorznabCapsResponseDTO> ExecuteAsync(GetCapabilitiesCommand command, CancellationToken cancellationToken)
    {
        var response = new TorznabCapsResponseDTO
        {
            Xmlns = new System.Xml.Serialization.XmlSerializerNamespaces(new[]
            {
                new System.Xml.XmlQualifiedName("torznab", "http://torznab.com/schemas/2015/feed"),
            }),
            Server = new TorznabServer
            {
                Version = "1.3",
                Title = "Reaparr Indexer"
            },
            Limits = new TorznabLimits
            {
                Max = 100,
                Default = 50
            },
            Searching = new TorznabSearching
            {
                Search = new TorznabSearch
                {
                    Available = "yes",
                    SupportedParams = "q,cat,limit,offset,extended,attrs"
                },
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
            Attributes =
            [
                new TorznabCapsAttr { Name = "seeders", Value = "yes" },
                new TorznabCapsAttr { Name = "peers", Value = "yes" },
                new TorznabCapsAttr { Name = "language", Value = "en" },
                new TorznabCapsAttr { Name = "downloadvolumefactor", Value = "1.0" },
                new TorznabCapsAttr { Name = "uploadvolumefactor", Value = "1.0" },
                new TorznabCapsAttr { Name = "tags", Value = "yes" },
            ],
            Categories =
            [
                // Movies
                new TorznabCategory(2000, "Movies"),
                new TorznabCategory(2010, "Movies/Foreign"),
                new TorznabCategory(2020, "Movies/Other"),
                new TorznabCategory(2030, "Movies/SD"),
                new TorznabCategory(2040, "Movies/HD"),

                new TorznabCategory(2045, "Movies/UHD"),
                new TorznabCategory(2050, "Movies/BluRay"),
                new TorznabCategory(2060, "Movies/3D"),
                new TorznabCategory(2070, "Movies/WEBDL"),

                // TV
                new TorznabCategory(5000, "TV"),
                new TorznabCategory(5030, "TV/HD"),
                new TorznabCategory(5040, "TV/SD"),
                new TorznabCategory(5050, "TV/UHD"),
                new TorznabCategory(5070, "TV/Anime"),
                new TorznabCategory(5080, "TV/Documentary"),
                new TorznabCategory(5090, "TV/Foreign"),
            ],
        };
        
        await Task.CompletedTask;

        return response;
    }
}