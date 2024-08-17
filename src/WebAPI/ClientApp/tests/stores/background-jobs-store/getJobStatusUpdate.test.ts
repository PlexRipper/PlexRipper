import { beforeAll, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { JobStatus, JobTypes } from '@dto';
import { generateJobStatusUpdate } from '@factories';
import { useBackgroundJobsStore } from '#build/imports';

describe('BackgroundJobsStore.getJobStatusUpdate()', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		setActivePinia(createPinia());
	});

	test('Should return the statusJobUpdate when its set', () => {
		// Arrange
		const backgroundJobsStore = useBackgroundJobsStore();

		const testMsg = generateJobStatusUpdate({
			jobType: JobTypes.InspectPlexServerJob,
			jobStatus: JobStatus.Started,
			data: [4],
		});

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
			jobType: JobTypes.SyncServerMediaJob,
			jobStatus: JobStatus.Started,
			data: {
				plexServerId: 1,
				forceSync: false,
			},
		});

		// Act
		backgroundJobsStore.setup();
		const result = subscribeSpyTo(backgroundJobsStore.getSyncServerMediaJobUpdate(JobStatus.Started));

		backgroundJobsStore.setStatusJobUpdate(testMsg);

		// Assert
		expect(result.getFirstValue()).toEqual(testMsg);
	});
});
