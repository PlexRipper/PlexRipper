import { randMovie } from '@ngneat/falso';
import { toPlexMediaType } from '@composables/conversion';
import { MockConfig } from '@mock/interfaces';
import { DownloadProgressDTO, DownloadStatus, DownloadTaskType, ServerDownloadProgressDTO } from '@dto/mainApi';
import { checkConfig, getId } from '@mock/mock-base';

export function generateDownloadTasks(plexServerId: number, config: Partial<MockConfig> = {}): ServerDownloadProgressDTO {
	const validConfig = checkConfig(config);

	const downloadTasks: DownloadProgressDTO[] = [];

	downloadTasks.push(...generateDownloadTasksByType(plexServerId, DownloadTaskType.TvShow, config));

	return {
		id: plexServerId,
		downloads: downloadTasks,
	};
}

export function generateDownloadTasksByType(
	plexServerId: number,
	type: DownloadTaskType,
	config: Partial<MockConfig> = {},
): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	const downloadTasks: DownloadProgressDTO[] = [];
	for (let i = 0; i < validConfig.tvShowDownloadTask; i++) {
		const downloadTask = {
			id: getId(),
			dataReceived: 0,
			dataTotal: 0,
			downloadSpeed: 0,
			fileTransferSpeed: 0,
			mediaType: toPlexMediaType(type),
			percentage: 55,
			status: DownloadStatus.Queued,
			timeRemaining: 0,
			title: randMovie(),
			actions: ['details'],
			children: [],
		};

		if (type === DownloadTaskType.TvShow) {
			downloadTask.children = generateDownloadTasksByType(plexServerId, DownloadTaskType.Season, config);
		}

		if (type === DownloadTaskType.Season) {
			downloadTask.children = generateDownloadTasksByType(plexServerId, DownloadTaskType.Episode, config);
			downloadTask.title = 'Season 0';
		}

		downloadTasks.push(downloadTask);
	}

	return downloadTasks;
}
