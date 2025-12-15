import { beforeAll, beforeEach, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import {
	type InspectPlexServerJobUpdateDTO,
	JobStatus,
	JobTypes,
	type LibrarySyncJobQueueDTO,
	LibrarySyncJobStatus,
} from '@dto';
import { generateJobStatusUpdate } from '@factories';
import { useBackgroundJobsStore } from '@store';
import { generateResultDTO } from '@mock';
import { BackgroundJobsPaths } from '@api/generated/BackgroundJobs';

describe('BackgroundJobsStore.getJobStatusUpdate()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return the statusJobUpdate when its set', () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();

		const testMsg = generateJobStatusUpdate({
			jobType: JobTypes.InspectPlexServerJob,
			jobStatus: JobStatus.Started,
			data: { plexServerIds: [4] } as InspectPlexServerJobUpdateDTO,
		});

		mock.onGet(BackgroundJobsPaths.getAllBackgroundJobsEndpoint()).reply(200, generateResultDTO([]));

		// Act
		backgroundJobsStore.setup();
		const result = subscribeSpyTo(backgroundJobsStore.getJobStatusUpdate(JobTypes.InspectPlexServerJob));

		backgroundJobsStore.setStatusJobUpdate(testMsg);

		// Assert
		expect(result.getFirstValue()).toEqual(testMsg);
	});

	test('Should return the statusJobUpdate when its filtered', () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();

		const testMsg = generateJobStatusUpdate({
			jobType: JobTypes.LibrarySyncJob,
			jobStatus: JobStatus.Started,
			data: {
				completedAt: new Date().toISOString(),
				createdAt: '',
				errorMessage: '',
				plexLibraryId: 1,
				plexServerId: 1,
				priority: 1,
				startedAt: new Date().toISOString(),
				status: LibrarySyncJobStatus.Queued,
			} as LibrarySyncJobQueueDTO,
		});

		mock.onGet(BackgroundJobsPaths.getAllBackgroundJobsEndpoint()).reply(200, generateResultDTO([]));

		// Act
		backgroundJobsStore.setup();

		const result = subscribeSpyTo(backgroundJobsStore.getLibrarySyncJobUpdate(JobStatus.Started));

		backgroundJobsStore.setStatusJobUpdate(testMsg);

		// Assert
		expect(result.getFirstValue()).toEqual(testMsg);
	});
});
