using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

/// <summary>
/// Deletes the source files from the download directory for the given file-task keys.
/// This is needed when an external tool (e.g. Sonarr/Radarr) has already imported the files
/// and Reaparr receives a delete-torrent request with <c>deleteFiles=true</c>.
/// In that scenario the tasks are in <see cref="DownloadStatus.Completed"/> state, so
/// <see cref="StopDownloadTaskCommand"/> intentionally skips file deletion. This command
/// fills that gap by explicitly removing the files still present in the download directory.
/// Only file-type keys (<see cref="DownloadTaskType.MovieData"/>, <see cref="DownloadTaskType.MoviePart"/>,
/// <see cref="DownloadTaskType.EpisodeData"/>, <see cref="DownloadTaskType.EpisodePart"/>) are meaningful here.
/// </summary>
public record DeleteDownloadTaskFilesCommand(List<DownloadTaskKey> Keys) : ICommand<Result>;
