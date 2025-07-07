using Domain.Entities;
using Settings.Contracts;
using Microsoft.Extensions.Logging;

namespace PlexRipper.Application;

public interface ITorznabWebhookService
{
    Task NotifyDownloadCompleted(DownloadTaskGeneric downloadTask, CancellationToken cancellationToken = default);
    Task NotifyDownloadStarted(DownloadTaskGeneric downloadTask, CancellationToken cancellationToken = default);
    Task NotifyDownloadFailed(DownloadTaskGeneric downloadTask, string error, CancellationToken cancellationToken = default);
}

public class TorznabWebhookService : ITorznabWebhookService
{
    private readonly IUserSettings _userSettings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TorznabWebhookService> _logger;

    public TorznabWebhookService(
        IUserSettings userSettings,
        HttpClient httpClient,
        ILogger<TorznabWebhookService> logger)
    {
        _userSettings = userSettings;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyDownloadCompleted(DownloadTaskGeneric downloadTask, CancellationToken cancellationToken = default)
    {
        if (!ShouldSendWebhook()) return;

        var payload = CreateWebhookPayload(downloadTask, "completed");
        await SendWebhook(payload, cancellationToken);
    }

    public async Task NotifyDownloadStarted(DownloadTaskGeneric downloadTask, CancellationToken cancellationToken = default)
    {
        if (!ShouldSendWebhook()) return;

        var payload = CreateWebhookPayload(downloadTask, "started");
        await SendWebhook(payload, cancellationToken);
    }

    public async Task NotifyDownloadFailed(DownloadTaskGeneric downloadTask, string error, CancellationToken cancellationToken = default)
    {
        if (!ShouldSendWebhook()) return;

        var payload = CreateWebhookPayload(downloadTask, "failed", error);
        await SendWebhook(payload, cancellationToken);
    }

    private bool ShouldSendWebhook()
    {
        return _userSettings.TorznabSettings.IsEnabled &&
               _userSettings.TorznabSettings.EnableWebhookNotifications &&
               !string.IsNullOrWhiteSpace(_userSettings.TorznabSettings.WebhookUrl);
    }

    private object CreateWebhookPayload(DownloadTaskGeneric downloadTask, string status, string? error = null)
    {
        var payload = new
        {
            EventType = "download_" + status,
            Timestamp = DateTime.UtcNow,
            Download = new
            {
                Id = downloadTask.Id,
                Title = downloadTask.Title,
                Type = downloadTask.MediaType.ToString().ToLowerInvariant(),
                Status = status,
                Progress = downloadTask.Percentage,
                Size = downloadTask.DataTotal,
                Downloaded = downloadTask.DataReceived,
                FilePath = downloadTask.DestinationFilePath,
                PlexServerId = downloadTask.PlexServerId,
                PlexLibraryId = downloadTask.PlexLibraryId,
                Error = error
            }
        };

        return payload;
    }

    private async Task SendWebhook(object payload, CancellationToken cancellationToken)
    {
        try
        {
            var webhookUrl = _userSettings.TorznabSettings.WebhookUrl;
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(webhookUrl, content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Webhook notification sent successfully to {WebhookUrl}", webhookUrl);
            }
            else
            {
                _logger.LogWarning("Webhook notification failed with status {StatusCode} to {WebhookUrl}", 
                    response.StatusCode, webhookUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send webhook notification to {WebhookUrl}", 
                _userSettings.TorznabSettings.WebhookUrl);
        }
    }
}