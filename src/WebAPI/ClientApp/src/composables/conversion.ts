import { kebabCase } from 'lodash-es';
import {
	DownloadTaskType,
	FolderType,
	PlexMediaType,
	type DownloadMediaDTO,
	type PlexMediaDTO,
	type PlexMediaSlimDTO,
} from '@dto/mainApi';

export function toDownloadMedia(mediaItem: PlexMediaDTO | PlexMediaSlimDTO): DownloadMediaDTO[] {
	return [
		{
			mediaIds: [mediaItem.id],
			type: mediaItem.type,
			plexServerId: mediaItem.plexServerId,
			plexLibraryId: mediaItem.plexLibraryId,
		},
	];
}

export function toFolderPathStringId(folderType: FolderType) {
	return kebabCase(folderType.toString().replace('Folder', ''));
}

export function toPlexMediaType(downloadType: DownloadTaskType) {
	switch (downloadType) {
		case DownloadTaskType.Movie:
			return PlexMediaType.Movie;
		case DownloadTaskType.TvShow:
			return PlexMediaType.TvShow;
		case DownloadTaskType.Season:
			return PlexMediaType.Season;
		case DownloadTaskType.Episode:
			return PlexMediaType.Episode;
		default:
			return PlexMediaType.Unknown;
	}
}

export function toDownloadTaskType(mediaType: PlexMediaType) {
	switch (mediaType) {
		case PlexMediaType.Movie:
			return DownloadTaskType.Movie;
		case PlexMediaType.TvShow:
			return DownloadTaskType.TvShow;
		case PlexMediaType.Season:
			return DownloadTaskType.Season;
		case PlexMediaType.Episode:
			return DownloadTaskType.Episode;
		default:
			return DownloadTaskType.None;
	}
}
