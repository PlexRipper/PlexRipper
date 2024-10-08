using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SyncPlexMoviesCommand(List<PlexMovie> PlexMovies) : IRequest<Result<CrudMoviesReport>>;

public class SyncPlexMoviesCommandValidator : AbstractValidator<SyncPlexMoviesCommand>
{
    public SyncPlexMoviesCommandValidator(ILog<SyncPlexMoviesCommandValidator> log)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        RuleFor(x => x.PlexMovies).NotNull();
        RuleForEach(x => x.PlexMovies)
            .ChildRules(tvShow =>
            {
                tvShow.RuleFor(x => x.Key).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexServerId).GreaterThan(0);
            });

        stopWatch.Stop();
        log.Here()
            .Debug(
                "Finished validating {ClassName} in {TotalMilliseconds} milliseconds",
                nameof(SyncPlexMoviesCommandValidator),
                stopWatch.Elapsed.TotalMilliseconds
            );
    }
}

public class SyncPlexMoviesCommandHandler : IRequestHandler<SyncPlexMoviesCommand, Result<CrudMoviesReport>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly CrudMoviesReport _report = new();

    private readonly BulkConfig? _config =
        new()
        {
            BatchSize = 500,
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
        };

    public SyncPlexMoviesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<CrudMoviesReport>> Handle(
        SyncPlexMoviesCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var plexLibraryId = command.PlexMovies.First().PlexLibraryId;
            var plexLibraryName = _dbContext
                .PlexLibraries.Where(x => x.Id == plexLibraryId)
                .Select(x => x.Title)
                .FirstOrDefault();

            _log.Debug(
                "Starting syncing of tv shows in library: {PlexLibraryName} with id:  {PlexLibraryId} by first removing all media and then reinserting it",
                plexLibraryName,
                plexLibraryId
            );

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await RemoveMedia(plexLibraryId, cancellationToken);

            var plexMovies = command.PlexMovies;

            await _dbContext.BulkInsertAsync(plexMovies, _config, cancellationToken);
            _report.CreatedMovies = plexMovies.Count;

            stopWatch.Stop();

            _log.Information(
                "Finished media syncing plexLibrary: {PlexLibraryName} with id: {PlexLibraryId} in {TotalMilliseconds} milliseconds",
                plexLibraryName,
                plexLibraryId,
                stopWatch.Elapsed.TotalMilliseconds
            );

            _log.DebugLine(_report.ToString());

            return Result.Ok(_report);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private async Task RemoveMedia(int plexLibraryId, CancellationToken cancellationToken)
    {
        _report.DeletedMovies = await _dbContext
            .PlexMovies.Where(e => e.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public record CrudMoviesReport
{
    public int CreatedMovies { get; set; }

    public int UpdatedMovies { get; set; }

    public int DeletedMovies { get; set; }

    public override string ToString() =>
        $@"
        CreatedMovies: {CreatedMovies}
        UpdatedMovies: {UpdatedMovies}
        DeletedMovies: {DeletedMovies}";
}
