using FluentValidation;
using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Common.Validators
{
    public class CreatePlexAccountDTOValidator : AbstractValidator<CreatePlexAccountDTO>
    {
        public CreatePlexAccountDTOValidator()
        {
            RuleFor(x => x.Username).NotEmpty().MinimumLength(5);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(5);
            RuleFor(x => x.DisplayName).NotEmpty();
        }
    }
}
