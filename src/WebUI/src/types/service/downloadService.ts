import { ReplaySubject, Observable } from 'rxjs';
import { getAllDownloads } from '@api/plexDownloadApi';
import GlobalService from '@service/globalService';
import Log from 'consola';
import IDownloadTask from '../dto/IDownloadTask';

export class DownloadService {
	private _downloadList: ReplaySubject<IDownloadTask[]> = new ReplaySubject();

	public constructor() {
		GlobalService.getAxiosReady().subscribe(() => {
			Log.debug('Retrieving downloadlist');
			this.fetchDownloadList();
		});
	}

	public getDownloadList(): Observable<IDownloadTask[]> {
		return this._downloadList.asObservable();
	}

	public fetchDownloadList(): void {
		getAllDownloads().subscribe((value) => this._downloadList.next(value ?? []));
	}
}

const downloadService = new DownloadService();
export default downloadService;
