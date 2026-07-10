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
import { nextTick } from 'vue';

describe('MediaOverviewStore - Filter / Nav loading states', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	function setupSuccessMocks(movies: PlexMediaStatisticsDTO) {
		mock.onGet(new RegExp('/api/PlexMedia')).reply(200, generateResultDTO(movies));
		mock.onGet(new RegExp('/api/PlexLibrary/0/metadata')).reply(200, generateResultDTO({
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
		mock.onGet(new RegExp('/api/PlexLibrary/0/metadata-filter')).reply(200, generateResultDTO({
			roles: [1, 2, 3],
			countries: [4, 5],
			genres: [6, 7, 8],
			qualities: [9],
		}));
	}

	test('filterMetadataLoading should be false initially', () => {
		// Arrange
		const store = useMediaOverviewStore();

		// Assert
		expect(store.filterMetadataLoading).toBe(false);
	});

	test('navLoading should be false initially', () => {
		// Arrange
		const store = useMediaOverviewStore();

		// Assert
		expect(store.navLoading).toBe(false);
	});

	test('filterMetadataLoading should be false after refreshFilterMetadata completes', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 10 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		setupSuccessMocks(movies);

		// Act
		const result = subscribeSpyTo(store.refreshFilterMetadata());
		await result.onComplete();

		// Assert
		expect(store.filterMetadataLoading).toBe(false);
		expect(result.receivedComplete()).toBe(true);
	});

	test('should store availableRoleIds from filter metadata response', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 5 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		setupSuccessMocks(movies);

		// Act
		const result = subscribeSpyTo(store.refreshFilterMetadata());
		await result.onComplete();

		// Assert
		expect(store.availableRoleIds).toEqual([1, 2, 3]);
		expect(store.availableCountryIds).toEqual([4, 5]);
		expect(store.availableGenreIds).toEqual([6, 7, 8]);
		expect(store.availableQualityIds).toEqual([9]);
		expect(result.receivedComplete()).toBe(true);
	});

	test('navLoading should be false after requestRange gets cached pages', () => {
		// Arrange
		const store = useMediaOverviewStore();
		const movies = generatePlexMediaStatisticsDTO(generatePlexMediaSlims({
			config: { movieCount: 100 },
			partialData: { plexServerId: 1, plexLibraryId: 0, type: PlexMediaType.Movie },
		}));
		movies.page = 1;
		setupSuccessMocks(movies);

		// Load page 1 first
		store.addMediaPage(movies);

		// Act — request same page range that is already cached
		const result = subscribeSpyTo(store.requestRange(0, 99));

		// Assert
		expect(store.navLoading).toBe(false);
	});

	test('scrollDict should only be set once across multiple page loads', async () => {
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
		// Page 2 has DIFFERENT navigation indexes — but should be ignored
		page2.navigationIndexes = [
			{ label: 'X', index: 999 },
		];

		setupSuccessMocks(page1);

		// Act — load page 1, then page 2
		store.addMediaPage(page1);
		store.addMediaPage(page2);

		// Assert — scrollDict should still have page 1's data, not page 2's
		const entries = Array.from(store.scrollDict.entries());
		expect(entries).toHaveLength(3);
		expect(entries[0]).toEqual(['A', 0]);
		expect(entries[1]).toEqual(['B', 3]);
		expect(entries[2]).toEqual(['C', 7]);
	});
});
