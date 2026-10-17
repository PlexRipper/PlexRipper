import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { Subject } from 'rxjs';
import { baseSetup, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
} from '@mock';
import { plexMediaApi } from '@api';
import { useMediaOverviewStore } from '@store';
import { type PlexMediaStatisticsDTO, PlexMediaType } from '@dto';
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
