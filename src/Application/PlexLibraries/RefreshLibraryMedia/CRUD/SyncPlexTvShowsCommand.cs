using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SyncPlexTvShowsCommand(List<PlexTvShow> PlexTvShows) : IRequest<Result<CrudTvShowsReport>>;

public class SyncPlexTvShowsCommandValidator : AbstractValidator<SyncPlexTvShowsCommand>
{
    public SyncPlexTvShowsCommandValidator(ILog<SyncPlexTvShowsCommandValidator> log)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        RuleFor(x => x.PlexTvShows).NotNull();
        RuleForEach(x => x.PlexTvShows)
            .ChildRules(tvShow =>
            {
                tvShow.RuleFor(x => x.Key).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexServerId).GreaterThan(0);
                tvShow.RuleForEach(y => y.Seasons).NotNull();
                tvShow.RuleForEach(a => a.Seasons).NotEmpty();

                tvShow
                    .RuleForEach(y => y.Seasons)
                    .ChildRules(season =>
                    {
                        season.RuleFor(a => a.Key).GreaterThan(0);
                        season.RuleFor(a => a.ParentKey).GreaterThan(0);
                        season.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                        season.RuleFor(y => y.PlexServerId).GreaterThan(0);

                        season.RuleForEach(a => a.Episodes).NotNull();
                        season.RuleForEach(a => a.Episodes).NotEmpty();
                        season
                            .RuleForEach(a => a.Episodes)
                            .ChildRules(episode =>
                            {
                                episode.RuleFor(c => c.Key).GreaterThan(0);
                                episode.RuleFor(c => c.ParentKey).GreaterThan(0);
                                season.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                                season.RuleFor(y => y.PlexServerId).GreaterThan(0);
                            });
                    });
            });

        stopWatch.Stop();
        log.Here()
            .Debug(
                "Finished validating {ClassName} in {TotalMilliseconds} milliseconds",
                nameof(SyncPlexTvShowsCommandValidator),
                stopWatch.Elapsed.TotalMilliseconds
            );
    }
}

public class SyncPlexTvShowsCommandHandler : IRequestHandler<SyncPlexTvShowsCommand, Result<CrudTvShowsReport>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly CrudTvShowsReport _report = new();

    private readonly BulkConfig? _config =
        new()
        {
            BatchSize = 500,
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
        };

    public SyncPlexTvShowsCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<CrudTvShowsReport>> Handle(
        SyncPlexTvShowsCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var plexLibraryId = command.PlexTvShows.First().PlexLibraryId;
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

            var plexTvShows = command.PlexTvShows;

            await _dbContext.BulkInsertAsync(plexTvShows, _config, cancellationToken);
            _report.CreatedTvShows = plexTvShows.Count;

            // Set the foreign keys (PlexTvShowId) in PlexSeason based on the inserted PlexTvShows
            var plexSeasons = plexTvShows
                .SelectMany(tvShow =>
                    tvShow.Seasons.Select(season =>
                    {
                        season.TvShowId = tvShow.Id;
                        season.Episodes.ForEach(episode => episode.TvShowId = tvShow.Id);
                        return season;
                    })
                )
                .ToList();

            // Bulk insert PlexSeasons
            await _dbContext.BulkInsertAsync(plexSeasons, _config, cancellationToken);
            _report.CreatedSeasons = plexSeasons.Count;

            // Set the foreign keys (PlexSeasonId) in PlexEpisodes based on the inserted PlexSeasons
            var plexEpisodes = plexSeasons
                .SelectMany(season =>
                {
                    season.Episodes.ForEach(episode => episode.TvShowSeasonId = season.Id);
                    return season.Episodes;
                })
                .ToList();

            // Bulk insert PlexEpisodes
            await _dbContext.BulkInsertAsync(plexEpisodes, _config, cancellationToken);
            _report.CreatedEpisodes = plexEpisodes.Count;

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
        _report.DeletedEpisodes = await _dbContext
            .PlexTvShowEpisodes.Where(e => e.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        _report.DeletedSeasons = await _dbContext
            .PlexTvShowSeason.Where(s => s.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        _report.DeletedTvShows = await _dbContext
            .PlexTvShows.Where(tv => tv.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public record CrudTvShowsReport
{
    public int CreatedTvShows { get; set; }

    public int UpdatedTvShows { get; set; }

    public int DeletedTvShows { get; set; }

    public int CreatedSeasons { get; set; }

    public int UpdatedSeasons { get; set; }

    public int DeletedSeasons { get; set; }

    public int CreatedEpisodes { get; set; }

    public int UpdatedEpisodes { get; set; }

    public int DeletedEpisodes { get; set; }

    public override string ToString() =>
        $@"
        CreatedTvShows: {CreatedTvShows}
        UpdatedTvShows: {UpdatedTvShows}
        DeletedTvShows: {DeletedTvShows}
        CreatedSeasons: {CreatedSeasons}
        UpdatedSeasons: {UpdatedSeasons}
        DeletedSeasons: {DeletedSeasons}
        CreatedEpisodes: {CreatedEpisodes}
        UpdatedEpisodes: {UpdatedEpisodes}
        DeletedEpisodes: {DeletedEpisodes}";
}
