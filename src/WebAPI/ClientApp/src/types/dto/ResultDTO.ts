import type { IError, ReasonDTO, SuccessDTO } from '@dto/mainApi';

export default interface ResultDTO<T = void> {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[] | null;
	errors?: IError[] | null;
	successes?: SuccessDTO[] | null;
	value?: T;
}
