import type { MockConfig } from '@mock';
import type { ResultDTO } from '@interfaces';
import { checkConfig } from '@mock/mock-base';

export function generateResultDTO<T>(value: T, config: Partial<MockConfig> = {}): ResultDTO<T> {
	checkConfig(config);

	return {
		value,
		errors: [],
		isSuccess: true,
		isFailed: false,
		reasons: [],
		successes: [],
	};
}

export function generateFailedResultDTO(partial: Partial<ResultDTO> = {}): ResultDTO {
	return {
		errors: [],
		isSuccess: false,
		isFailed: true,
		reasons: [],
		successes: [],
		...partial,
	};
}
