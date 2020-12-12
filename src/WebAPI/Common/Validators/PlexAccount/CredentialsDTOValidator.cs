using FluentValidation;
using PlexRipper.PlexApi.Common.DTO;

namespace PlexRipper.WebAPI.Common.Validators
{
    public class CredentialsDTOValidator : AbstractValidator<CredentialsDTO>
    {
        public CredentialsDTOValidator()
        {
            RuleFor(x => x.Username).NotEmpty().MinimumLength(5);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(5);
        }
    }
}