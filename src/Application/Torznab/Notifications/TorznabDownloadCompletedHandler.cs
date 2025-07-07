using Application.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using PlexRipper.Domain;

namespace PlexRipper.Application;

public class TorznabDownloadCompletedHandler : INotificationHandler<DownloadTaskUpdatedNotification>
{
    private readonly ITorznabWebhookService _webhookService;
    private readonly ILogger<TorznabDownloadCompletedHandler> _logger;

    public TorznabDownloadCompletedHandler(
        ITorznabWebhookService webhookService,
        ILogger<TorznabDownloadCompletedHandler> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
    }

    public async Task Handle(DownloadTaskUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var downloadTask = notification.DownloadTask;
        
        try
        {
            // Check if download just completed
            if (downloadTask.DownloadStatus == DownloadStatus.Completed)
            {
                await _webhookService.NotifyDownloadCompleted(downloadTask, cancellationToken);
            }
            // Check if download just started
            else if (downloadTask.DownloadStatus == DownloadStatus.Downloading)
            {
                await _webhookService.NotifyDownloadStarted(downloadTask, cancellationToken);
            }
            // Check if download failed
            else if (downloadTask.DownloadStatus == DownloadStatus.Error)
            {
                var error = $"Download failed with status: {downloadTask.DownloadStatus}";
                await _webhookService.NotifyDownloadFailed(downloadTask, error, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Torznab webhook notification for download task {DownloadTaskId}", 
                downloadTask.Id);
        }
    }
}