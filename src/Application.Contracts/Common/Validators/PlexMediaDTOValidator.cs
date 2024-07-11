using FluentValidation;
using PlexRipper.Domain;

namespace Application.Contracts.Validators;

public class PlexMediaDTOValidator : AbstractValidator<PlexMediaDTO>
{
    public PlexMediaDTOValidator()
    {
        RuleFor(x => x.Id).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Index).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.SortTitle).NotEmpty();
        RuleFor(x => x.Year).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Duration).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MediaSize).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ChildCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AddedAt).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
        RuleFor(x => x.PlexLibraryId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PlexServerId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Type)
            .Must(x =>
                x == PlexMediaType.Movie
                || x == PlexMediaType.TvShow
                || x == PlexMediaType.Season
                || x == PlexMediaType.Episode
            );

        RuleFor(x => x.Summary).NotEmpty();
        RuleFor(x => x.UpdatedAt).NotEmpty();
        RuleFor(x => x.FullThumbUrl).NotEmpty().When(x => x.HasThumb);
        RuleFor(x => x.FullThumbUrl).Must(x => new Uri(x).IsAbsoluteUri).When(x => x.HasThumb);
        RuleFor(x => x.FullThumbUrl).Must(x => new Uri(x).PathAndQuery.Contains("X-Plex-Token=")).When(x => x.HasThumb);
        RuleForEach(x => x.Qualities)
            .ChildRules(y =>
            {
                y.RuleFor(z => z.Quality).NotEmpty();
                y.RuleFor(z => z.DisplayQuality).NotEmpty();
                y.RuleFor(z => z.HashId).NotEmpty();
            });
    }
}
