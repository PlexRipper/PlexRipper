import { beforeAll, describe, expect, test } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, subscribeSpyTo } from '@services-test-base';
import { JobStatus, type JobStatusUpdateDTO, JobTypes } from '@dto';
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

		const testMsg: JobStatusUpdateDTO = {
			id: 'NON_CLUSTERED638568037901159617',
			jobName: 'plexAccountId_4',
			jobGroup: 'RefreshPlexServersAccessJob',
			jobType: JobTypes.InspectPlexServerJob,
			jobRuntime: '00:00:00.0194365',
			jobStartTime: '2024-07-17T09:03:10.1360007Z',
			status: JobStatus.Started,
			primaryKey: 'plexAccountId',
			primaryKeyValue: '4',
		};

		// Act
		backgroundJobsStore.setup();
		const result = subscribeSpyTo(backgroundJobsStore.getJobStatusUpdate(JobTypes.InspectPlexServerJob));

		backgroundJobsStore.setStatusJobUpdate(testMsg);

		// Assert
		expect(result.getFirstValue()).toEqual(testMsg);
	});
});
