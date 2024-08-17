import type { JobStatusUpdateDTOOfObject } from '@dto';

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export interface JobStatusUpdateDTO<T = any> extends JobStatusUpdateDTOOfObject {
	data: T;
}
