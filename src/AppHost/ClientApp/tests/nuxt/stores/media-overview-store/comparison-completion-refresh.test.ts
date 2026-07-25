import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	generatePlexLibrary,
	generatePlexMediaSlims,
	generatePlexMediaStatisticsDTO,
	generateResultDTO,
	Seed,
} from '@mock';
import { useLibraryStore, useMediaOverviewStore } from '@store';
import { type LibraryComparisonCompletedDTO, type PlexMediaSlimDTO, PlexMediaType } from '@dto';


describe('MediaOverviewStore.refreshCurrentMediaDataWhenComparisonCompleted()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	function createMediaPage(page: number, queryHash: string): ReturnType<typeof generatePlexMediaStatisticsDTO> {
		const mediaItems = generatePlexMediaSlims({
			config: { tvShowCount: 3 },
			partialData: {
				plexServerId: 1,
				plexLibraryId: 17,
				type: PlexMediaType.TvShow,
			},
		}).map((item, index) => ({
			...item,
			id: (page * 100) + index,
			sortIndex: (page * 100) + index,
		})) as PlexMediaSlimDTO[];
		const pageData = generatePlexMediaStatisticsDTO(mediaItems);
		pageData.page = page;
		pageData.pageSize = 100;
		pageData.queryHash = queryHash;
		pageData.mediaCount = mediaItems.length;
		pageData.totalCount = mediaItems.length;
		return pageData;
	}

	test('Should re-request cached pages when current library is included in affected library ids', async () => {
		// Arrange
		const libraryId = 17;
		const mediaOverviewStore = useMediaOverviewStore();
		const libraryStore = useLibraryStore();
		const library = generatePlexLibrary({
			seed: new Seed(4817),
			plexServerId: 1,
			type: PlexMediaType.TvShow,
			partialData: {
				id: libraryId,
			},
		});
		libraryStore.updateLibrary(library);
		mediaOverviewStore.libraryId = libraryId;

		const initialPage = createMediaPage(1, 'initial-query-hash');
		const refreshedPage = createMediaPage(1, 'refreshed-query-hash');
		let requestCount = 0;
		mock.onGet(new RegExp('/api/PlexMedia')).reply(() => {
			requestCount++;
			return [200, generateResultDTO(requestCount === 1 ? initialPage : refreshedPage)];
		});

		await subscribeSpyTo(mediaOverviewStore.requestMediaPage(1)).onComplete();
		const notification: LibraryComparisonCompletedDTO = {
			affectedLibraryIds: [214, libraryId],
			mediaType: PlexMediaType.TvShow,
			completedAt: new Date().toISOString(),
		};

		// Act
		const result = subscribeSpyTo(mediaOverviewStore.refreshCurrentMediaDataWhenComparisonCompleted(notification));
		await result.onComplete();

		// Assert
		expect(result.receivedComplete()).toEqual(true);
		expect(mock.history.get.filter((request) => request.url === '/api/PlexMedia')).toHaveLength(2);
		expect(mediaOverviewStore.queryHash).toBe('refreshed-query-hash');
		expect(mediaOverviewStore.getMediaItems).toEqual(refreshedPage.mediaList);
	});

	test('Should ignore comparison completion when current library is not affected', async () => {
		// Arrange
		const libraryId = 17;
		const mediaOverviewStore = useMediaOverviewStore();
		const libraryStore = useLibraryStore();
		const library = generatePlexLibrary({
			seed: new Seed(4818),
			plexServerId: 1,
			type: PlexMediaType.TvShow,
			partialData: {
				id: libraryId,
			},
		});
		libraryStore.updateLibrary(library);
		mediaOverviewStore.libraryId = libraryId;

		const initialPage = createMediaPage(1, 'initial-query-hash');
		mock.onGet(new RegExp('/api/PlexMedia')).reply(200, generateResultDTO(initialPage));

		await subscribeSpyTo(mediaOverviewStore.requestMediaPage(1)).onComplete();
		const notification: LibraryComparisonCompletedDTO = {
			affectedLibraryIds: [214, 18],
			mediaType: PlexMediaType.TvShow,
			completedAt: new Date().toISOString(),
		};

		// Act
		const result = subscribeSpyTo(mediaOverviewStore.refreshCurrentMediaDataWhenComparisonCompleted(notification));
		await result.onComplete();

		// Assert
		expect(result.receivedComplete()).toEqual(true);
		expect(mock.history.get.filter((request) => request.url === '/api/PlexMedia')).toHaveLength(1);
		expect(mediaOverviewStore.queryHash).toBe('initial-query-hash');
		expect(mediaOverviewStore.getMediaItems).toEqual(initialPage.mediaList);
	});
});
