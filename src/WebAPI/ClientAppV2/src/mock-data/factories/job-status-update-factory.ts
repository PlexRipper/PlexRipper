import * as Factory from 'factory.ts';
import { JobStatus, JobStatusUpdateDTO, JobTypes, TimeSpan } from '@dto/mainApi';

export function generateJobStatusUpdate({ jobType, jobStatus }: { jobType: JobTypes; jobStatus: JobStatus }): JobStatusUpdateDTO {
	const jobStatusUpdateDTOFactory = Factory.Sync.makeFactory<JobStatusUpdateDTO>(() => {
		return {
			id: Factory.each((i) => i) + '',
			jobName: 'asdasd',
			jobGroup: jobType,
			jobRuntime: defaultTimeSpan,
			jobStartTime: Date.now().toString(),
			jobType,
			primaryKey: '',
			primaryKeyValue: 0,
			status: jobStatus,
		};
	});

	return jobStatusUpdateDTOFactory.build();
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
