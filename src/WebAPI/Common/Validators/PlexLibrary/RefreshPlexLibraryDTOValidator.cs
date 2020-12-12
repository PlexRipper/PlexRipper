using FluentValidation;
using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Common.Validators
{
    public class RefreshPlexLibraryDTOValidator : AbstractValidator<RefreshPlexLibraryDTO>
    {
        public RefreshPlexLibraryDTOValidator()
        {
            RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        }
    }
}