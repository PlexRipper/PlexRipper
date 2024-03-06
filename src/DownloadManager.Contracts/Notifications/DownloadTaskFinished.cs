using MediatR;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public record DownloadTaskFinished(DownloadTaskKey Key) : INotification;