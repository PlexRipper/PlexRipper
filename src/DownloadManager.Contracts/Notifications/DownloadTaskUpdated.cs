using MediatR;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public record DownloadTaskUpdated(DownloadTaskKey Key) : IRequest;