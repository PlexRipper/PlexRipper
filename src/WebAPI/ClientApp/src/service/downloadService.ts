import Log from 'consola';
import { combineLatest, Observable, of } from 'rxjs';
import {
	clearDownloadTasks,
	deleteDownloadTasks,
	downloadMedia,
	getAllDownloads,
	pauseDownloadTask,
	restartDownloadTasks,
	startDownloadTask,
	stopDownloadTasks,
} from '@api/plexDownloadApi';
import { map, switchMap, take } from 'rxjs/operators';
import { DownloadMediaDTO, DownloadStatus, DownloadTaskDTO, PlexMediaType } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import { AccountService, BaseService, ProgressService } from '@service';
import { Context } from '@nuxt/types';
import _ from 'lodash';

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
			.subscribe((downloads) => {
				if (downloads.isSuccess) {
					this.setState({ downloads: downloads.value }, 'Initial DownloadTask Data');
				}
			});
	}

	private getDownloadState(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(
			switchMap((state: IStoreState) => {
				// Only return the filtered array by serverId, 0 is all
				if (state?.downloads) {
					if (serverId > 0) {
						return of(state.downloads.filter((x) => x.plexServerId === serverId));
					}
					return of(state.downloads);
				}
				return of([]);
			}),
		);
	}

	public getActiveDownloadList(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return this.getDownloadList(serverId).pipe(
			map((x) =>
				x.filter(
					(y) =>
						y.status === DownloadStatus.Downloading ||
						y.status === DownloadStatus.Moving ||
						y.status === DownloadStatus.Merging ||
						y.status === DownloadStatus.Paused,
				),
			),
		);
	}

	public getDownloadList(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return combineLatest([this.getDownloadState(serverId), ProgressService.getMergedDownloadTaskProgress(serverId)]).pipe(
			// Merge the baseDownload with the download progress and return the updated result
			switchMap(([baseDownloadRows, downloadProgressRows]) => {
				if (baseDownloadRows?.length === 0 && downloadProgressRows?.length === 0) {
					return of([]);
				}

				if (baseDownloadRows?.length > 0 && downloadProgressRows?.length === 0) {
					return of(baseDownloadRows);
				}

				return _.union([baseDownloadRows, downloadProgressRows]);
			}),
		);
	}

	/**
	 * Fetch the download list and signal to the observers that it is done.
	 */
	public fetchDownloadList(): Observable<DownloadTaskDTO[]> {
		return getAllDownloads().pipe(
			switchMap((downloads): Observable<DownloadTaskDTO[]> => {
				Log.debug('Fetching download list');
				if (downloads.isSuccess) {
					this.setState({ downloads: downloads.value, downloadTaskUpdateList: [] });
				}
				return of(downloads.value ?? []);
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

	public clearDownloadTasks(downloadTaskIds: number[] = []): void {
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
