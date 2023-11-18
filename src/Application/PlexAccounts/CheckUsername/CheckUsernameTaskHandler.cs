using Application.Contracts;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;

namespace PlexRipper.Application.PlexAccounts.CheckUsername;

public class CheckUsernameTaskValidator : AbstractValidator<CheckUsernameTaskQuery>
{
    #region Constructors

    public CheckUsernameTaskValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Username).MinimumLength(5);
    }

    #endregion
}

public class CheckUsernameTaskHandler : IRequestHandler<CheckUsernameTaskQuery, Result<bool>>
{
    #region Fields

    private readonly ILog _log;
    private readonly IMediator _mediator;

    #endregion

    #region Constructors

    public CheckUsernameTaskHandler(ILog log, IMediator mediator)
    {
        _log = log;
        _mediator = mediator;
    }

    #endregion

    #region Methods

    #region Public

    public async Task<Result<bool>> Handle(CheckUsernameTaskQuery request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPlexAccountByUsernameQuery(request.Username), cancellationToken);

        if (result.Has404NotFoundError())
            return Result.Ok(true);

        if (result.IsFailed)
            return result.ToResult();

        if (result.Value != null)
        {
            _log.Warning("An Account with the username: {UserName} already exists", request.Username);
            return Result.Ok(false);
        }

        _log.Debug("The username: {UserName} is available", request.Username);
        return Result.Ok(true);
    }

    #endregion

    #endregion
}