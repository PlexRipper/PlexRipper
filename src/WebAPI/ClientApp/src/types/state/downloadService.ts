import Log from 'consola';
import { combineLatest, Observable, of } from 'rxjs';
import {
	clearDownloadTasks,
	deleteDownloadTasks,
	downloadMedia,
	getAllDownloads,
	getDownloadTasksInServer,
	pauseDownloadTask,
	restartDownloadTasks,
	startDownloadTask,
	stopDownloadTasks,
} from '@api/plexDownloadApi';
import { startWith, switchMap, take, tap } from 'rxjs/operators';
import { DownloadMediaDTO, DownloadTaskDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import AccountService from '@service/accountService';
import { BaseService } from '@state/baseService';
import ProgressService from '@state/progressService';
import { Context } from '@nuxt/types';

export class DownloadService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					downloads: state.downloads,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		AccountService.getActiveAccount()
			.pipe(
				switchMap(() => getAllDownloads()),
				take(1),
			)
			.subscribe((downloads: DownloadTaskDTO[]) => {
				if (downloads) {
					this.setState({ downloads }, 'Initial DownloadTask Data');
				}
			});
	}

	public getDownloadList(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return combineLatest([
			this.stateChanged.pipe(switchMap((state: IStoreState) => of(state?.downloads ?? []))),
			ProgressService.getDownloadTaskUpdateProgress(serverId).pipe(startWith([])),
		]).pipe(
			// Merge the baseDownload with the download progress and return the updated result
			switchMap(([baseDownloadRows, downloadProgressRows]) => {
				Log.warn('TESTESTERSFG', [baseDownloadRows, downloadProgressRows]);
				if (!downloadProgressRows || downloadProgressRows === []) {
					return of(baseDownloadRows);
				}

				// Remove updates of finished download task updates
				const x = downloadProgressRows.filter((x) => baseDownloadRows.findIndex((y) => y.id === x.id) >= 0);
				this.setState({ downloadTaskUpdateList: x }, 'CLEAN UP DOWNLOAD TASK UPDATE LIST', false);

				// Replace by progress if found
				for (let i = 0; i < baseDownloadRows.length; i++) {
					const index = downloadProgressRows.findIndex((x) => x.id === baseDownloadRows[i].id);
					if (index > -1) {
						baseDownloadRows[i] = { ...baseDownloadRows[i], ...downloadProgressRows[index] };
						Log.warn('MERGED RESULT', baseDownloadRows[i]);
					}
				}

				// Fix for Vuetify, the "v-treeview" component freaks out if children are null/undefined
				baseDownloadRows.forEach((x) => {
					if (x.children === null) {
						x.children = [];
					}
				});

				return of(baseDownloadRows);
			}),
		);
	}

	/**
	 * returns the downloadTasks nested in PlexServerDTO -> PlexLibraryDTO -> DownloadTaskDTO[]
	 */
	public getDownloadListInServers(): Observable<PlexServerDTO[]> {
		return getDownloadTasksInServer();
	}

	/**
	 * Fetch the download list and signal to the observers that it is done.
	 */
	public fetchDownloadList(): Observable<DownloadTaskDTO[]> {
		return getAllDownloads().pipe(
			tap((downloads) => {
				Log.debug('Fetching download list');
				this.setState({ downloads, downloadTaskUpdateList: [] });
			}),
		);
	}

	public downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): void {
		downloadMedia(downloadMediaCommand)
			.pipe(switchMap(() => this.fetchDownloadList()))
			.subscribe();
	}

	// region Commands

	public startDownloadTasks(downloadTaskIds: number[]): void {
		startDownloadTask(downloadTaskIds)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public restartDownloadTasks(downloadTaskIds: number[]): void {
		restartDownloadTasks(downloadTaskIds)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public deleteDownloadTasks(downloadTaskIds: number[]): void {
		this.removeDownloadTasks(downloadTaskIds);
		deleteDownloadTasks(downloadTaskIds)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public pauseDownloadTasks(downloadTaskIds: number[]): void {
		pauseDownloadTask(downloadTaskIds)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public stopDownloadTasks(downloadTaskIds: number[]): void {
		stopDownloadTasks(downloadTaskIds)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public clearDownloadTasks(downloadTaskIds: number[]): void {
		this.removeDownloadTasks(downloadTaskIds);
		clearDownloadTasks(downloadTaskIds)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}
	// endregion

	private removeDownloadTasks(downloadTaskIds: number[]): void {
		const downloads = this.getState().downloads;

		downloads.forEach((downloadTask) => {
			if (downloadTask.mediaType === PlexMediaType.TvShow) {
				downloadTask.children?.forEach((season) => {
					if (downloadTask.mediaType === PlexMediaType.Season) {
						season.children = season.children?.filter((x) => downloadTaskIds.some((y) => y !== x.id)) ?? [];
					}
				});
			}
		});
		this.setState({ downloads });
	}
}

const downloadService = new DownloadService();
export default downloadService;
