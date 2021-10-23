import Log from 'consola';
import _ from 'lodash';
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
import IStoreState from '@interfaces/service/IStoreState';
import { AccountService, BaseService, ProgressService } from '@service';
import { Context } from '@nuxt/types';
import { determineDownloadStatus } from '@lib/common';
import globalService from '~/service/globalService';

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

		globalService
			.getAxiosReady()
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
			map((state) => state?.downloads.filter((x) => (serverId > 0 ? x.plexServerId === serverId : true)) ?? []),
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

	private mergeChildren(downloadTask: DownloadTaskDTO, downloadProgressRows: DownloadTaskDTO[]): DownloadTaskDTO {
		const progress = downloadProgressRows.find((x) => x.id === downloadTask.id);
		if (progress) {
			downloadTask = {
				...downloadTask,
				...progress,
				children: downloadTask.children,
			};
		}
		// Check if children have progress
		if (downloadTask.children) {
			for (let i = 0; i < downloadTask.children.length; i++) {
				downloadTask.children.splice(i, 1, this.mergeChildren(downloadTask.children[i], downloadProgressRows));
			}
		}

		return downloadTask;
	}

	private aggregateChildren(downloadTask: DownloadTaskDTO): DownloadTaskDTO {
		if (downloadTask.mediaType !== PlexMediaType.Season && downloadTask.mediaType !== PlexMediaType.TvShow) {
			return downloadTask;
		}
		if (!downloadTask.children || downloadTask.children.length === 0) {
			return downloadTask;
		}

		for (let i = 0; i < downloadTask.children.length; i++) {
			downloadTask.children.splice(i, 1, this.aggregateChildren(downloadTask.children[i]));
		}

		downloadTask.percentage = _.mean(downloadTask.children?.map((x) => x.percentage)) ?? 0;
		downloadTask.downloadSpeed = _.mean(downloadTask.children?.map((x) => x.downloadSpeed)) ?? 0;
		downloadTask.timeRemaining = _.sum(downloadTask.children?.map((x) => x.timeRemaining)) ?? 0;
		downloadTask.dataReceived = _.sum(downloadTask.children?.map((x) => x.dataReceived)) ?? 0;
		downloadTask.dataTotal = _.sum(downloadTask.children?.map((x) => x.dataTotal)) ?? 0;

		const statuses = downloadTask.children?.map((x) => x.status) ?? [];
		downloadTask.status = determineDownloadStatus(statuses);

		return downloadTask;
	}

	public getDownloadList(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return combineLatest([this.getDownloadState(serverId), ProgressService.getMergedDownloadTaskProgress(serverId)]).pipe(
			// Merge the baseDownload with the download progress and return the updated result
			switchMap(([baseDownloadRows, downloadProgressRows]) => {
				if (baseDownloadRows.length === 0 && downloadProgressRows.length === 0) {
					return of([]);
				}

				if (baseDownloadRows.length > 0 && downloadProgressRows.length === 0) {
					const mergedRows: DownloadTaskDTO[] = [];
					for (const baseDownloadRow of baseDownloadRows) {
						mergedRows.push(this.aggregateChildren(baseDownloadRow));
					}
					return of(mergedRows);
				}

				const mergedRows: DownloadTaskDTO[] = [...baseDownloadRows];

				for (let i = 0; i < mergedRows.length; i++) {
					let mergedDownloadTask = this.mergeChildren(mergedRows[i], downloadProgressRows);
					mergedDownloadTask = this.aggregateChildren(mergedDownloadTask);
					mergedRows.splice(i, 1, mergedDownloadTask);
				}

				return of(mergedRows);
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
