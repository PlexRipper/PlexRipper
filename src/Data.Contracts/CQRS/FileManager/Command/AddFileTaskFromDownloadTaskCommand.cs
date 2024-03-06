using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public record AddFileTaskFromDownloadTaskCommand(DownloadTaskKey DownloadTaskKey) : IRequest<Result<int>>;