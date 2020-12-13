import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import { PlexMediaType, PlexMovieDTO, PlexTvShowDTO } from '@dto/mainApi';
import IMediaData from '@mediaOverview/MediaTable/types/IMediaData';

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
								key: `${tvShow.id}-${season.id}-${episode.id}`,
								title: episode.title ?? '',
								type: PlexMediaType.Episode,
								children: [],
								mediaData: [] as IMediaData[],
								item: episode,
								addedAt: episode.addedAt ?? '',
								updatedAt: episode.updatedAt ?? '',
							});
						});
						// Add seasons
						seasons.push({
							id: season.id,
							key: `${tvShow.id}-${season.id}`,
							title: season.title ?? '',
							type: PlexMediaType.Season,
							children: episodes,
							mediaData: [] as IMediaData[],
							item: season,
							addedAt: season.addedAt ?? '',
							updatedAt: season.updatedAt ?? '',
						});
					}
				});
				// Add tvShow
				items.push({
					id: tvShow.id,
					key: `${tvShow.id}`,
					title: tvShow.title ?? '',
					year: tvShow.year,
					type: PlexMediaType.TvShow,
					item: tvShow,
					mediaData: [] as IMediaData[],
					children: seasons,
					addedAt: tvShow.addedAt ?? '',
					updatedAt: tvShow.updatedAt ?? '',
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
					size: movie.size,
					type: PlexMediaType.Movie,
					item: movie,
					mediaData: movie.plexMovieDatas as IMediaData[],
					children: [],
					addedAt: movie.addedAt ?? '',
					updatedAt: movie.updatedAt ?? '',
				});
			}
		});
		return items;
	}
}
