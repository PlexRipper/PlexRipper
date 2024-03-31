import type { IError, ReasonDTO, SuccessDTO } from '@dto';

export interface ResultDTO<T = void> {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[] | null;
	errors?: IError[] | null;
	successes?: SuccessDTO[] | null;
	value?: T;
}
