using Application.Contracts;
using FluentValidation;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public class ValidatePlexAccountCommandValidator : AbstractValidator<ValidatePlexAccountCommand>
{
    public ValidatePlexAccountCommandValidator()
    {
        RuleFor(x => x.PlexAccount).NotNull();
        RuleFor(x => x.PlexAccount.Username).NotEmpty();
        RuleFor(x => x.PlexAccount.Password).NotEmpty();
    }
}

public class ValidatePlexAccountCommandHandler : IRequestHandler<ValidatePlexAccountCommand, Result<PlexAccount>>
{
    private readonly ILog _log;
    private readonly IPlexApiService _plexApiService;

    public ValidatePlexAccountCommandHandler(ILog log, IPlexApiService plexApiService)
    {
        _log = log;
        _plexApiService = plexApiService;
    }

    public async Task<Result<PlexAccount>> Handle(ValidatePlexAccountCommand request, CancellationToken cancellationToken)
    {
        var plexSignInResult = await _plexApiService.PlexSignInAsync(request.PlexAccount);
        if (plexSignInResult.IsSuccess)
            return SignInSuccess(request, plexSignInResult);

        return IsTwoFactorAuth(plexSignInResult) ? SignInTwoFactorAuth(request) : plexSignInResult;
    }

    private static Result<PlexAccount> SignInTwoFactorAuth(ValidatePlexAccountCommand request)
    {
        request.PlexAccount.Is2Fa = true;
        return Result.Ok(request.PlexAccount);
    }

    private static bool IsTwoFactorAuth(Result<PlexAccount> plexSignInResult)
    {
        return plexSignInResult.HasPlexErrorEnterVerificationCode();
    }

    private Result<PlexAccount> SignInSuccess(ValidatePlexAccountCommand request, Result<PlexAccount> plexSignInResult)
    {
        _log.Debug("The PlexAccount with displayName {PlexAccountDisplayName} has been validated", request.PlexAccount.DisplayName);
        return plexSignInResult;
    }
}