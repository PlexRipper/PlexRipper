import type { ErrorDTO, ReasonDTO, SuccessDTO } from '@dto';

export interface ResultDTO<T = void> {
	isFailed: boolean;
	isSuccess: boolean;
	reasons: ReasonDTO[];
	errors: ErrorDTO[];
	successes: SuccessDTO[];
	value?: T;
	statusCode: number;
}
