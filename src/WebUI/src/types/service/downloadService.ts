import { ReplaySubject, Observable } from 'rxjs';
import { getAllDownloads } from '@api/plexDownloadApi';
import { map } from 'rxjs/operators';
import GlobalService from '@service/globalService';
import Log from 'consola';
import { DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';

export class DownloadService {
	private _downloadServerList: ReplaySubject<PlexServerDTO[]> = new ReplaySubject();

	public constructor() {
		GlobalService.getAxiosReady().subscribe(() => {
			Log.debug('Retrieving downloadlist');
			this.fetchDownloadList();
		});
	}

	/**
	 * returns the downloadTasks nested in PlexServerDTO -> PlexLibraryDTO -> DownloadTaskDTO[]
	 */
	public getDownloadList(): Observable<DownloadTaskDTO[]> {
		return this._downloadServerList
			.asObservable()
			.pipe(map((value) => value.map((x) => x.plexLibraries.map((y) => y.downloadTasks)).flat(2)));
	}

	public getDownloadListInServers(): Observable<PlexServerDTO[]> {
		return this._downloadServerList.asObservable();
	}

	/**
	 * Fetch the download list and signal to the observers that it is done.
	 */
	public fetchDownloadList(): void {
		getAllDownloads().subscribe((value) => this._downloadServerList.next(value ?? []));
	}
}

const downloadService = new DownloadService();
export default downloadService;
