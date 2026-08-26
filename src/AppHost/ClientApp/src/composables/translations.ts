import { PlexMediaType, type CreateDownloadTasksRequest, type DownloadTaskCreationReportDTO } from '@dto';
import type { ResultDTO } from '@interfaces';

function translateDownloadMediaCount(mediaType: PlexMediaType, count: number): string {
	const { $i18n } = useNuxtApp();
	const { t } = $i18n;

	switch (mediaType) {
		case PlexMediaType.Movie:
			return t('general.download-notification.movie', { count }, { plural: count });
		case PlexMediaType.TvShow:
			return t('general.download-notification.tv-show', { count }, { plural: count });
		case PlexMediaType.Season:
			return t('general.download-notification.season', { count }, { plural: count });
		case PlexMediaType.Episode:
			return t('general.download-notification.episode', { count }, { plural: count });
		default:
			return t('general.download-notification.unknown');
	}
}

export function translateDownloadNotification(
	result: ResultDTO<DownloadTaskCreationReportDTO>,
	request: CreateDownloadTasksRequest,
): string | null {
	if (!result.isSuccess || !result.value) {
		return null;
	}

	const { $i18n } = useNuxtApp();
	const { t } = $i18n;
	const { movies, tvShows, seasons, episodes } = result.value;
	const singleMediaType = request.downloadMedias.length === 1 && request.downloadMedias[0]?.mediaIds.length === 1
		? request.downloadMedias[0].type
		: null;
	let message = t('general.download-notification.unknown');

	if (movies > 0 && tvShows > 0) {
		const variant = (movies > 1 ? 8 : 0) | (tvShows > 1 ? 4 : 0) | (seasons > 1 ? 2 : 0) | (episodes > 1 ? 1 : 0);
		switch (variant) {
			case 0:
				message = t('general.download-notification.movie-and-tv-show-v0', { movies, shows: tvShows, seasons, episodes });
				break;
			case 1:
				message = t('general.download-notification.movie-and-tv-show-v1', { movies, shows: tvShows, seasons, episodes });
				break;
			case 2:
				message = t('general.download-notification.movie-and-tv-show-v2', { movies, shows: tvShows, seasons, episodes });
				break;
			case 3:
				message = t('general.download-notification.movie-and-tv-show-v3', { movies, shows: tvShows, seasons, episodes });
				break;
			case 4:
				message = t('general.download-notification.movie-and-tv-show-v4', { movies, shows: tvShows, seasons, episodes });
				break;
			case 5:
				message = t('general.download-notification.movie-and-tv-show-v5', { movies, shows: tvShows, seasons, episodes });
				break;
			case 6:
				message = t('general.download-notification.movie-and-tv-show-v6', { movies, shows: tvShows, seasons, episodes });
				break;
			case 7:
				message = t('general.download-notification.movie-and-tv-show-v7', { movies, shows: tvShows, seasons, episodes });
				break;
			case 8:
				message = t('general.download-notification.movie-and-tv-show-v8', { movies, shows: tvShows, seasons, episodes });
				break;
			case 9:
				message = t('general.download-notification.movie-and-tv-show-v9', { movies, shows: tvShows, seasons, episodes });
				break;
			case 10:
				message = t('general.download-notification.movie-and-tv-show-v10', { movies, shows: tvShows, seasons, episodes });
				break;
			case 11:
				message = t('general.download-notification.movie-and-tv-show-v11', { movies, shows: tvShows, seasons, episodes });
				break;
			case 12:
				message = t('general.download-notification.movie-and-tv-show-v12', { movies, shows: tvShows, seasons, episodes });
				break;
			case 13:
				message = t('general.download-notification.movie-and-tv-show-v13', { movies, shows: tvShows, seasons, episodes });
				break;
			case 14:
				message = t('general.download-notification.movie-and-tv-show-v14', { movies, shows: tvShows, seasons, episodes });
				break;
			case 15:
				message = t('general.download-notification.movie-and-tv-show-v15', { movies, shows: tvShows, seasons, episodes });
				break;
		}
	} else if (movies > 0) {
		message = translateDownloadMediaCount(PlexMediaType.Movie, movies);
	} else if (singleMediaType === PlexMediaType.TvShow && tvShows > 0) {
		message = translateDownloadMediaCount(singleMediaType, tvShows);
	} else if (singleMediaType === PlexMediaType.Season && seasons > 0) {
		message = translateDownloadMediaCount(singleMediaType, seasons);
	} else if (singleMediaType === PlexMediaType.Episode && episodes > 0) {
		message = translateDownloadMediaCount(singleMediaType, episodes);
	} else if (tvShows > 0 || seasons > 0 || episodes > 0) {
		const variant = (tvShows > 1 ? 4 : 0) | (seasons > 1 ? 2 : 0) | (episodes > 1 ? 1 : 0);
		switch (variant) {
			case 0:
				message = t('general.download-notification.tv-show-detail-v0', { shows: tvShows, seasons, episodes });
				break;
			case 1:
				message = t('general.download-notification.tv-show-detail-v1', { shows: tvShows, seasons, episodes });
				break;
			case 2:
				message = t('general.download-notification.tv-show-detail-v2', { shows: tvShows, seasons, episodes });
				break;
			case 3:
				message = t('general.download-notification.tv-show-detail-v3', { shows: tvShows, seasons, episodes });
				break;
			case 4:
				message = t('general.download-notification.tv-show-detail-v4', { shows: tvShows, seasons, episodes });
				break;
			case 5:
				message = t('general.download-notification.tv-show-detail-v5', { shows: tvShows, seasons, episodes });
				break;
			case 6:
				message = t('general.download-notification.tv-show-detail-v6', { shows: tvShows, seasons, episodes });
				break;
			case 7:
				message = t('general.download-notification.tv-show-detail-v7', { shows: tvShows, seasons, episodes });
				break;
		}
	}

	return message;
}
