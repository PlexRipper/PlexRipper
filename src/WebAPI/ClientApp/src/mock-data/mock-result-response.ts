import { MockConfig } from '@mock/interfaces/MockConfig';
import ResultDTO from '@dto/ResultDTO';
import { checkConfig } from '@mock/mock-base';

export function generateResultDTO<T>(value: T, config: MockConfig | null = null): ResultDTO<T> {
	config = checkConfig(config);

	return {
		value,
		errors: [],
		isSuccess: true,
		isFailed: false,
		reasons: [],
		successes: [],
	};
}
