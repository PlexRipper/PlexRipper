import { describe, beforeAll, test, expect, beforeEach, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { subscribeSpyTo, baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useBackgroundJobsStore, useMediaOverviewStore } from '@store';
import { BackgroundJobsPaths } from '@api/generated/BackgroundJobs';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { generateJobStatusUpdate } from '@factories';
import { JobStatus, JobTypes, PlexMediaType } from '@dto';
import { of } from 'rxjs';

describe('BackgroundJobsStore.setup()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return success and complete when setup is run', async () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.BackgroundJobsStore,
		};

		// Act
		const result = subscribeSpyTo(backgroundJobsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});

	test('Should report failure when fetching background jobs fails', async () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();
		mock.onGet(BackgroundJobsPaths.getAllBackgroundJobsEndpoint()).reply(500, generateFailedResultDTO());
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.BackgroundJobsStore,
		};

		// Act
		const result = subscribeSpyTo(backgroundJobsStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(result.receivedComplete()).toEqual(true);
	});

	test('Should ignore malformed comparison history and keep refreshing valid live completions', async () => {
		// Arrange
		const mediaOverviewStore = useMediaOverviewStore();
		const refreshSpy = vi.spyOn(mediaOverviewStore, 'refreshCurrentMediaDataWhenComparisonCompleted').mockReturnValue(of(null));
		const invalidHistory = generateJobStatusUpdate({
			jobType: JobTypes.LibraryComparisonJob,
			jobStatus: JobStatus.Completed,
			data: {
				affectedLibraryIds: [1],
				completedAt: '2026-10-19T00:00:00.000Z',
				mediaType: PlexMediaType.Movie,
			},
			partial: { jsonString: '' },
		});
		mock.onGet(BackgroundJobsPaths.getAllBackgroundJobsEndpoint()).reply(200, generateResultDTO([invalidHistory]));
		const backgroundJobsStore = useBackgroundJobsStore();

		// Act
		await subscribeSpyTo(backgroundJobsStore.setup()).onComplete();

		// Assert
		expect(refreshSpy).not.toHaveBeenCalled();

		// Act
		backgroundJobsStore.setStatusJobUpdate(generateJobStatusUpdate({
			jobType: JobTypes.LibraryComparisonJob,
			jobStatus: JobStatus.Completed,
			data: {
				affectedLibraryIds: [1],
				completedAt: '2026-10-19T00:00:01.000Z',
				mediaType: PlexMediaType.Movie,
			},
		}));

		// Assert
		expect(refreshSpy).toHaveBeenCalledOnce();
	});
});
