import type { JobStatus, JobTypes } from '@dto';
import type { JobStatusUpdateDTO } from '@api';
import { format } from 'date-fns';
import { incrementSeed } from '~/mock-data';

let jobStatusIdIndex = 1;

export function generateJobStatusUpdate<T>({
	jobType,
	jobStatus,
	data,
	partial,
}: {
	jobType: JobTypes;
	jobStatus: JobStatus;
	data: T;
	partial?: Partial<JobStatusUpdateDTO<T>>;
}): JobStatusUpdateDTO<T> {
	const id = jobStatusIdIndex++;
	incrementSeed(id);

	return {
		id: '' + id,
		jobStartTime: format(new Date(), 'yyyy-MM-dd\'T\'HH:mm:ss.SSS'),
		jobType,
		status: jobStatus,
		data,
		...partial,
	};
}
