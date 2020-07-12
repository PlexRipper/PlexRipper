import Log from 'consola';
import IPlexLibrary from '@dto/IPlexLibrary';
import { GlobalStore } from '@/store';

const logText = 'From PlexLibraryAPI => ';
const apiPath = '/plexLibrary';

export async function getPlexLibraryAsync(libraryId: number, plexAccountId: number): Promise<IPlexLibrary> {
	return await GlobalStore.Axios.get(`${apiPath}/${libraryId}?plexAccountId=${plexAccountId}`)
		.then((x) => {
			Log.debug(logText + 'getPlexLibrary response: ', x.data);
			return x.data;
		})
		.catch((e) => {
			Log.error(logText + 'getPlexLibrary error: ', e);
			return e.response.status;
		});
}

export async function refreshPlexLibraryAsync(libraryId: number, accountId: number): Promise<IPlexLibrary> {
	return await GlobalStore.Axios.post(`${apiPath}/refresh/`, {
		plexAccountId: accountId,
		plexLibraryId: libraryId,
	})
		.then((x) => {
			Log.debug(logText + 'refreshPlexLibraryAsync response: ', x.data);
			return x.data;
		})
		.catch((e) => {
			Log.error(logText + 'refreshPlexLibraryAsync error: ', e);
			return e.response.status;
		});
}
