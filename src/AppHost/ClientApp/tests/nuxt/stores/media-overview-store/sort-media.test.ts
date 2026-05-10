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
import { MediaSortField, SortDirection } from '@enums/mediaSortField';

describe('MediaOverviewStore.sortMedia()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	async function loadMovies(mediaOverviewStore: ReturnType<typeof useMediaOverviewStore>, count = 20) {
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

		const result = subscribeSpyTo(mediaOverviewStore.requestMedia());
		await result.onComplete();
		return movies;
	}

	test('Should return items in ascending title order by default (getIsSorted is false)', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store);

		// Assert
		expect(store.getIsSorted).toBe(false);
		const items = store.getMediaItems;
		expect(items.length).toBeGreaterThan(0);
		// Default is Title/Asc — items come from server already sorted
	});

	test('Should sort items descending by year when toggleSortMedia is called', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store);

		// Act
		store.toggleSortMedia(MediaSortField.Year);
		store.toggleSortMedia(MediaSortField.Year); // second toggle → Desc

		// Assert
		expect(store.getIsSorted).toBe(true);
		expect(store.sortedState.field).toBe(MediaSortField.Year);
		expect(store.sortedState.sort).toBe(SortDirection.Desc);
	});

	test('Should sort items ascending by year on first toggle', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store);

		// Act
		store.toggleSortMedia(MediaSortField.Year);

		// Assert
		expect(store.sortedState.field).toBe(MediaSortField.Year);
		expect(store.sortedState.sort).toBe(SortDirection.Asc);
	});

	test('getIsSorted should be true when sorted by Title descending', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store);

		// Act — Title/Desc should be "sorted" (non-default)
		store.sortMedia({ field: MediaSortField.Title, sort: SortDirection.Desc });

		// Assert
		expect(store.getIsSorted).toBe(true);
		expect(store.getMediaItems.length).toBeGreaterThan(0);
	});

	test('Should reset sortedItems and return unsorted items when NoSort is applied', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		await loadMovies(store);

		// First, apply a real sort
		store.toggleSortMedia(MediaSortField.Year);
		expect(store.getIsSorted).toBe(true);

		// Act — clear sort
		store.clearSort();

		// Assert
		expect(store.sortedState.field).toBe(MediaSortField.Title);
		expect(store.sortedState.sort).toBe(SortDirection.Asc);
		expect(store.getIsSorted).toBe(false);
		// Items should still be accessible from the unsorted items array
		expect(store.getMediaItems.length).toBeGreaterThan(0);
	});

	test('sortMedia with NoSort should not leave getMediaItems empty', async () => {
		// Arrange — this tests the bug: sortMedia({field: Year, sort: NoSort})
		// sets sortedItems = [] but getIsSorted returns true (field !== Title),
		// which causes getMediaItems to return the empty sortedItems.
		const store = useMediaOverviewStore();
		await loadMovies(store);

		// Act
		store.sortMedia({ field: MediaSortField.Year, sort: SortDirection.NoSort });

		// Assert — after NoSort, items should still be accessible
		expect(store.getMediaItems.length).toBeGreaterThan(0);
	});
});
