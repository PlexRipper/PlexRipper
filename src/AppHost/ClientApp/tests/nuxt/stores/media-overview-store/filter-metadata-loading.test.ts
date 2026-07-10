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

describe('MediaOverviewStore - Filter / Nav loading states', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	function setupMediaMock(movies: PlexMediaStatisticsDTO) {
		mock.onGet(new RegExp('/api/PlexMedia')).reply(200, generateResultDTO(movies));
	}

	test('filterMetadataLoading should be false initially', () => {
		const store = useMediaOverviewStore();
		expect(store.filterMetadataLoading).toBe(false);
	});

	test('navLoading should be false initially', () => {
		const store = useMediaOverviewStore();
		expect(store.navLoading).toBe(false);
	});

	test('navLoading should be false after requestRange gets only cached pages', () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 10 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		movies.page = 1;
		movies.navigationIndexes = [{ label: 'A', index: 0 }];
		setupMediaMock(movies);

		// Load page 1 first
		store.addMediaPage(movies);

		// Act — request same page range already cached
		subscribeSpyTo(store.requestRange(0, 9));

		// Assert
		expect(store.navLoading).toBe(false);
	});

	test('scrollDict should only be set on first page with valid navigation indexes', () => {
		// Arrange
		const store = useMediaOverviewStore();

		const page1 = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 10 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		page1.page = 1;
		page1.navigationIndexes = [
			{ label: 'A', index: 0 },
			{ label: 'B', index: 3 },
			{ label: 'C', index: 7 },
		];

		const page2 = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 10, seed: 200 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		page2.page = 2;
		// Different nav indexes — should be ignored since scrollDict already set
		page2.navigationIndexes = [{ label: 'X', index: 999 }];

		setupMediaMock(page1);

		// Act — load page 1, then page 2
		store.addMediaPage(page1);
		store.addMediaPage(page2);

		// Assert — scrollDict should still have page 1's data
		const entries = Array.from(store.scrollDict.entries());
		expect(entries).toHaveLength(3);
		expect(entries[0]).toEqual(['A', 0]);
		expect(entries[1]).toEqual(['B', 3]);
		expect(entries[2]).toEqual(['C', 7]);
	});

	test('filter arrays should be empty in addMediaPage result', () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 5 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		movies.page = 1;
		movies.navigationIndexes = [{ label: 'A', index: 0 }];
		// Explicitly set filter arrays to verify they're not overwritten
		movies.roles = [99];
		movies.countries = [88];
		movies.genres = [77];
		movies.qualities = [66];
		setupMediaMock(movies);

		// Act
		store.addMediaPage(movies);

		// Assert — filter arrays should NOT be populated from page response anymore
		// They come from metadata-filter endpoint instead
		expect(store.availableRoleIds).toEqual([]);
		expect(store.availableCountryIds).toEqual([]);
		expect(store.availableGenreIds).toEqual([]);
		expect(store.availableQualityIds).toEqual([]);
	});
});
