using System.Net;
using FastEndpoints;
using FluentValidation;
using Flurl;
using Reaparr.Data.Contracts;
using Reaparr.FluentResultExtensions;

namespace Reaparr.Application;

/// <summary>
/// Some <see cref="PlexServer"/> need a download=1 param, and some explicitly don't.
/// This command returns the valid DownloadURL containing either download=1 query or not
/// </summary>
public record GetDirectDownloadUrlCommand(int PlexServerId, string FileLocationUrl) : ICommand<Result<string>>;

public class GetDirectDownloadUrlCommandValidator : AbstractValidator<GetDirectDownloadUrlCommand>
{
    public GetDirectDownloadUrlCommandValidator()
    {
        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.FileLocationUrl).NotEmpty();
    }
}

public class GetDirectDownloadUrlCommandHandler : ICommandHandler<GetDirectDownloadUrlCommand, Result<string>>
{
    private readonly HttpClient _httpClient;
    private readonly IReaparrDbContext _dbContext;

    public GetDirectDownloadUrlCommandHandler(IHttpClientFactory httpClientFactory, IReaparrDbContext dbContext)
    {
        _httpClient = httpClientFactory.CreateClient();
        _dbContext = dbContext;
    }

    public async Task<Result<string>> ExecuteAsync(
        GetDirectDownloadUrlCommand command,
        CancellationToken cancellationToken
    )
    {
        var downloadUrlResult = await _dbContext.GetDownloadUrl(
            command.PlexServerId,
            command.FileLocationUrl,
            cancellationToken
        );
        if (downloadUrlResult.IsFailed)
            return downloadUrlResult.ToResult();

        var defaultDownloadUrl = downloadUrlResult.Value;
        var initialProbeResult = await ProbeDownloadUrl(defaultDownloadUrl, cancellationToken);
        if (initialProbeResult.IsFailed)
            return initialProbeResult.ToResult();

        if (initialProbeResult.Value.IsSuccessStatusCode)
            return Result.Ok(defaultDownloadUrl);

        var statusCode = initialProbeResult.Value.StatusCode;

        if (statusCode != System.Net.HttpStatusCode.Forbidden)
        {
            return Result
                .Fail($"Plex download URL probe failed with status {(int)statusCode} ({statusCode})")
                .LogError();
        }

        var downloadUrlWithFlag = defaultDownloadUrl.SetQueryParam("download", 1).ToString();
        var fallbackProbeResult = await ProbeDownloadUrl(downloadUrlWithFlag, cancellationToken);
        if (fallbackProbeResult.IsFailed)
            return fallbackProbeResult.ToResult();

        if (fallbackProbeResult.Value.IsSuccessStatusCode)
            return Result.Ok(downloadUrlWithFlag);

        return Result
            .Fail(
                $"Plex download URL probe failed for both default URL and URL with download=1 flag. "
                    + $"Default URL status: {(int)initialProbeResult.Value.StatusCode} ({initialProbeResult.Value.StatusCode}). "
                    + $"Fallback URL status: {(int)fallbackProbeResult.Value.StatusCode} ({fallbackProbeResult.Value.StatusCode})"
            )
            .LogError();
    }

    private async Task<Result<ProbeResult>> ProbeDownloadUrl(string downloadUrl, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
        var responseResult = await _httpClient.SendResultAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (responseResult.IsFailed && responseResult.IsServerUnreachable())
        {
            using var failedResponse = responseResult.ToHttpResponseMessage(request);
            return Result.Fail<ProbeResult>(responseResult.Errors);
        }

        if (responseResult.IsFailed)
        {
            using var failedResponse = responseResult.ToHttpResponseMessage(request);
            return Result.Ok(new ProbeResult(failedResponse.StatusCode, failedResponse.IsSuccessStatusCode));
        }

        using var response = responseResult.Value;
        return Result.Ok(new ProbeResult(response.StatusCode, response.IsSuccessStatusCode));
    }

    private sealed record ProbeResult(HttpStatusCode StatusCode, bool IsSuccessStatusCode);
}
