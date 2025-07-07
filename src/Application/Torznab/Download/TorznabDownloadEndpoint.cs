using Application.Contracts;
using Settings.Contracts;
using Data.Contracts;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain;

namespace PlexRipper.Application;

public class TorznabDownloadEndpoint : BaseEndpoint<TorznabDownloadRequest, string>
{
    private readonly IUserSettings _userSettings;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public TorznabDownloadEndpoint(IUserSettings userSettings, IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _userSettings = userSettings;
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/torznab/download/{id}");
        AllowAnonymous(); // We'll handle API key authentication in the handler
        Summary(s =>
        {
            s.Summary = "Torznab download endpoint";
            s.Description = "Initiate download of media item via PlexRipper";
        });
    }

    public override async Task<string> ExecuteAsync(TorznabDownloadRequest req, CancellationToken ct)
    {
        // Validate API key if Torznab is enabled
        if (_userSettings.TorznabSettings.IsEnabled)
        {
            if (string.IsNullOrEmpty(req.ApiKey) || req.ApiKey != _userSettings.TorznabSettings.ApiKey)
            {
                await SendUnauthorizedAsync(ct);
                return string.Empty;
            }
        }
        else
        {
            await SendErrorsAsync(503, ct); // Service Unavailable
            return string.Empty;
        }

        if (!int.TryParse(req.Id, out var mediaId))
        {
            await SendErrorsAsync(400, ct);
            return string.Empty;
        }

        try
        {
            if (_userSettings.TorznabSettings.AutoCreateDownloadTasks)
            {
                var success = await CreateDownloadTask(mediaId, ct);
                if (success)
                {
                    // Return success response for ARR apps
                    await SendOkAsync("Download initiated successfully", ct);
                    return "Download initiated successfully";
                }
            }

            // Return download URL for manual handling
            var downloadUrl = GenerateDownloadUrl(mediaId);
            HttpContext.Response.Redirect(downloadUrl);
            return downloadUrl;
        }
        catch (Exception ex)
        {
            await SendErrorsAsync(500, ct);
            return string.Empty;
        }
    }

    private async Task<bool> CreateDownloadTask(int mediaId, CancellationToken ct)
    {
        try
        {
            // Check if it's a movie
            var movie = await _dbContext.PlexMovies.FindAsync(new object[] { mediaId }, ct);
            if (movie != null)
            {
                var downloadMedia = new DownloadMediaDTO
                {
                    MediaIds = new List<int> { mediaId },
                    Type = PlexMediaType.Movie,
                    PlexServerId = movie.PlexServerId,
                    PlexLibraryId = movie.PlexLibraryId
                };

                var command = new CreateDownloadTasksCommand(new List<DownloadMediaDTO> { downloadMedia });
                var result = await _mediator.Send(command, ct);
                return result.IsSuccess;
            }

            // Check if it's a TV show episode
            var episode = await _dbContext.PlexTvShowEpisodes.FindAsync(new object[] { mediaId }, ct);
            if (episode != null)
            {
                var downloadMedia = new DownloadMediaDTO
                {
                    MediaIds = new List<int> { mediaId },
                    Type = PlexMediaType.Episode,
                    PlexServerId = episode.PlexServerId,
                    PlexLibraryId = episode.PlexLibraryId
                };

                var command = new CreateDownloadTasksCommand(new List<DownloadMediaDTO> { downloadMedia });
                var result = await _mediator.Send(command, ct);
                return result.IsSuccess;
            }

            // Check if it's a TV show season
            var season = await _dbContext.PlexTvShowSeasons.FindAsync(new object[] { mediaId }, ct);
            if (season != null)
            {
                var downloadMedia = new DownloadMediaDTO
                {
                    MediaIds = new List<int> { mediaId },
                    Type = PlexMediaType.Season,
                    PlexServerId = season.PlexServerId,
                    PlexLibraryId = season.PlexLibraryId
                };

                var command = new CreateDownloadTasksCommand(new List<DownloadMediaDTO> { downloadMedia });
                var result = await _mediator.Send(command, ct);
                return result.IsSuccess;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private string GenerateDownloadUrl(int mediaId)
    {
        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        return $"{baseUrl}/api/PlexMedia/{mediaId}/download";
    }
}

public class TorznabDownloadRequest
{
    public string Id { get; set; } = "";
    public string? ApiKey { get; set; }
}