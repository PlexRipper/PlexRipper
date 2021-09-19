import { Context } from '@nuxt/types';
import { combineLatest, of } from 'rxjs';
import { startWith, switchMap } from 'rxjs/operators';
import { DownloadStatus, DownloadTaskDTO } from '@dto/mainApi';
import IStoreState from '@interfaces/IStoreState';
import { BaseService, SignalrService } from '@service';

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
	}

	public getMergedDownloadTaskProgress(serverId: number = 0) {
		return combineLatest([
			SignalrService.getAllDownloadTaskUpdate().pipe(startWith([])),
			SignalrService.getAllFileMergeProgress().pipe(startWith([])),
		]).pipe(
			switchMap(([downloadRows, fileMergeList]) => {
				if (serverId > 0) {
					downloadRows = downloadRows.filter((x) => x.plexServerId === serverId);
					fileMergeList = fileMergeList.filter((x) => x.plexServerId === serverId);
				}

				const mergedDownloadRows: DownloadTaskDTO[] = [];

				for (let i = 0; i < downloadRows.length; i++) {
					let mergedDownloadRow = {
						...downloadRows[i],
					};

					if (downloadRows[i].status !== DownloadStatus.Merging) {
						mergedDownloadRows.push(mergedDownloadRow);
						continue;
					}

					const fileMergeProgress = fileMergeList.find((x) => x.downloadTaskId === downloadRows[i].id);
					if (fileMergeProgress) {
						mergedDownloadRow = {
							...downloadRows[i],
							dataReceived: fileMergeProgress.dataTransferred,
							dataTotal: fileMergeProgress.dataTotal,
							percentage: fileMergeProgress.percentage,
							timeRemaining: fileMergeProgress.timeRemaining,
							downloadSpeed: fileMergeProgress.transferSpeed,
						};
					}
					mergedDownloadRows.push(mergedDownloadRow);
				}

				return of(mergedDownloadRows);
			}),
		);
	}
}

const progressService = new ProgressService();
export default progressService;
