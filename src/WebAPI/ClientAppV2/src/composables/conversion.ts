import Log from 'consola';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import IDownloadPreview from '@interfaces/components/IDownloadPreview';

export function toDownloadMedia(mediaItem: PlexMediaDTO): DownloadMediaDTO[] {
	return [
		{
			mediaIds: [mediaItem.id],
			type: mediaItem.type,
			plexServerId: mediaItem.plexServerId,
			plexLibraryId: mediaItem.plexLibraryId,
		},
	];
}

export function toDownloadPreview(downloadList: DownloadMediaDTO[], mediaItems: PlexMediaDTO[]): IDownloadPreview[] {
	let downloadPreview: PlexMediaDTO[] = [];

	const movieDownloadCommand = downloadList.find((x) => x.type === PlexMediaType.Movie);
	// If statements instead of switch to avoid having to overcomplicate the variable names.
	// Movie: Show only the movie
	if (movieDownloadCommand) {
		downloadPreview = downloadPreview.concat(mediaItems.filter((movie) => movieDownloadCommand.mediaIds.includes(movie.id)));
	}

	// TvShow: Show tvShow -> with all season -> with all episodes
	const tvShowDownloadCommand = downloadList.find((x) => x.type === PlexMediaType.TvShow);
	if (tvShowDownloadCommand) {
		downloadPreview = downloadPreview.concat(
			mediaItems.filter((tvShow) => tvShowDownloadCommand.mediaIds.includes(tvShow.id)),
		);
	}

	// Season: Show tvShow -> season -> with all episodes
	const tvShowSeasonDownloadCommand = downloadList.find((x) => x.type === PlexMediaType.Season);
	if (tvShowSeasonDownloadCommand) {
		const mediaIds = tvShowSeasonDownloadCommand.mediaIds;

		downloadPreview = downloadPreview.concat(
			mediaItems
				.filter((tvShow) => tvShow.children?.some((season) => mediaIds.includes(season.id)))
				.map((tvShow) => {
					return {
						...tvShow,
						children: tvShow.children?.filter((season) => mediaIds.includes(season.id)),
					};
				}),
		);
	}

	// Episode: Show tvShow -> season -> episode without anything else
	const tvShowEpisodeDownloadCommand = downloadList.find((x) => x.type === PlexMediaType.Episode);
	if (tvShowEpisodeDownloadCommand) {
		const mediaIds = tvShowEpisodeDownloadCommand.mediaIds;
		const filterResult = mediaItems
			.filter((tvShow) =>
				tvShow.children?.some((season) => season.children?.some((episode) => mediaIds.includes(episode.id))),
			)
			.map((tvShow) => {
				// Create the tvShow
				return {
					...tvShow,
					children: tvShow.children
						?.filter((season: PlexMediaDTO) => season?.children?.some((episode) => mediaIds.includes(episode.id)))
						.map((season: PlexMediaDTO) => {
							// Create the tvShowSeason
							return {
								...season,
								children: season?.children?.filter((episode) => mediaIds.includes(episode.id)),
							};
						}),
				};
			});

		// Merge the tvShows
		filterResult.forEach((filterResultTvShow) => {
			const downloadPreviewTvShow = downloadPreview.find((x) => x.id === filterResultTvShow.id);
			if (downloadPreviewTvShow) {
				// There already is a tvShow in the filterResult with the same id
				filterResultTvShow.children?.forEach((season) => {
					const filterResultTvShowSeason = downloadPreviewTvShow?.children?.find((x) => x.id === season.id);
					if (!filterResultTvShowSeason) {
						downloadPreviewTvShow?.children?.push(season);
					}
				});
			} else {
				downloadPreview.push(filterResultTvShow);
			}
		});
	}

	// Calculate mediaSize for each parent and child (TvShow and Season);
	downloadPreview.forEach((parent) => {
		if (parent.type === PlexMediaType.TvShow || parent.type === PlexMediaType.Season) {
			parent.children?.forEach((child) => {
				child.mediaSize = child?.children?.map((x) => x.mediaSize).sum() ?? 0;
			});
			parent.mediaSize = parent.children?.map((x) => x.mediaSize).sum() ?? 0;
		}
	});

	function convertToPreview(media: PlexMediaDTO[]): IDownloadPreview[] {
		return media.map((x) => {
			return {
				id: x.id,
				title: x.title,
				type: x.type,
				size: x.mediaSize,
				children: x.children ? convertToPreview(x.children) : [],
			};
		});
	}

	const result = convertToPreview(downloadPreview);
	Log.info('downloadPreview', result);
	return result;
}
