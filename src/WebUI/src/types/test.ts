import Axios from 'axios';
import Result from 'fluent-type-results';
import IPlexAccount from '@dto/IPlexAccount';

export async function getUserAccount(id: number): Promise<IPlexAccount | null> {
	const { data } = await Axios.get<Result<IPlexAccount>>(`/account/${id}`);

	if (data.isFailed) {
		console.log(data.toErrorString);
	}

	return data.value;
}
