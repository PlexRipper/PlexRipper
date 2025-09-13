import type { BaseResultDTO } from '@dto';

export interface ResultDTO<T = void> extends BaseResultDTO {
	value?: T;
}
