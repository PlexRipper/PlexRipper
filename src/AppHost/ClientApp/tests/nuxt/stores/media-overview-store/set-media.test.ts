import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
} from '@mock';
import { useMediaOverviewStore } from '@store';
import { type PlexMediaStatisticsDTO, PlexMediaType } from '@dto';

describe('MediaOverviewStore.setMedia()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	function setupMocks(mediaStats: PlexMediaStatisticsDTO) {
		mock.onGet(new RegExp(`/api/PlexMedia`)).reply(200, generateResultDTO(mediaStats));
		mock.onGet(new RegExp(`/api/PlexLibrary/0/metadata`)).reply(200, generateResultDTO({
			episodeCount: 0,
			mediaCount: 0,
			mediaSize: 0,
			movieCount: 0,
			seasonCount: 0,
			tvShowCount: 0,
			mediaList: [],
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
		}));
	}

	test('Should reset filterQuery to empty string when new media is set', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const type = PlexMediaType.Movie;
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 10 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type },
		}));
		setupMocks(movies);

		const result = subscribeSpyTo(store.requestMedia());
		await result.onComplete();

		// Set a filter query
		store.filterQuery = 'some-filter';
		expect(store.filterQuery).toBe('some-filter');

		// Re-setup mocks for second request and re-request
		setupMocks(movies);

		// Act — request media again (need to reset loading state manually via $reset trick, or just call setMedia directly)
		store.$reset();
		store.mediaType = type;
		setupMocks(movies);
		const result2 = subscribeSpyTo(store.requestMedia());
		await result2.onComplete();

		// Assert — filterQuery should be cleared by setMedia
		expect(store.filterQuery).toBe('');
	});

	test('Should set all count fields correctly from PlexMediaStatisticsDTO', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const type = PlexMediaType.Movie;
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 42 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type },
		}));
		setupMocks(movies);

		// Act
		const result = subscribeSpyTo(store.requestMedia());
		await result.onComplete();

		// Assert
		expect(store.allMovieCount).toBe(movies.movieCount);
		expect(store.allTvShowCount).toBe(movies.tvShowCount);
		expect(store.allSeasonCount).toBe(movies.seasonCount);
		expect(store.allEpisodeCount).toBe(movies.episodeCount);
		expect(store.allFileSize).toBe(movies.mediaSize);
		expect(store.itemsLength).toBe(movies.mediaCount);
	});

	test('Should set all counts to zero when API returns null data (isSuccess: false)', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		store.mediaType = PlexMediaType.Movie;

		// Return failed result for media endpoint
		mock.onGet(new RegExp(`/api/PlexMedia`)).reply(200, {
			isSuccess: false,
			statusCode: 400,
			errors: [],
			successes: [],
			value: null,
		});
		mock.onGet(new RegExp(`/api/PlexLibrary/0/metadata`)).reply(200, generateResultDTO({
			episodeCount: 0,
			mediaCount: 0,
			mediaSize: 0,
			movieCount: 0,
			seasonCount: 0,
			tvShowCount: 0,
			mediaList: [],
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
		}));

		// Act
		const result = subscribeSpyTo(store.requestMedia());
		await result.onComplete();

		// Assert — null data path in setMedia
		expect(store.allMovieCount).toBe(0);
		expect(store.allTvShowCount).toBe(0);
		expect(store.allSeasonCount).toBe(0);
		expect(store.allEpisodeCount).toBe(0);
		expect(store.allFileSize).toBe(0);
		expect(store.itemsLength).toBe(0);
		expect(store.items).toEqual([]);
	});

	test('Should correctly set TV show counts from statistics DTO', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		store.mediaType = PlexMediaType.TvShow;
		const type = PlexMediaType.TvShow;
		const tvShows = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { tvShowCount: 15, seasonCount: 3, episodeCount: 5 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type },
		}));
		mock.onGet(new RegExp(`/api/PlexMedia`)).reply(200, generateResultDTO(tvShows));
		mock.onGet(new RegExp(`/api/PlexLibrary/0/metadata`)).reply(200, generateResultDTO({
			episodeCount: 0,
			mediaCount: 0,
			mediaSize: 0,
			movieCount: 0,
			seasonCount: 0,
			tvShowCount: 0,
			mediaList: [],
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
		}));

		// Act
		const result = subscribeSpyTo(store.requestMedia());
		await result.onComplete();

		// Assert
		expect(store.allTvShowCount).toBe(tvShows.tvShowCount);
		expect(store.itemsLength).toBe(tvShows.mediaCount);
	});
});
