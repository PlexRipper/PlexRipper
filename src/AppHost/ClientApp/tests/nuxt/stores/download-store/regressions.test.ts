import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO } from '@mock';
import { DownloadActions, DownloadStatus, PlexMediaType, type CreateDownloadTasksRequest, type DownloadProgressDTO } from '@dto';
import { DownloadPaths } from '@api-urls';
import { StoreNames } from '@interfaces';
import { useDownloadStore } from '@store';

describe('DownloadStore regressions', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should report setup failure when download refresh fails', async () => {
		// Arrange
		const downloadStore = useDownloadStore();
		mock.onGet(DownloadPaths.getAllDownloadTasksEndpoint()).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(downloadStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual({
			name: StoreNames.DownloadStore,
			isSuccess: false,
		});
	});

	test('Should not refresh downloads when clearing selected tasks fails', async () => {
		// Arrange
		const downloadStore = useDownloadStore();
		mock.onDelete(DownloadPaths.clearCompletedDownloadTasksByDownloadTaskIdEndpoint()).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(downloadStore.executeDownloadCommand(DownloadActions.Clear, ['task-1']));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()?.isSuccess).toBe(false);
		expect(mock.history.get.filter((request) => request.url === DownloadPaths.getAllDownloadTasksEndpoint())).toHaveLength(0);
	});

	test('Should not refresh downloads when clearing a server fails', async () => {
		// Arrange
		const downloadStore = useDownloadStore();
		mock.onDelete(DownloadPaths.clearCompletedDownloadTasksByServerIdEndpoint(7)).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(downloadStore.executeDownloadCommand(DownloadActions.Clear, [], 7));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()?.isSuccess).toBe(false);
		expect(mock.history.get.filter((request) => request.url === DownloadPaths.getAllDownloadTasksEndpoint())).toHaveLength(0);
	});

	test('Should not refresh downloads when deleting tasks fails', async () => {
		// Arrange
		const downloadStore = useDownloadStore();
		mock.onDelete(DownloadPaths.deleteDownloadTaskEndpoint()).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		const result = subscribeSpyTo(downloadStore.executeDownloadCommand(DownloadActions.Delete, ['task-1']));
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()?.isSuccess).toBe(false);
		expect(mock.history.get.filter((request) => request.url === DownloadPaths.getAllDownloadTasksEndpoint())).toHaveLength(0);
	});

	test('Should return a failed result for unsupported download actions', async () => {
		// Arrange
		const downloadStore = useDownloadStore();

		// Act
		const result = subscribeSpyTo(downloadStore.executeDownloadCommand(999 as unknown as DownloadActions, ['task-1']));
		await result.onComplete();

		// Assert
		expect(result.getValues()).toHaveLength(1);
		expect(result.getFirstValue()).toEqual({
			errors: [],
			isSuccess: false,
			statusCode: 400,
			successes: [],
		});
	});

	test('Should not refresh downloads when creating download tasks fails', async () => {
		// Arrange
		const downloadStore = useDownloadStore();
		const request: CreateDownloadTasksRequest = {
			customDestinationFolderPath: '',
			destinationFolderPathId: null,
			downloadMedias: [],
		};
		mock.onPost(DownloadPaths.createDownloadTasksEndpoint()).reply(200, generateFailedResultDTO({ statusCode: 500 }));

		// Act
		downloadStore.downloadMedia(request);
		await Promise.resolve();
		await Promise.resolve();

		// Assert
		expect(mock.history.get.filter((request) => request.url === DownloadPaths.getAllDownloadTasksEndpoint())).toHaveLength(0);
	});

	test('Should include active download descendants deeper than three nested levels', () => {
		// Arrange
		const downloadStore = useDownloadStore();
		const deepLeaf: DownloadProgressDTO = {
			id: 'leaf',
			title: 'Leaf',
			status: DownloadStatus.Downloading,
			percentage: 25,
			dataReceived: 10,
			dataTotal: 100,
			downloadSpeed: 1,
			timeRemaining: 90,
			mediaType: PlexMediaType.Movie,
			children: [],
		};
		downloadStore.serverDownloads = [{
			id: 1,
			downloadableTasksCount: 1,
			downloads: [{
				id: 'root',
				title: 'Root',
				status: DownloadStatus.Queued,
				percentage: 0,
				dataReceived: 0,
				dataTotal: 100,
				downloadSpeed: 0,
				timeRemaining: 0,
				mediaType: PlexMediaType.Movie,
				children: [{
					id: 'level-1',
					title: 'Level 1',
					status: DownloadStatus.Queued,
					percentage: 0,
					dataReceived: 0,
					dataTotal: 100,
					downloadSpeed: 0,
					timeRemaining: 0,
					mediaType: PlexMediaType.Movie,
					children: [{
						id: 'level-2',
						title: 'Level 2',
						status: DownloadStatus.Queued,
						percentage: 0,
						dataReceived: 0,
						dataTotal: 100,
						downloadSpeed: 0,
						timeRemaining: 0,
						mediaType: PlexMediaType.Movie,
						children: [{
							id: 'level-3',
							title: 'Level 3',
							status: DownloadStatus.Queued,
							percentage: 0,
							dataReceived: 0,
							dataTotal: 100,
							downloadSpeed: 0,
							timeRemaining: 0,
							mediaType: PlexMediaType.Movie,
							children: [deepLeaf],
						}],
					}],
				}],
			}],
		}];

		// Act
		const result = downloadStore.getActiveDownloadList(1);

		// Assert
		expect(result.map((download) => download.id)).toContain('leaf');
	});
});
