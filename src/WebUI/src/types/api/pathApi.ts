import Log from 'consola';
import IPath from '@dto/settings/iPath';
import { GlobalStore } from '@/store';

const logText = 'From folderPathApi => ';
const apiPath = '/folderpath';

export async function getFolderPaths(): Promise<IPath[]> {
	return await GlobalStore.Axios.get(`${apiPath}`)
		.then((x) => {
			Log.debug(logText + 'getFolderPaths response: ', x.data);
			return x.data;
		})
		.catch((e) => {
			Log.error(logText + 'getFolderPaths error: ', e);
			return e.response.status;
		});
}
