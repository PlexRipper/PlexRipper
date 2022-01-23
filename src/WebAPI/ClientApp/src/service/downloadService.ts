import Log from 'consola';
import { Observable, of} from 'rxjs';
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
import { DownloadMediaDTO, DownloadProgressDTO, DownloadStatus, PlexMediaType, ServerDownloadProgressDTO } from '@dto/mainApi';
import IStoreState from '@interfaces/service/IStoreState';
import { BaseService, GlobalService, SignalrService } from '@service';
import { Context } from '@nuxt/types';

export class DownloadService extends BaseService {
	public constructor() {
		super({
			// Note: Each service file can only have "unique" state slices which are not also used in other service files
			stateSliceSelector: (state: IStoreState) => {
				return {
					serverDownloads: state.serverDownloads,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		GlobalService.getAxiosReady()
			.pipe(
				switchMap(() => getAllDownloads()),
				take(1),
			)
			.subscribe((downloads) => {
				if (downloads.isSuccess) {
					this.setState({ serverDownloads: downloads.value }, 'Initial DownloadTask Data');
				}
			});

		SignalrService.GetServerDownloadProgress().subscribe((data: ServerDownloadProgressDTO) => {
			this.updateStore('serverDownloads', data);
		});
	}

	public getActiveDownloadList(serverId: number = 0): Observable<DownloadProgressDTO[]> {
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

	public getServerDownloadList(): Observable<ServerDownloadProgressDTO[]> {
		return this.stateChanged.pipe(map((state) => state?.serverDownloads ?? []));
	}

	public getDownloadList(serverId: number = 0): Observable<DownloadProgressDTO[]> {
		if (serverId > 0) {
			return this.getServerDownloadList().pipe(map((x) => x.find((y) => y.id === serverId)?.downloads ?? []));
		}
		return this.getServerDownloadList().pipe(map((x) => x.map((y) => y.downloads).flat() ?? []));
	}

	/**
	 * Fetch the download list and signal to the observers that it is done.
	 */
	public fetchDownloadList(): Observable<ServerDownloadProgressDTO[]> {
		return getAllDownloads().pipe(
			switchMap((downloads) => {
				Log.debug('Fetching download list');
				if (downloads.isSuccess) {
					this.setState({ serverDownloads: downloads.value });
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

	public startDownloadTasks(downloadTaskId: number): void {
		startDownloadTask(downloadTaskId)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public restartDownloadTasks(downloadTaskId: number): void {
		restartDownloadTasks(downloadTaskId)
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

	public pauseDownloadTasks(downloadTaskId: number): void {
		pauseDownloadTask(downloadTaskId)
			.pipe(
				switchMap(() => this.fetchDownloadList()),
				take(1),
			)
			.subscribe();
	}

	public stopDownloadTasks(downloadTaskId: number): void {
		stopDownloadTasks(downloadTaskId)
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
		const downloads = this.getState().serverDownloads;

		downloads.forEach((serverDownload) => {
			serverDownload.downloads.forEach((downloadTask) => {
				if (downloadTask.mediaType === PlexMediaType.TvShow) {
					downloadTask.children?.forEach((season) => {
						if (downloadTask.mediaType === PlexMediaType.Season) {
							season.children = season.children?.filter((x) => downloadTaskIds.some((y) => y !== x.id)) ?? [];
						}
					});
				}
			});
		});
		this.setState({ serverDownloads: downloads });
	}
}

const downloadService = new DownloadService();
export default downloadService;
