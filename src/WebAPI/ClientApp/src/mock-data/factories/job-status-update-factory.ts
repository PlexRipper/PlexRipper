import { JobStatus, JobTypes, type JobStatusUpdateDTO } from '@dto/mainApi';
import { incrementSeed } from '~/mock-data';

let jobStatusIdIndex = 1;

export function generateJobStatusUpdate({
	jobType,
	jobStatus,
	primaryKey,
	primaryKeyValue,
}: {
	jobType: JobTypes;
	jobStatus: JobStatus;
	primaryKey: string;
	primaryKeyValue: number;
}): JobStatusUpdateDTO {
	const id = jobStatusIdIndex++;
	incrementSeed(id);

	return {
		id: '' + id,
		jobName: 'asdasd',
		jobGroup: jobType,
		jobRuntime: '0.0s',
		jobStartTime: Date.now().toString(),
		jobType,
		primaryKey,
		primaryKeyValue,
		status: jobStatus,
	};
}
