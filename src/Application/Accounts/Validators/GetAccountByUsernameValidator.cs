using FluentValidation;
using PlexRipper.Application.Accounts.Queries;

namespace PlexRipper.Application.Accounts.Validators
{
    public class GetAccountByUsernameValidator : AbstractValidator<GetAccountByUsernameQuery>
    {
        public GetAccountByUsernameValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
        }
    }
}
