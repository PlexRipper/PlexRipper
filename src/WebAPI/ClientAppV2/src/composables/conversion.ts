import { kebabCase } from 'lodash-es/string';
import {
	DownloadMediaDTO,
	DownloadTaskType,
	FolderPathDTO,
	FolderType,
	PlexMediaDTO,
	PlexMediaSlimDTO,
	PlexMediaType,
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
		// case DownloadTaskType.Music:
		// 	return PlexMediaType.Music;
		default:
			return PlexMediaType.Unknown;
	}
}
