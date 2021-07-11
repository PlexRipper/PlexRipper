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
import { startWith, switchMap, take, tap } from 'rxjs/operators';
import { DownloadMediaDTO, DownloadStatus, DownloadTaskDTO, PlexMediaType } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import { BaseService, ProgressService, AccountService } from '@service';
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

	private getDownloadState(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(
			tap((state) => {
				Log.warn('Downloads', state?.downloads);
			}),
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

	public getDownloadList(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return combineLatest([
			this.getDownloadState(serverId),
			ProgressService.getDownloadTaskUpdateProgress(serverId).pipe(startWith([])),
			ProgressService.getFileMergeProgress(serverId).pipe(startWith([])),
		]).pipe(
			// Merge the baseDownload with the download progress and return the updated result
			switchMap(([baseDownloadRows, downloadProgressRows, fileMergeRows]) => {
				if (!downloadProgressRows || downloadProgressRows === []) {
					return of(baseDownloadRows);
				}

				// Remove updates of finished download task updates
				// const x1 = downloadProgressRows.filter((x) => baseDownloadRows.find((y) => y.id === x.id));
				// const y1 = fileMergeRows.filter((x) => baseDownloadRows.find((y) => y.id === x.downloadTaskId));
				// this.setState({ downloadTaskUpdateList: x1, fileMergeProgressList: y1 }, 'CLEAN UP DOWNLOAD TASK UPDATE LIST', false);

				const mergedDownloadRows: DownloadTaskDTO[] = [];
				for (const baseDownloadRow of baseDownloadRows) {
					let mergedDownloadRow: DownloadTaskDTO = { ...baseDownloadRow };
					// Merge downloadProgress
					const downloadProgress = downloadProgressRows.find((x) => x.id === baseDownloadRow.id);
					if (downloadProgress) {
						mergedDownloadRow = { ...mergedDownloadRow, ...downloadProgress };
					}

					// Merge filemergeProgress
					if (mergedDownloadRow.status === DownloadStatus.Merging || mergedDownloadRow.status === DownloadStatus.Moving) {
						const fileMergeProgress = fileMergeRows.find((x) => x.downloadTaskId === baseDownloadRow.id);
						if (fileMergeProgress) {
							mergedDownloadRow = {
								...mergedDownloadRow,
								dataReceived: fileMergeProgress.dataTransferred,
								dataTotal: fileMergeProgress.dataTotal,
								percentage: fileMergeProgress.percentage,
								timeRemaining: fileMergeProgress.timeRemaining,
								downloadSpeed: fileMergeProgress.transferSpeed,
							};
						}
					}
					mergedDownloadRows.push(mergedDownloadRow);
				}

				return of(mergedDownloadRows);
			}),
		);
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
