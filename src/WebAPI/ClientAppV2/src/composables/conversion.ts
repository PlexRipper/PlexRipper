import { kebabCase } from 'lodash-es/string';
import { DownloadMediaDTO, FolderPathDTO, FolderType, PlexMediaDTO, PlexMediaSlimDTO } from '@dto/mainApi';

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
