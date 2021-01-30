import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import { PlexMediaType, PlexMovieDTO, PlexTvShowDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';

export default abstract class Convert {
	public static tvShowsToTreeViewItems(tvShows: PlexTvShowDTO[]): ITreeViewItem[] {
		const items: ITreeViewItem[] = [];

		tvShows.forEach((tvShow: PlexTvShowDTO) => {
			const seasons: ITreeViewItem[] = [];
			if (tvShow.seasons) {
				tvShow.seasons.forEach((season) => {
					const episodes: ITreeViewItem[] = [];
					if (season.episodes) {
						season.episodes.forEach((episode) => {
							// Add Episode
							episodes.push({
								id: episode.id,
								duration: episode.duration,
								hasArt: episode.hasArt,
								hasBanner: episode.hasBanner,
								hasTheme: episode.hasTheme,
								hasThumb: episode.hasThumb,
								plexLibraryId: episode.plexLibraryId,
								plexServerId: episode.plexServerId,
								year: episode.year,
								key: `${tvShow.id}-${season.id}-${episode.id}`,
								title: episode.title ?? '',
								type: PlexMediaType.Episode,
								mediaSize: episode.mediaSize,
								addedAt: episode.addedAt ?? '',
								updatedAt: episode.updatedAt ?? '',
								childCount: 0,
							});
						});
						// Add seasons
						seasons.push({
							duration: season.duration,
							hasArt: season.hasArt,
							hasBanner: season.hasBanner,
							hasTheme: season.hasTheme,
							hasThumb: season.hasThumb,
							plexLibraryId: season.plexLibraryId,
							plexServerId: season.plexServerId,
							year: season.year,
							id: season.id,
							key: `${tvShow.id}-${season.id}`,
							title: season.title,
							type: PlexMediaType.Season,
							children: episodes,
							mediaSize: season.mediaSize,
							addedAt: season.addedAt,
							updatedAt: season.updatedAt,
							childCount: season.childCount,
						});
					}
				});
				// Add tvShow
				items.push({
					duration: tvShow.duration,
					hasArt: tvShow.hasArt,
					hasBanner: tvShow.hasBanner,
					hasTheme: tvShow.hasTheme,
					hasThumb: tvShow.hasThumb,
					plexLibraryId: tvShow.plexLibraryId,
					plexServerId: tvShow.plexServerId,
					id: tvShow.id,
					key: `${tvShow.id}`,
					title: tvShow.title ?? '',
					year: tvShow.year,
					type: PlexMediaType.TvShow,
					mediaSize: tvShow.mediaSize,
					children: seasons,
					addedAt: tvShow.addedAt ?? '',
					updatedAt: tvShow.updatedAt ?? '',
					childCount: tvShow.childCount,
				});
			}
		});

		return items;
	}

	public static moviesToTreeViewItems(movies: PlexMovieDTO[]): ITreeViewItem[] {
		const items: ITreeViewItem[] = [];

		movies.forEach((movie: PlexMovieDTO) => {
			if (movie) {
				// Add movie
				items.push({
					id: movie.id,
					key: `${movie.id}`,
					title: movie.title ?? '',
					year: movie.year,
					mediaSize: movie.mediaSize,
					type: PlexMediaType.Movie,
					addedAt: movie.addedAt ?? '',
					updatedAt: movie.updatedAt ?? '',
					duration: movie.duration,
					hasArt: movie.hasArt,
					hasBanner: movie.hasBanner,
					hasTheme: movie.hasTheme,
					hasThumb: movie.hasThumb,
					plexLibraryId: movie.plexLibraryId,
					plexServerId: movie.plexServerId,
					childCount: 0,
				});
			}
		});
		return items;
	}

	public static buttonTypeToIcon(type: ButtonType): string {
		switch (type) {
			case ButtonType.Cancel:
				return 'mdi-cancel';
			case ButtonType.Confirm:
				return 'mdi-check';
			case ButtonType.Save:
				return 'mdi-content-save';
			case ButtonType.Download:
				return 'mdi-download';
			case ButtonType.Delete:
				return 'mdi-delete';
			case ButtonType.Error:
				return 'mdi-alert-circle';
			case ButtonType.ExternalLink:
				return 'mdi-open-in-new';
			case ButtonType.Start:
			case ButtonType.Resume:
				return 'mdi-play';
			case ButtonType.Restart:
				return 'mdi-refresh';
			case ButtonType.Pause:
				return 'mdi-pause';
			case ButtonType.Stop:
				return 'mdi-stop';
			case ButtonType.Clear:
				return 'mdi-notification-clear-all';
			case ButtonType.Details:
				return 'mdi-chart-box-outline';
			default:
				return 'mdi-help-circle-outline';
		}
	}

	public static mediaTypeToIcon(mediaType: PlexMediaType): string {
		switch (mediaType) {
			case PlexMediaType.TvShow:
				return 'mdi-television-classic';
			case PlexMediaType.Season:
				return 'mdi-play-box-multiple';
			case PlexMediaType.Episode:
				return 'mdi-movie-open';
			case PlexMediaType.Movie:
				return 'mdi-filmstrip';
			case PlexMediaType.Music:
				return 'mdi-music';
			default:
				return 'mdi-help-circle-outline';
		}
	}
}
