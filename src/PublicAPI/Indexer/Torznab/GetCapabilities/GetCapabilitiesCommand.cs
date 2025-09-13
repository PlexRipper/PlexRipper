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