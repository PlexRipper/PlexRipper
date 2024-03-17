using Application.Contracts;
using FluentValidation;

namespace PlexRipper.WebAPI.Common.Validators;

public class RefreshPlexLibraryDTOValidator : AbstractValidator<RefreshPlexLibraryDTO>
{
    public RefreshPlexLibraryDTOValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}