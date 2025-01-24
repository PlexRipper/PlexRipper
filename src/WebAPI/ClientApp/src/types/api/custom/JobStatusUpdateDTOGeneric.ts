import type { JobStatusUpdateDTO as ApiJobStatusUpdateDTO } from '@dto';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export interface JobStatusUpdateDTO<T = any> extends ApiJobStatusUpdateDTO {
	data: T;
}
