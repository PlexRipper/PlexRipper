import { randMovie } from '@ngneat/falso';
import { toPlexMediaType } from '@composables/conversion';
import { MockConfig } from '@mock/interfaces';
import { DownloadProgressDTO, DownloadStatus, DownloadTaskType, ServerDownloadProgressDTO } from '@dto/mainApi';
import { checkConfig } from '@mock/mock-base';

let downloadTaskIdIndex = 1;

export function generateServerDownloadTasks({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): ServerDownloadProgressDTO {
	const downloadTasks: DownloadProgressDTO[] = generateDownloadTasks({ plexServerId, plexLibraryId, config });

	return {
		id: plexServerId,
		downloads: downloadTasks,
		downloadableTasksCount: downloadTasks.length,
	};
}

export function generateDownloadTasks({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	const downloadTasks: DownloadProgressDTO[] = [];
	if (validConfig.movieDownloadTask > 0) {
		downloadTasks.push(...generateDownloadTaskMovies({ plexServerId, plexLibraryId, config }));
	}

	if (validConfig.tvShowDownloadTask > 0) {
		downloadTasks.push(...generateDownloadTaskTvShows({ plexServerId, plexLibraryId, config }));
	}

	return downloadTasks;
}

export function generateDownloadTask({
	id,
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	plexServerId,
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	plexLibraryId,
	type,
	config = {},
}: {
	id: number;
	plexServerId: number;
	plexLibraryId: number;
	type: DownloadTaskType;
	config: Partial<MockConfig>;
}): DownloadProgressDTO {
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	const validConfig = checkConfig(config);

	return {
		id,
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
}

export function generateDownloadTaskTvShow({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: number;
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO {
	return {
		...generateDownloadTask({
			id,
			plexServerId,
			plexLibraryId,
			type: DownloadTaskType.TvShow,
			config,
		}),
		children: generateDownloadTaskTvShowSeasons({ plexServerId, plexLibraryId, config }),
	};
}

export function generateDownloadTaskMovie({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: number;
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO {
	return generateDownloadTask({
		id,
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.Movie,
		config,
	});
}

export function generateDownloadTaskMovies({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	return Array(validConfig.movieDownloadTask)
		.fill(null)
		.map(() =>
			generateDownloadTaskMovie({
				id: downloadTaskIdIndex++,
				plexServerId,
				plexLibraryId,
				config,
			}),
		);
}

export function generateDownloadTaskTvShows({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	return Array(validConfig.tvShowDownloadTask)
		.fill(null)
		.map(() =>
			generateDownloadTaskTvShow({
				id: downloadTaskIdIndex++,
				plexServerId,
				plexLibraryId,
				config,
			}),
		);
}

export function generateDownloadTaskTvShowSeason({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: number;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO {
	return {
		...generateDownloadTask({
			id,
			plexServerId,
			plexLibraryId,
			type: DownloadTaskType.Season,
			config,
		}),
		children: generateDownloadTaskTvShowEpisodes({ plexServerId, plexLibraryId, config }),
	};
}

export function generateDownloadTaskTvShowSeasons({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	let seasonIndex = 1;
	return Array(validConfig.seasonDownloadTask)
		.fill(null)
		.map(() => {
			const season = generateDownloadTaskTvShowSeason({
				id: downloadTaskIdIndex++,
				plexServerId,
				plexLibraryId,
				config,
			});
			season.title = `Season ${seasonIndex++}`;
			return season;
		});
}

export function generateDownloadTaskTvShowEpisode({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: number;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO {
	return generateDownloadTask({ id, plexServerId, plexLibraryId, type: DownloadTaskType.Episode, config });
}

export function generateDownloadTaskTvShowEpisodes({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);
	let episodeIndex = 1;

	return Array(validConfig.episodeDownloadTask)
		.fill(null)
		.map(() => {
			const episode = generateDownloadTaskTvShowEpisode({
				id: downloadTaskIdIndex++,
				plexServerId,
				plexLibraryId,
				config,
			});
			episode.title = `Episode ${episodeIndex++} - ${episode.title}`;
			return episode;
		});
}
