import { Observable, of } from 'rxjs';
import {
	clearDownloadTasks,
	deleteDownloadTasks,
	downloadMedia,
	getAllDownloads,
	getDownloadTasksInServer,
	stopDownloadTasks,
} from '@api/plexDownloadApi';
import { finalize, switchMap } from 'rxjs/operators';
import { DownloadMediaDTO, DownloadTaskDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import StoreState from '@state/storeState';
import AccountService from '@service/accountService';
import { BaseService } from '@state/baseService';

export class DownloadService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: StoreState) => {
				return {
					downloads: state.downloads,
				};
			},
		});

		AccountService.getActiveAccount()
			.pipe(switchMap(() => getAllDownloads()))
			.subscribe((downloads: DownloadTaskDTO[]) => {
				if (downloads) {
					this.setState({ downloads }, 'Initial DownloadTask Data');
				}
			});
	}

	public getDownloadList(): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(switchMap((state: StoreState) => of(state?.downloads)));
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
		getAllDownloads().subscribe((downloads) => this.setState({ downloads }));
		return this.getDownloadList();
	}

	public downloadMedia(downloadMediaCommand: DownloadMediaDTO[]): void {
		downloadMedia(downloadMediaCommand)
			.pipe(finalize(() => this.fetchDownloadList()))
			.subscribe();
	}

	// region Commands

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
