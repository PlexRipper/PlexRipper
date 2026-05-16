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

describe('MediaOverviewStore - Loading State', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	function setupSuccessMocks(movies: PlexMediaStatisticsDTO) {
		mock.onGet(new RegExp(`/api/PlexMedia`)).reply(200, generateResultDTO(movies));
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

	test('loading should be false before any request is made', () => {
		// Arrange
		const store = useMediaOverviewStore();

		// Assert
		expect(store.loading).toBe(false);
	});

	test('loading should be false after a successful requestMedia completes', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 10 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		setupSuccessMocks(movies);

		// Act
		const result = subscribeSpyTo(store.refreshMediaData());
		await result.onComplete();

		// Assert
		expect(store.loading).toBe(false);
		expect(result.receivedComplete()).toBe(true);
	});

	test('requestMedia should return of(null) immediately when loading is already true', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 5 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		setupSuccessMocks(movies);

		// Start first request but don't await it
		const first = subscribeSpyTo(store.refreshMediaData());

		// Act — second call while loading is true
		const second = subscribeSpyTo(store.refreshMediaData());
		await second.onComplete();

		// Assert — second call completes immediately with null (skipped)
		expect(second.getLastValue()).toBeNull();
		expect(second.receivedComplete()).toBe(true);

		// Clean up first
		await first.onComplete();
	});

	test('loading should be false after requestMedia with a failed API response', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		store.mediaType = PlexMediaType.Movie;

		mock.onGet(new RegExp(`/api/PlexMedia`)).reply(200, {
			isSuccess: false,
			statusCode: 500,
			errors: [{ message: 'Server error', reasons: [], metadata: {} }],
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
		const result = subscribeSpyTo(store.refreshMediaData());
		await result.onComplete();

		// Assert — loading must be reset to false even on failed response
		expect(store.loading).toBe(false);
	});
});
