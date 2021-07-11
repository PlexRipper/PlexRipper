import { Error, Reason, Success } from '@dto/mainApi';

export default interface ResultDTO<T = void> {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: Reason[] | null;
	errors?: Error[] | null;
	successes?: Success[] | null;
	value?: T;
}
