import type { JobStatusUpdateDTOOfObject } from '@dto';

export interface JobStatusUpdateDTO<T = never> extends JobStatusUpdateDTOOfObject {
	data: T;
}
