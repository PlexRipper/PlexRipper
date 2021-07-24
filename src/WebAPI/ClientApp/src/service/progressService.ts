import Log from 'consola';
import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { filter, map, switchMap } from 'rxjs/operators';
import { DownloadTaskDTO, FileMergeProgress, PlexAccountRefreshProgress } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import { BaseService, SignalrService } from '@service';

export class ProgressService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					fileMergeProgressList: state.fileMergeProgressList,
					downloadTaskUpdateList: state.downloadTaskUpdateList,
					plexAccountRefreshProgress: state.plexAccountRefreshProgress,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		// SignalrService.getFileMergeProgress().subscribe((fileMergeProgress) => {
		// 	if (fileMergeProgress) {
		// 		const { fileMergeProgressList } = this.getState();
		// 		const i = fileMergeProgressList.findIndex((x) => x.id === fileMergeProgress.id);
		// 		if (i > -1) {
		// 			// Update entry
		// 			fileMergeProgressList.splice(i, 1, fileMergeProgress);
		// 		} else {
		// 			// Add new entry
		// 			fileMergeProgressList.push(fileMergeProgress);
		// 		}
		// 		this.setState({ fileMergeProgressList });
		// 	} else {
		// 		Log.error(`FileMergeProgress was ${fileMergeProgress}`);
		// 	}
		// });

		SignalrService.getDownloadTaskUpdate().subscribe((downloadTaskUpdate) => {
			if (downloadTaskUpdate) {
				Log.debug('downloadTaskUpdate', downloadTaskUpdate);

				const { downloadTaskUpdateList } = this.getState();
				const i = downloadTaskUpdateList.findIndex((x) => x.id === downloadTaskUpdate.id);
				if (i > -1) {
					// Update entry
					downloadTaskUpdateList.splice(i, 1, downloadTaskUpdate);
				} else {
					// Add new entry
					downloadTaskUpdateList.push(downloadTaskUpdate);
				}
				this.setState({ downloadTaskUpdateList });
			} else {
				Log.error(`downloadTaskUpdate was ${downloadTaskUpdate}`);
			}
		});
	}

	/**
	 * Merge the latest fileMergeUpdates into an array to be queried for real-time updates.
	 */
	public getFileMergeProgress(serverId: number): Observable<FileMergeProgress[]> {
		return this.stateChanged.pipe(
			switchMap((state: IStoreState) => of(state?.fileMergeProgressList.filter((x) => x.plexServerId === serverId))),
			filter((x) => x && x !== []),
		);
	}

	/**
	 * Merge the latest fileMergeUpdates into an array to be queried for real-time updates.
	 */
	public getDownloadTaskUpdateProgress(serverId: number = 0): Observable<DownloadTaskDTO[]> {
		return this.stateChanged.pipe(
			switchMap((state: IStoreState) =>
				of(state?.downloadTaskUpdateList.filter((x) => (serverId > 0 ? x.plexServerId === serverId : true))),
			),
			filter((x) => x && x !== []),
		);
	}

	public getPlexAccountRefreshProgress(plexAccountId: number = 0): Observable<PlexAccountRefreshProgress | null> {
		return this.stateChanged.pipe(
			map((state: IStoreState) => state?.plexAccountRefreshProgress.find((x) => x.plexAccountId === plexAccountId) ?? null),
		);
	}
}

const progressService = new ProgressService();
export default progressService;
