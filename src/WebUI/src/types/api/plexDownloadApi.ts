import Log from 'consola';
import IPlexLibrary from '@dto/IPlexLibrary';
import IDownloadTask from '@dto/IDownloadTask';
import { GlobalStore } from '@/store';
import {} from '@aspnet/signalr';
const logText = 'From PlexLibraryAPI => ';
const apiPath = '/Download';

export async function downloadPlexMovie(movieId: number, plexAccountId: number): Promise<IPlexLibrary> {
	return await GlobalStore.Axios.get(`${apiPath}/movie/${movieId}?plexAccountId=${plexAccountId}`)
		.then((x) => {
			Log.debug(logText + 'downloadPlexMovie response: ', x.data);
			return x.data;
		})
		.catch((e) => {
			Log.error(logText + 'downloadPlexMovie error: ', e);
			return e.response.status;
		});
}

export async function getAllDownloads(): Promise<IDownloadTask[]> {
	return await GlobalStore.Axios.get(`${apiPath}/`)
		.then((x) => {
			Log.debug(logText + 'downloadPlexMovie response: ', x.data);
			return x.data;
		})
		.catch((e) => {
			Log.error(logText + 'downloadPlexMovie error: ', e);
			return e.response.status;
		});
}
