import type { ResultDTO } from '@interfaces';
import type { BaseResultDTO } from '@dto';

export function generateResultDTO<T>(value: T): ResultDTO<T> {
	return {
		value: value,
		isSuccess: true,
		statusCode: 200,
		errors: [],
		successes: [],
	};
}

export function generateFailedResultDTO(partial: Partial<BaseResultDTO> = {}): BaseResultDTO {
	return {
		isSuccess: false,
		statusCode: 0,
		errors: [],
		successes: [],
		...partial,
	};
}
