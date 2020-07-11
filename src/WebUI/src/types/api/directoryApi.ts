import Log from 'consola';
import { IFileSystem } from '@dto/settings/paths/IFileSystem';
import { GlobalStore } from '@/store';

const logText = 'From DirectoryAPI => ';
const apiPath = '/directory';

export async function requestDirectories(path: string): Promise<IFileSystem> {
	return await GlobalStore.Axios.get(`${apiPath}/?path=${path}`)
		.then((x) => {
			Log.debug(logText + 'requestDirectories response: ', x.data);
			return x.data;
		})
		.catch((e) => {
			Log.error(logText + 'requestDirectories error: ', e);
			return e.response.status;
		});
}
