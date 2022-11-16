import { Error, IReason, ISuccess } from '@dto/mainApi';

export default interface ResultDTO<T = void> {
	isFailed?: boolean;
	isSuccess?: boolean;
	reasons?: IReason[] | null;
	errors?: Error[] | null;
	successes?: ISuccess[] | null;
	value?: T;
}
