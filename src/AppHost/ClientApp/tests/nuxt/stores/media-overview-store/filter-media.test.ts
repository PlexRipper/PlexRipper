import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
} from '@mock';
import { useMediaOverviewStore } from '@store';
import { PlexMediaType } from '@dto';

describe('MediaOverviewStore - Filter / Search', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	async function loadMovies(store: ReturnType<typeof useMediaOverviewStore>, count = 20) {
		const type = PlexMediaType.Movie;
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: count },
			partialData: { plexServerId: 1, plexLibraryId: 0, type },
		}));

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

		await subscribeSpyTo(store.refreshMediaData()).onComplete();
		return movies;
	}

	test('getMediaItems should return all items when filterQuery is empty', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = await loadMovies(store, 20);

		// Assert
		expect(store.getMediaItems.length).toBe(movies.mediaCount);
	});

	test('getMediaItems should return backend items when filterQuery is set', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = await loadMovies(store, 20);

		// Act
		store.filterQuery = 'backend-search';

		// Assert
		expect(store.getMediaItems.length).toBe(movies.mediaCount);
	});

	test('hasNoSearchResults should be true when backend returns no items for filterQuery', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store, 10);
		store.addMediaPage(generatePlexMediaStatisticsDTO([]));

		// Act
		store.filterQuery = 'zzz-no-match-xyz-impossible-string';

		// Assert
		expect(store.hasNoSearchResults).toBe(true);
		expect(store.getMediaItems.length).toBe(0);
	});

	test('hasNoSearchResults should be false when filterQuery is empty', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store, 10);

		// Assert
		expect(store.hasNoSearchResults).toBe(false);
	});

	test('clearFilter should reset filterQuery to empty string', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store, 10);
		store.filterQuery = 'some-search';
		expect(store.filterQuery).toBe('some-search');

		// Act
		const result = subscribeSpyTo(store.clearFilter());
		await result.onComplete();

		// Assert
		expect(store.filterQuery).toBe('');
	});

	test('setFilterQuery should request media from the backend', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const firstResponse = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 20 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		const secondResponse = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 3 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));

		mock.onGet(new RegExp(`/api/PlexMedia`))
			.replyOnce(200, generateResultDTO(firstResponse))
			.onGet(new RegExp(`/api/PlexMedia`))
			.reply(200, generateResultDTO(secondResponse));
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
		await subscribeSpyTo(store.refreshMediaData()).onComplete();

		// Act
		const result = subscribeSpyTo(store.setFilterQuery('matrix'));
		await result.onComplete();

		// Assert
		expect(store.filterQuery).toBe('matrix');
		expect(store.getMediaItems.length).toBe(secondResponse.mediaCount);
	});
});
