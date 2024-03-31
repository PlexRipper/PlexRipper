import { randMovie, randDirectoryPath, randUrl, randFileName, randNumber, randUuid } from '@ngneat/falso';
import { times } from 'lodash-es';
import { toPlexMediaType } from '@composables/conversion';
import type { MockConfig } from '@mock';
import { DownloadStatus, type DownloadTaskDTO, DownloadTaskType } from '@dto';
import { checkConfig, incrementSeed } from '@mock/mock-base';

export function generateDownloadTasks({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadTaskDTO[] {
	const validConfig = checkConfig(config);

	const downloadTasks: DownloadTaskDTO[] = [];
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
	partial = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	type: DownloadTaskType;
	config: Partial<MockConfig>;
	partial?: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO {
	checkConfig(config);
	incrementSeed();
	const title = randMovie();
	return {
		id,
		title,
		dataReceived: 0,
		dataTotal: 1000000000, // 1 GB in bytes
		downloadSpeed: 0,
		fileTransferSpeed: 0,
		percentage: 0,
		mediaType: toPlexMediaType(type),
		status: DownloadStatus.Queued,
		timeRemaining: 0,
		actions: ['details'],
		destinationDirectory: randDirectoryPath(),
		downloadDirectory: randDirectoryPath(),
		downloadTaskType: type,
		downloadUrl: randUrl(),
		fileLocationUrl: randUrl(),
		fileName: randFileName(),
		fullTitle: title,
		key: randNumber({ min: 1, max: 1000000 }),
		parentId: randUuid(),
		plexLibraryId: 0,
		plexServerId: 0,
		createdAt: new Date().toISOString(),
		children: [],
		...partial,
	};
}

export function generateDownloadTaskTvShow({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	partial: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO {
	return generateDownloadTask({
		id,
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.TvShow,
		config,
		partial: {
			downloadTaskType: DownloadTaskType.TvShow,
			children: generateDownloadTaskTvShowSeasons({
				plexServerId,
				plexLibraryId,
				config,
				partial: {
					parentId: id,
				},
			}),
			...partial,
		},
	});
}

export function generateDownloadTaskMovie({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	partial: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO {
	return generateDownloadTask({
		id,
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.Movie,
		config,
		partial: {
			downloadTaskType: DownloadTaskType.Movie,
			...partial,
		},
	});
}

export function generateDownloadTaskMovies({
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	partial?: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO[] {
	const validConfig = checkConfig(config);

	return times(validConfig.movieDownloadTask, () =>
		generateDownloadTaskMovie({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
			partial,
		}),
	);
}

export function generateDownloadTaskTvShows({
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	partial?: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.tvShowDownloadTask, () =>
		generateDownloadTaskTvShow({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
			partial,
		}),
	);
}

export function generateDownloadTaskTvShowSeason({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	partial: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO {
	incrementSeed();

	return generateDownloadTask({
		id,
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.Season,
		config,
		partial: {
			children: generateDownloadTaskTvShowEpisodes({ plexServerId, plexLibraryId, config }),
			downloadTaskType: DownloadTaskType.Season,
			...partial,
		},
	});
}

export function generateDownloadTaskTvShowSeasons({
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	partial?: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO[] {
	const validConfig = checkConfig(config);

	const seasonIndex = 1;
	const id = randUuid();
	return times(validConfig.seasonDownloadTask, () =>
		generateDownloadTaskTvShowSeason({
			id,
			plexServerId,
			plexLibraryId,
			config,
			partial: {
				title: `Season ${seasonIndex}`,
				parentId: id,
				...partial,
			},
		}),
	);
}

export function generateDownloadTaskTvShowEpisode({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	partial: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO {
	return generateDownloadTask({ id, plexServerId, plexLibraryId, type: DownloadTaskType.Episode, config, partial });
}

export function generateDownloadTaskTvShowEpisodes({
	plexServerId,
	plexLibraryId,
	config = {},
	partial = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	partial?: Partial<DownloadTaskDTO>;
}): DownloadTaskDTO[] {
	const validConfig = checkConfig(config);
	let episodeIndex = 1;

	return times(validConfig.episodeDownloadTask, () => {
		const episode = generateDownloadTaskTvShowEpisode({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
			partial,
		});
		episode.title = `Episode ${episodeIndex++} - ${episode.title}`;
		return episode;
	});
}
