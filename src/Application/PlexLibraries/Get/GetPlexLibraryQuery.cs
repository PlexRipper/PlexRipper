using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Returns the PlexLibrary by the Id, will refresh if the library has no media assigned.
/// Note: this will not include the media.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
/// <returns>Valid result if found.</returns>
public record GetPlexLibraryQuery(int PlexLibraryId) : IRequest<Result<PlexLibrary>>;

public class GetPlexLibraryQueryValidator : AbstractValidator<GetPlexLibraryQuery>
{
    public GetPlexLibraryQueryValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class GetPlexLibraryQueryHandler : IRequestHandler<GetPlexLibraryQuery, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public GetPlexLibraryQueryHandler(ILog log, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public async Task<Result<PlexLibrary>> Handle(GetPlexLibraryQuery command, CancellationToken cancellationToken)
    {
        var plexLibrary = await _dbContext.PlexLibraries.FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);
        if (plexLibrary is null)
            return ResultExtensions.EntityNotFound(nameof(plexLibrary), plexLibrary.Id);

        if (!plexLibrary.HasMedia)
        {
            _log.Information("PlexLibrary with id {LibraryId} has no media, forcing refresh from the PlexApi", plexLibrary.Id);

            var refreshResult = await _mediator.Send(new RefreshLibraryMediaCommand(plexLibrary.Id), cancellationToken);
            if (refreshResult.IsFailed)
                return refreshResult.ToResult();
        }

        return Result.Ok(plexLibrary);
    }
}