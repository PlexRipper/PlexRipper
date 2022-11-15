import { MockConfig } from '@mock/interfaces/MockConfig';
import ResultDTO from '@dto/ResultDTO';
import { checkConfig } from '@mock/mock-base';

export function generateResultDTO<T>(config: MockConfig | null = null, value: T): ResultDTO<T> {
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
