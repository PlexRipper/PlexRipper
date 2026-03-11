using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

public record GetTranscodeUrlCommand : ICommand<Result<GetTranscodeUrlResult>>
{
    public required DownloadTaskKey DownloadTaskKey { get; init; }

    public required string MetaDataPath { get; init; }
}

public record GetTranscodeUrlResult
{
    public required string DownloadUrl { get; init; }

    public VideoQuality TranscodedQuality { get; init; }
}
