import type { JobStatus, JobTypes } from '@dto';
import type { JobStatusUpdateDTO } from '@api';
import { incrementSeed } from '~/mock-data';

let jobStatusIdIndex = 1;

export function generateJobStatusUpdate<T>({
	jobType,
	jobStatus,
	data,
}: {
	jobType: JobTypes;
	jobStatus: JobStatus;
	data: T;
}): JobStatusUpdateDTO<T> {
	const id = jobStatusIdIndex++;
	incrementSeed(id);

	return {
		id: '' + id,
		jobStartTime: Date.now().toString(),
		jobType,
		status: jobStatus,
		data,
	};
}
