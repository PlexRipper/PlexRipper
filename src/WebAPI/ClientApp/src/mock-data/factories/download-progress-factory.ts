import { randMovie, randUuid } from '@ngneat/falso';
import { times } from 'lodash-es';
import type { Seed, MockConfig } from '@mock';
import { DownloadStatus, DownloadTaskType, type DownloadProgressDTO, type ServerDownloadProgressDTO } from '@dto';
import { checkConfig, incrementSeed } from '@mock/mock-base';
import Convert from '@class/Convert';
import { toDownloadActions } from '@composables';

export function generateServerDownloadProgress({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	seed: Seed;
}): ServerDownloadProgressDTO {
	const downloadTasks: DownloadProgressDTO[] = generateDownloadProgress({ plexServerId, plexLibraryId, config, seed });

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
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	const downloadTasks: DownloadProgressDTO[] = [];
	if (validConfig.movieDownloadTask > 0) {
		downloadTasks.push(...generateDownloadProgressMovies({ plexServerId, plexLibraryId, config, seed }));
	}

	if (validConfig.tvShowDownloadTask > 0) {
		downloadTasks.push(...generateDownloadProgressTvShows({ plexServerId, plexLibraryId, config, seed }));
	}

	return downloadTasks;
}

export function generateDownloadProgressBase({
	type,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	type: DownloadTaskType;
	config: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO {
	checkConfig(config);

	seed.next();

	return {
		id: randUuid(),
		dataReceived: 0,
		dataTotal: 1000000000, // 1 GB in bytes
		downloadSpeed: 0,
		mediaType: Convert.toPlexMediaType(type),
		percentage: 0,
		status: DownloadStatus.Queued,
		timeRemaining: 0,
		title: randMovie(),
		children: [],
	};
}

export function generateDownloadProgressTvShow({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO {
	return {
		...generateDownloadProgressBase({
			plexServerId,
			plexLibraryId,
			type: DownloadTaskType.TvShow,
			config,
			seed,
		}),
		children: generateDownloadProgressTvShowSeasons({ plexServerId, plexLibraryId, config, seed }),
	};
}

export function generateDownloadProgressMovie({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO {
	return generateDownloadProgressBase({
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.Movie,
		config,
		seed,
	});
}

export function generateDownloadProgressMovies({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	return times(validConfig.movieDownloadTask, () =>
		generateDownloadProgressMovie({
			plexServerId,
			plexLibraryId,
			config,
			seed,
		}),
	);
}

export function generateDownloadProgressTvShows({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);
	return times(validConfig.tvShowDownloadTask, () =>
		generateDownloadProgressTvShow({
			plexServerId,
			plexLibraryId,
			config,
			seed,
		}),
	);
}

export function generateDownloadProgressTvShowSeason({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	id: string;
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO {
	incrementSeed();

	return {
		...generateDownloadProgressBase({
			plexServerId,
			plexLibraryId,
			type: DownloadTaskType.Season,
			config,
			seed,
		}),
		children: generateDownloadProgressTvShowEpisodes({ plexServerId, plexLibraryId, config, seed }),
	};
}

export function generateDownloadProgressTvShowSeasons({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);

	let seasonIndex = 1;
	return times(validConfig.seasonDownloadTask, () => {
		const season = generateDownloadProgressTvShowSeason({
			id: randUuid(),
			plexServerId,
			plexLibraryId,
			config,
			seed,
		});
		season.title = `Season ${seasonIndex++}`;
		return season;
	});
}

export function generateDownloadProgressTvShowEpisode({
	plexServerId,
	plexLibraryId,
	config = {},
	seed,
}: {
	plexServerId: number;
	plexLibraryId: number;
	config?: Partial<MockConfig>;
	seed: Seed;
}): DownloadProgressDTO {
	return generateDownloadProgressBase({
		plexServerId,
		plexLibraryId,
		type: DownloadTaskType.Episode,
		config,
		seed,
	});
}

export function generateDownloadProgressTvShowEpisodes({
	plexServerId,
	plexLibraryId,
	seed,
	config = {},
}: {
	plexServerId: number;
	plexLibraryId: number;
	seed: Seed;
	config?: Partial<MockConfig>;
}): DownloadProgressDTO[] {
	const validConfig = checkConfig(config);
	let episodeIndex = 1;

	return times(validConfig.episodeDownloadTask, () => {
		const episode = generateDownloadProgressTvShowEpisode({
			plexServerId,
			plexLibraryId,
			config,
			seed,
		});
		episode.title = `Episode ${episodeIndex++} - ${episode.title}`;
		return episode;
	});
}
