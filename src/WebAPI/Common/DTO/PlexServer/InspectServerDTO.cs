using FluentValidation;
using Newtonsoft.Json;

namespace PlexRipper.WebAPI.Common.DTO;

public class InspectServerDTO
{
    [JsonProperty("plexAccountId", Required = Required.Always)]
    public int PlexAccountId { get; set; }

    [JsonProperty("plexServerIds", Required = Required.Always)]
    public List<int> PlexServerIds { get; set; }
}

public class InspectServerDTOValidator : AbstractValidator<InspectServerDTO>
{
    public InspectServerDTOValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.PlexAccountId).NotNull().GreaterThan(0);
        RuleFor(x => x.PlexServerIds).NotNull().NotEmpty();
        RuleFor(x => x.PlexServerIds.First())
            .NotEqual(0)
            .When(x => x.PlexServerIds is { Count: > 0 });
    }
}