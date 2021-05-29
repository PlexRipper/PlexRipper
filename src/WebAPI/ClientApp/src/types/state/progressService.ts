import Log from 'consola';
import { Context } from '@nuxt/types';
import { Observable, of } from 'rxjs';
import { filter, switchMap } from 'rxjs/operators';
import { DownloadTaskDTO, FileMergeProgress } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import { BaseService } from '@state/baseService';
import SignalrService from '@service/signalrService';

export class ProgressService extends BaseService {
	public constructor() {
		super({
			stateSliceSelector: (state: IStoreState) => {
				return {
					fileMergeProgressList: state.fileMergeProgressList,
					downloadTaskUpdateList: state.downloadTaskUpdateList,
				};
			},
		});
	}

	public setup(nuxtContext: Context): void {
		super.setup(nuxtContext);

		SignalrService.getFileMergeProgress().subscribe((fileMergeProgress) => {
			if (fileMergeProgress) {
				const { fileMergeProgressList } = this.getState();
				const i = fileMergeProgressList.findIndex((x) => x.id === fileMergeProgress.id);
				if (i > -1) {
					// Update entry
					fileMergeProgressList.splice(i, 1, fileMergeProgress);
				} else {
					// Add new entry
					fileMergeProgressList.push(fileMergeProgress);
				}
				this.setState({ fileMergeProgressList });
			} else {
				Log.error(`FileMergeProgress was ${fileMergeProgress}`);
			}
		});

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

	public cleanUpProgressByDownloadTaskId(downloadTaskId: number): void {
		const { fileMergeProgressList } = this.getState();
		const fileMergeProgressIndex = fileMergeProgressList.findIndex((x) => x.downloadTaskId === downloadTaskId);
		if (fileMergeProgressIndex > -1) {
			fileMergeProgressList.splice(fileMergeProgressIndex, 1);
			this.setState({ fileMergeProgressList });
		}

		const { downloadTaskUpdateList } = this.getState();
		const downloadTaskUpdateListIndex = downloadTaskUpdateList.findIndex((x) => x.id === downloadTaskId);
		if (downloadTaskUpdateListIndex > -1) {
			downloadTaskUpdateList.splice(downloadTaskUpdateListIndex, 1);
			this.setState({ downloadTaskUpdateList });
		}
	}
}

const progressService = new ProgressService();
export default progressService;
