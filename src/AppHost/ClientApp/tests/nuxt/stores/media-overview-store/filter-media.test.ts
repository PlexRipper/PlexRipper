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

		await subscribeSpyTo(store.requestMedia()).onComplete();
		return movies;
	}

	test('getMediaItems should return all items when filterQuery is empty', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = await loadMovies(store, 20);

		// Assert
		expect(store.getMediaItems.length).toBe(movies.mediaCount);
	});

	test('getMediaItems should filter items by searchTitle (case-insensitive)', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store, 20);

		// Find an actual item to search for
		const target = store.items[0]!;
		// searchTitle is kebab-case lowercase of the title
		const partialSearch = target.searchTitle.slice(0, 3);

		// Act
		store.filterQuery = partialSearch;

		// Assert
		const filtered = store.getMediaItems;
		expect(filtered.length).toBeGreaterThan(0);
		for (const item of filtered) {
			expect(item.searchTitle).toContain(partialSearch.toLowerCase());
		}
	});

	test('hasNoSearchResults should be true when filterQuery yields no results', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store, 10);

		// Act — use a query that will match nothing
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
		store.clearFilter();

		// Assert
		expect(store.filterQuery).toBe('');
	});

	test('getMediaItems should still filter correctly when sorted', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store, 20);

		// Apply sort
		store.toggleSortMedia('year' as any);

		// Get a search term from an actual item
		const target = store.items[0]!;
		const partialSearch = target.searchTitle.slice(0, 3);

		// Act
		store.filterQuery = partialSearch;

		// Assert — should filter from sortedItems
		expect(store.getIsSorted).toBe(true);
		const filtered = store.getMediaItems;
		for (const item of filtered) {
			expect(item.searchTitle).toContain(partialSearch.toLowerCase());
		}
	});
});
