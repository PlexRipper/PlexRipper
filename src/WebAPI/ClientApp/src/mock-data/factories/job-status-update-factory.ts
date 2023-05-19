import { JobStatus, JobStatusUpdateDTO, JobTypes, TimeSpan } from '@dto/mainApi';
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
		jobRuntime: defaultTimeSpan,
		jobStartTime: Date.now().toString(),
		jobType,
		primaryKey,
		primaryKeyValue,
		status: jobStatus,
	};
}

const defaultTimeSpan: TimeSpan = {
	days: 0,
	hours: 0,
	microseconds: 0,
	milliseconds: 0,
	minutes: 0,
	nanoseconds: 0,
	seconds: 0,
	ticks: 0,
	totalDays: 0,
	totalHours: 0,
	totalMicroseconds: 0,
	totalMilliseconds: 0,
	totalMinutes: 0,
	totalNanoseconds: 0,
	totalSeconds: 0,
};
