import Log from 'consola';
import { combineLatest, merge, Observable, of } from 'rxjs';
import {
	clearDownloadTasks,
	deleteDownloadTasks,
	downloadMedia,
	getAllDownloads,
	getDownloadTasksInServer,
	restartDownloadTasks,
	stopDownloadTasks,
} from '@api/plexDownloadApi';
import { finalize, startWith, switchMap } from 'rxjs/operators';
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
			.pipe(switchMap(() => getAllDownloads()))
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
				// Replace by progress if found
				for (let i = 0; i < baseDownloadRows.length; i++) {
					const index = downloadProgressRows.findIndex((x) => x.id === baseDownloadRows[i].id);
					if (index > -1) {
						baseDownloadRows[i] = downloadProgressRows[index];
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
	public fetchDownloadList(): void {
		getAllDownloads().subscribe((downloads) => this.setState({ downloads }));
	}

	public downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): void {
		downloadMedia(downloadMediaCommand)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	// region Commands

	public restartDownloadTasks(downloadTaskIds: number[]): void {
		restartDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	public deleteDownloadTasks(downloadTaskIds: number[]): void {
		this.removeDownloadTasks(downloadTaskIds);
		deleteDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	public clearDownloadTasks(downloadTaskIds: number[]): void {
		this.removeDownloadTasks(downloadTaskIds);
		clearDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	public stopDownloadTasks(downloadTaskIds: number[]): void {
		stopDownloadTasks(downloadTaskIds)
			.pipe(finalize(() => this.fetchDownloadList()))
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
