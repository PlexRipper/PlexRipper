﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentResults;
using PlexRipper.Domain;
using PlexRipper.WebAPI.Common.DTO;

namespace PlexRipper.WebAPI.Common
{
    public static class ControllerHelpers
    {
        public static Result<List<DownloadTaskDTO>> ConvertToDownloadTaskDTOHierarchy(List<DownloadTask> downloadTasks, IMapper mapper)
        {
            try
            {
                var convertedDownloadTasks = new List<DownloadTaskDTO>();
                foreach (var downloadTask in downloadTasks)
                {
                    var mappedDownloadTask = mapper.Map<DownloadTaskDTO>(downloadTask);

                    // Convert movie typed downloadTasks
                    if (downloadTask.MediaType == PlexMediaType.Movie)
                    {
                        mappedDownloadTask.Key = downloadTask.Id;
                        mappedDownloadTask.Title = downloadTask.TitleMovie;
                        convertedDownloadTasks.Add(mappedDownloadTask);
                        continue;
                    }

                    // Convert episode typed downloadTasks
                    if (downloadTask.MediaType == PlexMediaType.Episode)
                    {
                        var tvShowDownloadTask = convertedDownloadTasks.Find(x =>
                            x.Key == downloadTask.MetaData.TvShowKey && x.PlexLibraryId == downloadTask.PlexLibraryId);
                        if (tvShowDownloadTask is null)
                        {
                            tvShowDownloadTask = new DownloadTaskDTO
                            {
                                Key = downloadTask.MetaData.TvShowKey,
                                Title = downloadTask.TitleTvShow,
                                FullTitle = downloadTask.TitleTvShow,
                                MediaType = PlexMediaType.TvShow,
                                Children = new List<DownloadTaskDTO>(),
                            };
                            convertedDownloadTasks.Add(tvShowDownloadTask);
                        }

                        var children = new List<DownloadTaskDTO>();
                        convertedDownloadTasks.ForEach(x =>
                        {
                            if (x.Children is not null && x.Children.Any())
                            {
                                children.AddRange(x.Children);
                            }
                        });

                        if (children.Any())
                        {
                            var tvShowSeasonDownloadTask = children
                                .Find(x => x.Key == downloadTask.MetaData.TvShowSeasonKey && x.PlexLibraryId == downloadTask.PlexLibraryId);
                            if (tvShowSeasonDownloadTask is null)
                            {
                                tvShowSeasonDownloadTask = new DownloadTaskDTO
                                {
                                    Key = downloadTask.MetaData.TvShowSeasonKey,
                                    Title = downloadTask.TitleTvShowSeason,
                                    FullTitle = $"{tvShowDownloadTask.Title}/{downloadTask.TitleTvShowSeason}",
                                    MediaType = PlexMediaType.Season,
                                    Children = new List<DownloadTaskDTO>(),
                                };
                                tvShowDownloadTask.Children.Add(tvShowSeasonDownloadTask);
                            }

                            mappedDownloadTask.Key = downloadTask.MetaData.TvShowEpisodeKey;
                            mappedDownloadTask.Title = downloadTask.TitleTvShowEpisode;
                            tvShowSeasonDownloadTask.Children.Add(mappedDownloadTask);
                        }
                    }
                }

                // Ensure all root downloadTasks have the correct summed values
                convertedDownloadTasks.ForEach(downloadTask =>
                {
                    if (downloadTask.MediaType == PlexMediaType.TvShow)
                    {
                        if (downloadTask.Children is not null && !downloadTask.Children.Any())
                        {
                            downloadTask.Children.ForEach(season =>
                            {
                                var children = downloadTask.Children ?? new List<DownloadTaskDTO>();
                                if (children.Any())
                                {
                                    season.DataTotal = season.Children.Sum(x => x.DataTotal);
                                    season.DataReceived = season.Children.Sum(x => x.DataReceived);
                                    season.DataReceived = season.Children.Sum(x => x.DataReceived);
                                    season.Percentage = decimal.Round(season.Children?.Average(x => x.Percentage) ?? 0, 2);
                                }
                            });

                            var children = downloadTask.Children ?? new List<DownloadTaskDTO>();
                            if (children.Any())
                            {
                                downloadTask.DataTotal = children.Sum(x => x.DataTotal);
                                downloadTask.DataReceived = children.Sum(x => x.DataReceived);
                                downloadTask.DataReceived = children.Sum(x => x.DataReceived);
                                downloadTask.Percentage = decimal.Round(children.Average(x => x.Percentage), 2);
                            }
                        }
                    }
                });

                return Result.Ok(convertedDownloadTasks);
            }
            catch (Exception e)
            {
                return Result.Fail(new ExceptionalError(e)).LogError();
            }
        }
    }
}