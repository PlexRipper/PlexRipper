import type { MockConfig } from '@mock';
import type { ResultDTO } from '@interfaces';
import { checkConfig } from '@mock/mock-base';
import type { BaseResultDTO } from '@dto';

export function generateResultDTO<T>(value: T, config: Partial<MockConfig> = {}): ResultDTO<T> {
	checkConfig(config);

	return {
		value,
		errors: [],
		isSuccess: true,
		successes: [],
		statusCode: 0,
	};
}

export function generateFailedResultDTO(partial: Partial<BaseResultDTO> = {}): BaseResultDTO {
	return {
		errors: [],
		isSuccess: false,
		successes: [],
		statusCode: 0,
		...partial,
	};
}
