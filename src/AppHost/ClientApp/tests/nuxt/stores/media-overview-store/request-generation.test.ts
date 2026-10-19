import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { Subject } from 'rxjs';
import { baseSetup, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
} from '@mock';
import { plexLibraryApi, plexMediaApi } from '@api';
import { useLibraryStore, useMediaOverviewStore } from '@store';
import {
	type PlexLibraryDTO,
	type PlexMediaFilterMetadataDTO,
	type PlexMediaMetadataDTO,
	type PlexMediaStatisticsDTO,
	PlexMediaType,
} from '@dto';
import type { ResultDTO } from '@interfaces';

describe('MediaOverviewStore - Request generation ownership', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		getAxiosMock();
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	function createPage(title: string, queryHash: string, totalCount: number, page = 1): PlexMediaStatisticsDTO {
		const mediaList = generatePlexMediaSlims({
			config: { seed: 263, movieCount: 1 },
			partialData: {
				id: title.charCodeAt(0),
				title,
				searchTitle: title.toLowerCase(),
				plexServerId: 1,
				plexLibraryId: 0,
				type: PlexMediaType.Movie,
			},
		});
		const pageData = generatePlexMediaStatisticsDTO(mediaList);
		pageData.page = page;
		pageData.pageSize = 100;
		pageData.queryHash = queryHash;
		pageData.mediaCount = totalCount;
		pageData.totalCount = totalCount;
		return pageData;
	}

	function createLibrary(id: number): PlexLibraryDTO {
		return {
			id,
			type: PlexMediaType.Movie,
			isEnabled: true,
			syncedAt: '2026-01-01T00:00:00.000Z',
		} as PlexLibraryDTO;
	}

	function createMetadata(mediaCount: number): PlexMediaMetadataDTO {
		return {
			mediaCount,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
			roleCount: 0,
			countries: [],
			genres: [],
			qualities: [],
			roles: [],
		};
	}

	function createFilterMetadata(ids: number): PlexMediaFilterMetadataDTO {
		return {
			countries: [ids],
			genres: [ids + 1],
			qualities: [ids + 2],
			roles: [ids + 3],
		};
	}

	test('Should load required metadata after a search supersedes a pending library lookup', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const libraryStore = useLibraryStore();
		const library = createLibrary(4);
		libraryStore.updateLibrary(library);
		const libraryResponse = new Subject<ResultDTO<PlexLibraryDTO>>();
		const metadataResponse = new Subject<ResultDTO<PlexMediaMetadataDTO>>();
		const filterMetadataResponse = new Subject<ResultDTO<PlexMediaFilterMetadataDTO>>();
		const searchResponse = new Subject<ResultDTO<PlexMediaStatisticsDTO>>();
		const secondPage = createPage('S', 'query-second', 1);
		const metadataRequest = vi.spyOn(plexLibraryApi, 'getLibraryMediaMetadata').mockReturnValue(metadataResponse.asObservable());
		const filterMetadataRequest = vi.spyOn(plexLibraryApi, 'getMetadataFilter').mockReturnValue(filterMetadataResponse.asObservable());
		vi.spyOn(plexLibraryApi, 'getPlexLibraryByIdEndpoint').mockReturnValue(libraryResponse.asObservable());
		const mediaRequest = vi.spyOn(plexMediaApi, 'getAllMediaByTypeEndpoint').mockReturnValue(searchResponse.asObservable());

		// Act
		const initialization = subscribeSpyTo(store.initializeLibrary(library.id));
		const search = subscribeSpyTo(store.setFilterQuery('second'));
		expect(metadataRequest).not.toHaveBeenCalled();
		expect(filterMetadataRequest).not.toHaveBeenCalled();
		searchResponse.next(generateResultDTO(secondPage));
		searchResponse.complete();
		await search.onComplete();
		libraryResponse.next(generateResultDTO(library));
		libraryResponse.complete();
		expect(metadataRequest).toHaveBeenCalledTimes(1);
		expect(filterMetadataRequest).toHaveBeenCalledTimes(1);
		metadataResponse.next(generateResultDTO(createMetadata(42)));
		metadataResponse.complete();
		filterMetadataResponse.next(generateResultDTO(createFilterMetadata(40)));
		filterMetadataResponse.complete();
		await initialization.onComplete();

		// Assert
		expect(mediaRequest).toHaveBeenCalledWith(expect.objectContaining({ q: 'second' }));
		expect(store.filterQuery).toBe('second');
		expect(store.metadataList.mediaCount).toBe(42);
		expect(store.availableCountryIds).toEqual([40]);
		expect(store.availableGenreIds).toEqual([41]);
		expect(store.availableQualityIds).toEqual([42]);
		expect(store.availableRoleIds).toEqual([43]);
		expect(store.getMediaItems).toEqual(secondPage.mediaList);
	});

	test('Should cancel obsolete library metadata and media when switching libraries', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const libraryStore = useLibraryStore();
		const firstLibrary = createLibrary(1);
		const secondLibrary = createLibrary(2);
		libraryStore.updateLibrary(firstLibrary);
		libraryStore.updateLibrary(secondLibrary);
		const firstLibraryResponse = new Subject<ResultDTO<PlexLibraryDTO>>();
		const secondLibraryResponse = new Subject<ResultDTO<PlexLibraryDTO>>();
		const firstMetadataResponse = new Subject<ResultDTO<PlexMediaMetadataDTO>>();
		const secondMetadataResponse = new Subject<ResultDTO<PlexMediaMetadataDTO>>();
		const firstFilterMetadataResponse = new Subject<ResultDTO<PlexMediaFilterMetadataDTO>>();
		const secondFilterMetadataResponse = new Subject<ResultDTO<PlexMediaFilterMetadataDTO>>();
		const firstMediaResponse = new Subject<ResultDTO<PlexMediaStatisticsDTO>>();
		const secondMediaResponse = new Subject<ResultDTO<PlexMediaStatisticsDTO>>();
		vi.spyOn(plexLibraryApi, 'getPlexLibraryByIdEndpoint').mockImplementation((libraryId) =>
			libraryId === firstLibrary.id ? firstLibraryResponse.asObservable() : secondLibraryResponse.asObservable());
		vi.spyOn(plexLibraryApi, 'getLibraryMediaMetadata').mockImplementation((libraryId) =>
			libraryId === firstLibrary.id ? firstMetadataResponse.asObservable() : secondMetadataResponse.asObservable());
		vi.spyOn(plexLibraryApi, 'getMetadataFilter').mockImplementation((libraryId) =>
			libraryId === firstLibrary.id ? firstFilterMetadataResponse.asObservable() : secondFilterMetadataResponse.asObservable());
		vi.spyOn(plexMediaApi, 'getAllMediaByTypeEndpoint').mockImplementation((query) =>
			query.plexLibraryId === firstLibrary.id ? firstMediaResponse.asObservable() : secondMediaResponse.asObservable());
		const firstInitialization = subscribeSpyTo(store.initializeLibrary(firstLibrary.id));
		firstLibraryResponse.next(generateResultDTO(firstLibrary));
		const secondInitialization = subscribeSpyTo(store.initializeLibrary(secondLibrary.id));

		// Act
		await firstInitialization.onComplete();
		firstMetadataResponse.next(generateResultDTO(createMetadata(11)));
		firstFilterMetadataResponse.next(generateResultDTO(createFilterMetadata(10)));
		firstMediaResponse.next(generateResultDTO(createPage('O', 'old-library', 1)));
		firstMetadataResponse.complete();
		firstFilterMetadataResponse.complete();
		firstMediaResponse.complete();
		expect(store.libraryId).toBe(secondLibrary.id);
		expect(store.metadataList.mediaCount).toBe(0);
		expect(store.availableRoleIds).toEqual([]);
		expect(store.getMediaItems).toEqual([]);
		secondLibraryResponse.next(generateResultDTO(secondLibrary));
		secondLibraryResponse.complete();
		secondMetadataResponse.next(generateResultDTO(createMetadata(22)));
		secondMetadataResponse.complete();
		secondFilterMetadataResponse.next(generateResultDTO(createFilterMetadata(20)));
		secondFilterMetadataResponse.complete();
		const newPage = createPage('N', 'new-library', 1);
		secondMediaResponse.next(generateResultDTO(newPage));
		secondMediaResponse.complete();
		await secondInitialization.onComplete();

		// Assert
		expect(store.libraryId).toBe(secondLibrary.id);
		expect(store.metadataList.mediaCount).toBe(22);
		expect(store.availableCountryIds).toEqual([20]);
		expect(store.availableRoleIds).toEqual([23]);
		expect(store.getMediaItems).toEqual(newPage.mediaList);
	});

	test('Should ignore an obsolete refresh response and preserve active loading controls', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const firstPage = createPage('A', 'query-a', 100);
		const secondPage = createPage('B', 'query-b', 50);
		const firstResponse = new Subject<ResultDTO<PlexMediaStatisticsDTO>>();
		const secondResponse = new Subject<ResultDTO<PlexMediaStatisticsDTO>>();
		const request = vi.spyOn(plexMediaApi, 'getAllMediaByTypeEndpoint').mockImplementation((query) => {
			return query.q === 'second' ? secondResponse.asObservable() : firstResponse.asObservable();
		});

		// Act
		const first = subscribeSpyTo(store.refreshMediaData());
		const second = subscribeSpyTo(store.setFilterQuery('second'));
		expect(store.loading).toBe(true);
		secondResponse.next(generateResultDTO(secondPage));
		secondResponse.complete();
		await second.onComplete();
		firstResponse.next(generateResultDTO(firstPage));
		firstResponse.complete();
		await first.onComplete();

		// Assert
		expect(request).toHaveBeenCalledTimes(2);
		expect(store.getMediaItems).toEqual(secondPage.mediaList);
		expect(store.queryHash).toBe('query-b');
		expect(store.itemsLength).toBe(1);
		expect(store.totalCount).toBe(50);
		expect(store.loading).toBe(false);
		expect(store.mediaLoadError).toBe(false);
	});

	test('Should deduplicate same-query concurrent pagination and release navigation loading once', async () => {
		// Arrange
		const store = useMediaOverviewStore();
		const pageResponse = new Subject<ResultDTO<PlexMediaStatisticsDTO>>();
		const request = vi.spyOn(plexMediaApi, 'getAllMediaByTypeEndpoint').mockReturnValue(pageResponse.asObservable());

		// Act
		const first = subscribeSpyTo(store.requestRange(100, 150));
		const second = subscribeSpyTo(store.requestRange(100, 150));
		expect(request).toHaveBeenCalledTimes(1);
		expect(store.navLoading).toBe(true);
		const page = createPage('P', 'pagination-query', 250, 2);
		pageResponse.next(generateResultDTO(page));
		pageResponse.complete();
		await first.onComplete();
		await second.onComplete();

		// Assert
		expect(store.getMediaItems).toEqual(page.mediaList);
		expect(store.itemsLength).toBe(1);
		expect(store.totalCount).toBe(250);
		expect(store.navLoading).toBe(false);
	});
});
