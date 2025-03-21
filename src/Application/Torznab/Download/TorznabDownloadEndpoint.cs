using Application.Contracts;
using FastEndpoints;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Settings.Contracts;

namespace PlexRipper.Application.Torznab;

/// <summary>
/// Request model for Torznab download requests from Sonarr/Radarr
/// </summary>
public record TorznabDownloadRequest
{
    [QueryParam, BindFrom("apikey")]
    public string? ApiKey { get; init; }

    [QueryParam, BindFrom("id")]
    public int MediaId { get; init; }

    [QueryParam, BindFrom("type")]
    public string? MediaType { get; init; }
}

public class TorznabDownloadRequestValidator : Validator<TorznabDownloadRequest>
{
    public TorznabDownloadRequestValidator()
    {
        RuleFor(x => x.MediaId).GreaterThan(0).WithMessage("Valid media ID is required");
    }
}

/// <summary>
/// Endpoint that handles download requests from Torznab clients like Sonarr/Radarr and initiates
/// the download through PlexRipper's existing download system
/// </summary>
public class TorznabDownloadEndpoint : Endpoint<TorznabDownloadRequest>
{
    private readonly IMediator _mediator;
    private readonly TorznabSettingsModule _torznabSettings;

    public override void Configure()
    {
        Get(ApiRoutes.TorznabDownloadEndpoint);
        AllowAnonymous(); // Torznab clients will use API key for auth
        Description(b => 
            b.Produces(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized));
    }

    public TorznabDownloadEndpoint(IMediator mediator, TorznabSettingsModule torznabSettings)
    {
        _mediator = mediator;
        _torznabSettings = torznabSettings;
    }

    public override async Task HandleAsync(TorznabDownloadRequest req, CancellationToken ct)
    {
        try
        {
            // Validate API key if enabled (check settings)
            if (!IsValidApiKey(req.ApiKey))
            {
                await SendUnauthorizedResponse(ct);
                return;
            }

            // Create a download media DTO for the requested media
            var downloadMediaDto = new DownloadMediaDTO
            {
                MediaIds = new List<int> { req.MediaId },
                // Based on the media type set the appropriate type
                Type = DetermineMediaType(req.MediaType),
                PlexServerId = 1, // Default to first server
                PlexLibraryId = 1 // Default to first library
            };

            // Determine the correct command based on the media type
            Result result;
            if (downloadMediaDto.Type == PlexMediaType.Movie)
            {
                result = await _mediator.Send(
                    new GenerateDownloadTaskMoviesCommand(new List<DownloadMediaDTO> { downloadMediaDto }),
                    ct);
            }
            else if (downloadMediaDto.Type == PlexMediaType.TvShow)
            {
                result = await _mediator.Send(
                    new GenerateDownloadTaskTvShowsCommand(new List<DownloadMediaDTO> { downloadMediaDto }),
                    ct);
            }
            else
            {
                await SendErrorResponse("Unsupported media type", ct);
                return;
            }

            if (result.IsFailed)
            {
                await SendErrorResponse($"Failed to create download task: {result.Errors.FirstOrDefault()?.Message}", ct);
                return;
            }

            // Normally with Torznab we would redirect to a .torrent or .nzb file
            // However we're handling the download ourselves so we'll return a success response

            // Return a simple success response (NZB format is just XML)
            var successResponse = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<nzb xmlns=""http://www.newzbin.com/DTD/2003/nzb"">
  <head>
    <meta type=""title"">PlexRipper Download {req.MediaId}</meta>
    <meta type=""tag"">Plex</meta>
  </head>
  <file id=""{req.MediaId}"">
    <groups>
      <group>alt.binaries.media</group>
    </groups>
    <segments>
      <segment bytes=""1"" number=""1"">dummy-segment</segment>
    </segments>
  </file>
</nzb>";

            await SendStringAsync(successResponse, contentType: "application/xml", cancellation: ct);
        }
        catch (Exception ex)
        {
            await SendErrorResponse($"Error processing download request: {ex.Message}", ct);
        }
    }

    private PlexMediaType DetermineMediaType(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            // Default to movie if not specified
            return PlexMediaType.Movie;
        }

        return mediaType.ToLowerInvariant() switch
        {
            "movie" => PlexMediaType.Movie,
            "tv" or "tvseries" or "series" => PlexMediaType.TvShow,
            _ => PlexMediaType.Movie // Default to movie for unknown types
        };
    }

    private async Task SendErrorResponse(string message, CancellationToken ct)
    {
        var errorResponse = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<error code=""100"" description=""{message}"" />";

        await SendStringAsync(errorResponse, 400, "application/xml", ct);
    }

    private async Task SendUnauthorizedResponse(CancellationToken ct)
    {
        var errorResponse = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<error code=""101"" description=""Invalid API Key"" />";

        await SendStringAsync(errorResponse, 401, "application/xml", ct);
    }

    private bool IsValidApiKey(string? apiKey)
    {
        // Use the settings module to validate the API key
        return _torznabSettings.IsValidApiKey(apiKey);
    }
}
