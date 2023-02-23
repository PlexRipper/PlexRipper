import { Error, ReasonDTO, SuccessDTO } from '@dto/mainApi';

export default interface ResultDTO<T = void> {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: ReasonDTO[] | null;
	errors?: Error[] | null;
	successes?: SuccessDTO[] | null;
	value?: T;
}
