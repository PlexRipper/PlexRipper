import { DownloadActions, type DownloadMediaDTO, DownloadStatus, type PlexMediaDTO, type PlexMediaSlimDTO } from '@dto';

export function toDownloadMedia(mediaItem: PlexMediaDTO | PlexMediaSlimDTO): DownloadMediaDTO[] {
	return [
		{
			mediaIds: [mediaItem.id],
			type: mediaItem.type,
			plexServerId: mediaItem.plexServerId,
			plexLibraryId: mediaItem.plexLibraryId,
			qualities: mediaItem.qualities,
		},
	];
}

export function toDownloadActions(downloadStatus: DownloadStatus): DownloadActions[] {
	const actions: DownloadActions[] = [DownloadActions.Details];

	// NOTE: When updating this, also update back-end: src/Domain/Common/Converters/DownloadTaskActions.cs
	switch (downloadStatus) {
		case DownloadStatus.Unknown:
			actions.push(DownloadActions.Delete);
			break;
		case DownloadStatus.Queued:
			actions.push(DownloadActions.Start, DownloadActions.Delete);
			break;
		case DownloadStatus.Downloading:
			actions.push(DownloadActions.Pause, DownloadActions.Stop);
			break;
		case DownloadStatus.DownloadFinished:
		case DownloadStatus.MoveFinished:
			actions.push(DownloadActions.Delete);
			break;
		case DownloadStatus.Paused:
		case DownloadStatus.MovePaused:
			actions.push(DownloadActions.Start, DownloadActions.Stop, DownloadActions.Delete);
			break;
		case DownloadStatus.Completed:
			actions.push(DownloadActions.Clear, DownloadActions.Restart);
			break;
		case DownloadStatus.Stopped:
			actions.push(DownloadActions.Restart, DownloadActions.Delete);
			break;
		case DownloadStatus.Moving:
		case DownloadStatus.Merging:
			actions.push(DownloadActions.Pause, DownloadActions.Stop);
			break;
		case DownloadStatus.Error:
		case DownloadStatus.MoveError:
			actions.push(DownloadActions.Restart, DownloadActions.Delete);
			break;
		case DownloadStatus.ServerUnreachable:
			actions.push(DownloadActions.Start, DownloadActions.Stop, DownloadActions.Delete);
			break;
		default:
			console.error(`Unknown download status: ${downloadStatus}`);
			break;
	}

	return actions;
}
