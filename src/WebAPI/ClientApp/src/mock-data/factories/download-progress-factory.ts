import { randMovie, randUuid } from '@ngneat/falso';
import { times } from 'lodash-es';
import type { MockConfig } from '@mock';
import { DownloadStatus, DownloadTaskType, type DownloadProgressDTO, type ServerDownloadProgressDTO } from '@dto';
import { checkConfig, incrementSeed } from '@mock/mock-base';
import Convert from '@class/Convert';

export function generateServerDownloadProgress({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): ServerDownloadProgressDTO {
	const downloadTasks: DownloadProgressDTO[] = generateDownloadProgress({ plexServerId, plexLibraryId, config });

	return {
		id: plexServerId,
		downloads: downloadTasks,
		downloadableTasksCount: downloadTasks.length,
	};
}

export function generateDownloadProgress({
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
		downloadTasks.push(...generateDownloadProgressMovies({ plexServerId, plexLibraryId, config }));
	}

	if (validConfig.tvShowDownloadTask > 0) {
		downloadTasks.push(...generateDownloadProgressTvShows({ plexServerId, plexLibraryId, config }));
	}

	return downloadTasks;
}

export function generateDownloadProgressBase({
	id,
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	plexServerId,
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	plexLibraryId,
	type,
	config = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	type: DownloadTaskType;
	config: Partial<MockConfig>;
}): DownloadProgressDTO {
	checkConfig(config);
	incrementSeed();

	return {
		id,
		dataReceived: 0,
		dataTotal: 1000000000, // 1 GB in bytes
		downloadSpeed: 0,
		fileTransferSpeed: 0,
		mediaType: Convert.toPlexMediaType(type),
		percentage: 0,
		status: DownloadStatus.Queued,
		timeRemaining: 0,
		title: randMovie(),
		actions: ['details'],
		children: [],
	};
}

export function generateDownloadProgressTvShow({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO {
	return {
		...generateDownloadProgressBase({
			id,
			plexServerId,
			plexLibraryId,
			type: DownloadTaskType.TvShow,
			config,
		}),
		children: generateDownloadProgressTvShowSeasons({ plexServerId, plexLibraryId, config }),
	};
}

export function generateDownloadProgressMovie({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO {
	return generateDownloadProgressBase({
		id,
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.Movie,
		config,
	});
}

export function generateDownloadProgressMovies({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	return times(validConfig.movieDownloadTask, () =>
		generateDownloadProgressMovie({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
		}),
	);
}

export function generateDownloadProgressTvShows({
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.tvShowDownloadTask, () =>
		generateDownloadProgressTvShow({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
		}),
	);
}

export function generateDownloadProgressTvShowSeason({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO {
	incrementSeed();

	return {
		...generateDownloadProgressBase({
			id,
			plexServerId,
			plexLibraryId,
			type: DownloadTaskType.Season,
			config,
		}),
		children: generateDownloadProgressTvShowEpisodes({ plexServerId, plexLibraryId, config }),
	};
}

export function generateDownloadProgressTvShowSeasons({
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
	return times(validConfig.seasonDownloadTask, () => {
		const season = generateDownloadProgressTvShowSeason({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
		});
		season.title = `Season ${seasonIndex++}`;
		return season;
	});
}

export function generateDownloadProgressTvShowEpisode({
	id,
	plexServerId,
	plexLibraryId,
	config = {},
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO {
	return generateDownloadProgressBase({ id, plexServerId, plexLibraryId, type: DownloadTaskType.Episode, config });
}

export function generateDownloadProgressTvShowEpisodes({
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

	return times(validConfig.episodeDownloadTask, () => {
		const episode = generateDownloadProgressTvShowEpisode({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
		});
		episode.title = `Episode ${episodeIndex++} - ${episode.title}`;
		return episode;
	});
}
