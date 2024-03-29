using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class DownloadTaskDirectory
{
    [Required]
    public required DownloadTaskType Type { get; set; }

    [Required]
    public required string RootPath { get; set; }

    public required string MovieFolder { get; set; }

    public required string TvShowFolder { get; set; }

    public required string SeasonFolder { get; set; }

    [Required]
    public required string FileName { get; set; }

    [NotMapped]
    public Result<string> GetDownloadFolderPath => Type switch
    {
        DownloadTaskType.MovieData => Result.Ok(Path.Combine(RootPath, "Movies", MovieFolder)),
        DownloadTaskType.EpisodeData => Result.Ok(Path.Combine(RootPath, "TvShows", TvShowFolder, SeasonFolder)),
        _ => Result.Fail<string>($"Invalid DownloadTaskType of type: {Type}").LogError(),
    };

    [NotMapped]
    public Result<string> GetDestinationFolderPath => Type switch
    {
        DownloadTaskType.MovieData => Result.Ok(Path.Combine(RootPath, MovieFolder)),
        DownloadTaskType.EpisodeData => Result.Ok(Path.Combine(RootPath, TvShowFolder, SeasonFolder)),
        _ => Result.Fail<string>($"Invalid DownloadTaskType of type: {Type}").LogError(),
    };
}