import { DownloadMediaDTO, PlexMediaDTO, PlexMediaSlimDTO } from '@dto/mainApi';

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
