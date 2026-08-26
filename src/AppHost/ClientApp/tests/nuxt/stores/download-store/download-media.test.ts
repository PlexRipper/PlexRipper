import { describe, beforeAll, test, expect } from 'vitest';
import { baseSetup } from '@services-test-base';
import { PlexMediaType, type CreateDownloadTasksRequest, type DownloadTaskCreationReportDTO } from '@dto';
import { generateResultDTO } from '@mock';
import { translateDownloadNotification } from '@composables';

const request = (type: PlexMediaType): CreateDownloadTasksRequest => ({
	customDestinationFolderPath: '',
	downloadMedias: [{
		keepCompletedInDownloadFolder: false,
		mediaIds: [1],
		plexLibraryId: 1,
		plexServerId: 1,
		qualities: [],
		type,
	}],
});

describe('translateDownloadNotification()', () => {
	beforeAll(() => {
		baseSetup();
	});

	test.each([
		[PlexMediaType.TvShow, { movies: 0, tvShows: 1, seasons: 1, episodes: 1 }, 'Downloading 1 TV show'],
		[PlexMediaType.Season, { movies: 0, tvShows: 0, seasons: 1, episodes: 1 }, 'Downloading 1 season'],
		[PlexMediaType.Episode, { movies: 0, tvShows: 0, seasons: 0, episodes: 1 }, 'Downloading 1 episode'],
	])('Should translate a single %s download', (type, counts, message) => {
		const report = generateResultDTO(counts as DownloadTaskCreationReportDTO);

		expect(translateDownloadNotification(report, request(type))).toBe(message);
	});

	test.each([
		[1, 1, 1, 1, 'Downloading 1 movie and 1 TV show with 1 season and 1 episode'],
		[1, 1, 1, 2, 'Downloading 1 movie and 1 TV show with 1 season and 2 episodes'],
		[1, 1, 2, 1, 'Downloading 1 movie and 1 TV show with 2 seasons and 1 episode'],
		[1, 1, 2, 2, 'Downloading 1 movie and 1 TV show with 2 seasons and 2 episodes'],
		[1, 2, 1, 1, 'Downloading 1 movie and 2 TV shows with 1 season and 1 episode'],
		[1, 2, 1, 2, 'Downloading 1 movie and 2 TV shows with 1 season and 2 episodes'],
		[1, 2, 2, 1, 'Downloading 1 movie and 2 TV shows with 2 seasons and 1 episode'],
		[1, 2, 2, 2, 'Downloading 1 movie and 2 TV shows with 2 seasons and 2 episodes'],
		[2, 1, 1, 1, 'Downloading 2 movies and 1 TV show with 1 season and 1 episode'],
		[2, 1, 1, 2, 'Downloading 2 movies and 1 TV show with 1 season and 2 episodes'],
		[2, 1, 2, 1, 'Downloading 2 movies and 1 TV show with 2 seasons and 1 episode'],
		[2, 1, 2, 2, 'Downloading 2 movies and 1 TV show with 2 seasons and 2 episodes'],
		[2, 2, 1, 1, 'Downloading 2 movies and 2 TV shows with 1 season and 1 episode'],
		[2, 2, 1, 2, 'Downloading 2 movies and 2 TV shows with 1 season and 2 episodes'],
		[2, 2, 2, 1, 'Downloading 2 movies and 2 TV shows with 2 seasons and 1 episode'],
		[2, 2, 2, 2, 'Downloading 2 movies and 2 TV shows with 2 seasons and 2 episodes'],
	])('Should preserve mixed download variant %s', (movies, tvShows, seasons, episodes, message) => {
		const report = generateResultDTO({ movies, tvShows, seasons, episodes } as DownloadTaskCreationReportDTO);

		expect(translateDownloadNotification(report, request(PlexMediaType.Movie))).toBe(message);
	});

	test.each([
		[1, 1, 1, 'Downloading 1 TV show with 1 season and 1 episode'],
		[1, 1, 2, 'Downloading 1 TV show with 1 season and 2 episodes'],
		[1, 2, 1, 'Downloading 1 TV show with 2 seasons and 1 episode'],
		[1, 2, 2, 'Downloading 1 TV show with 2 seasons and 2 episodes'],
		[2, 1, 1, 'Downloading 2 TV shows with 1 season and 1 episode'],
		[2, 1, 2, 'Downloading 2 TV shows with 1 season and 2 episodes'],
		[2, 2, 1, 'Downloading 2 TV shows with 2 seasons and 1 episode'],
		[2, 2, 2, 'Downloading 2 TV shows with 2 seasons and 2 episodes'],
	])('Should preserve TV detail variant %s', (tvShows, seasons, episodes, message) => {
		const report = generateResultDTO({ movies: 0, tvShows, seasons, episodes } as DownloadTaskCreationReportDTO);

		expect(translateDownloadNotification(report, request(PlexMediaType.None))).toBe(message);
	});
});
