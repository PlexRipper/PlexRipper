<template>
	<v-tree-view-table :items="getDownloads" :headers="getHeaders" @action="tableAction" />
</template>
<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import {
	DownloadProgress,
	DownloadStatus,
	DownloadStatusChanged,
	DownloadTaskContainerDTO,
	FileMergeProgress,
} from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ITreeViewTableHeader from '@vTreeViewTable/ITreeViewTableHeader';
import Convert from '@mediaOverview/MediaTable/types/Convert';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';
import SignalrService from '@service/signalrService';
import Log from 'consola';
import { filter, switchMap } from 'rxjs/operators';
import ButtonType from '@enums/buttonType';
import DownloadService from '@state/downloadService';
import { of } from 'rxjs';
import IDownloadRow from '../types/IDownloadRow';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class DownloadsTable extends Vue {
	@Prop({ type: Boolean })
	readonly loading: Boolean = false;

	@Prop({ required: true, type: Array as () => IDownloadRow[] })
	readonly value!: IDownloadRow[];

	@Prop({ required: true, type: Number })
	readonly serverId!: number;

	downloadProgressList: DownloadProgress[] = [];
	fileMergeProgressList: FileMergeProgress[] = [];
	downloadStatusList: DownloadStatusChanged[] = [];
	tvShowsDownloadRows: IDownloadRow[] = [];

	get getDownloads(): IDownloadRow[] {
		this.tvShowsDownloadRows.forEach((tvShow) => {
			tvShow.actions = this.availableActions(tvShow.status);
			tvShow?.children?.forEach((season) => {
				season.actions = this.availableActions(season.status);
				if (season.children.length > 0) {
					const updatedEpisodes: IDownloadRow[] = [];
					season.children.forEach((episode) => {
						const downloadProgress = this.downloadProgressList.find((x) => x.id === episode.id);
						const downloadStatusUpdate = this.downloadStatusList.find((x) => x.id === episode.id);
						// Merge the various feeds
						// Status priority: downloadStatusUpdate > getDownloadList
						const status = downloadStatusUpdate?.status ?? episode?.status ?? DownloadStatus.Unknown;
						// Note: Need to create a new one, then add to array and then use that array to overwrite the season.children,
						// otherwise result will not be updated.
						const downloadRow: IDownloadRow = {
							...episode,
							...downloadProgress,
							status,
							actions: this.availableActions(status),
						};

						if (downloadRow.status === DownloadStatus.Merging) {
							const fileMergeProgress = this.fileMergeProgressList.find((x) => x.downloadTaskId === downloadRow.id);
							downloadRow.percentage = fileMergeProgress?.percentage ?? 0;
							downloadRow.dataReceived = fileMergeProgress?.dataTransferred ?? 0;
							downloadRow.timeRemaining = fileMergeProgress?.timeRemaining ?? 0;
							downloadRow.downloadSpeed = fileMergeProgress?.transferSpeed ?? 0;
						}

						if (downloadRow.status === DownloadStatus.Completed) {
							downloadRow.percentage = 100;
							downloadRow.timeRemaining = 0;
							downloadRow.downloadSpeed = 0;
							downloadRow.dataReceived = episode.dataTotal;
							this.cleanupProgress(episode.id);
						}

						updatedEpisodes.push(downloadRow);
					});
					season.children = updatedEpisodes;
				} else {
					Log.warn(`Season: ${season.title} had no episodes`);
				}
			});
		});
		Log.info('tvShows', this.tvShowsDownloadRows);
		return this.tvShowsDownloadRows;
	}

	get getHeaders(): ITreeViewTableHeader[] {
		return [
			// {
			// 	text: 'Id',
			// 	value: 'id',
			// },
			{
				text: 'Title',
				value: 'title',
				maxWidth: 250,
			},
			{
				text: 'Status',
				value: 'status',
				width: 120,
			},
			{
				text: 'Received',
				value: 'dataReceived',
				type: TreeViewTableHeaderEnum.FileSize,
				width: 120,
			},
			{
				text: 'Size',
				value: 'dataTotal',
				type: TreeViewTableHeaderEnum.FileSize,
				width: 120,
			},
			{
				text: 'Speed',
				value: 'downloadSpeed',
				type: TreeViewTableHeaderEnum.FileSpeed,
				width: 120,
			},
			{
				text: 'ETA',
				value: 'timeRemaining',
				width: 120,
			},
			{
				text: 'Percentage',
				value: 'percentage',
				type: TreeViewTableHeaderEnum.Percentage,
				width: 120,
			},
			{
				text: 'Actions',
				value: 'actions',
				type: TreeViewTableHeaderEnum.Actions,
				width: 160,
				sortable: false,
			},
		];
	}

	formatCountdown(seconds: number): string {
		if (!seconds || seconds <= 0) {
			return '0:00';
		}
		return new Date(seconds * 1000)?.toISOString()?.substr(11, 8)?.toString() ?? '?';
	}

	availableActions(status: DownloadStatus): ButtonType[] {
		const availableActions: ButtonType[] = [ButtonType.Details];
		switch (status) {
			case DownloadStatus.Initialized:
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Starting:
				availableActions.push(ButtonType.Stop);
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Queued:
				availableActions.push(ButtonType.Start);
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Downloading:
				availableActions.push(ButtonType.Pause);
				availableActions.push(ButtonType.Stop);
				break;
			case DownloadStatus.Paused:
				availableActions.push(ButtonType.Start);
				availableActions.push(ButtonType.Stop);
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Completed:
				availableActions.push(ButtonType.Clear);
				break;
			case DownloadStatus.Stopping:
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Stopped:
				availableActions.push(ButtonType.Restart);
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Merging:
				break;
			case DownloadStatus.Error:
				availableActions.push(ButtonType.Restart);
				availableActions.push(ButtonType.Delete);
				break;
		}
		return availableActions;
	}

	tableAction({ action, payload }: { action: string; payload: any }) {
		Log.info('command', { action, payload });
		this.$emit(action, payload);
	}

	updateDownloadProgress(downloadProgress: DownloadProgress): void {
		// Check if there is already a progress object for this Id
		const i = this.downloadProgressList.findIndex((x) => x.id === downloadProgress.id);
		if (i > -1) {
			// Update
			this.downloadProgressList.splice(i, 1, downloadProgress);
		} else {
			// Add
			this.downloadProgressList.push(downloadProgress);
		}
	}

	cleanupProgress(downloadTaskId: number): void {
		// Clean-up progress objects
		const downloadProgressIndex = this.downloadProgressList.findIndex((x) => x.id === downloadTaskId);
		if (downloadProgressIndex > -1) {
			this.downloadProgressList.splice(downloadProgressIndex, 1);
		}

		const fileMergeProgressIndex = this.fileMergeProgressList.findIndex((x) => x.downloadTaskId === downloadTaskId);
		if (fileMergeProgressIndex > -1) {
			this.fileMergeProgressList.splice(fileMergeProgressIndex, 1);
		}
	}

	created(): void {
		SignalrService.getDownloadProgress()
			.pipe(filter((x) => x.plexServerId === this.serverId))
			.subscribe((data) => {
				if (data) {
					this.updateDownloadProgress(data);
				} else {
					Log.error(`DownloadProgress was undefined.`);
				}
			});

		// Retrieve download status from SignalR
		SignalrService.getDownloadStatus()
			.pipe(filter((x) => x.plexServerId === this.serverId))
			.subscribe((data) => {
				if (data) {
					this.downloadStatusList.addOrReplace((x) => x.id === data.id, data);
				} else {
					Log.error(`DownloadStatusChanged was undefined.`);
				}
			});

		SignalrService.getFileMergeProgress().subscribe((fileMergeProgress) => {
			if (fileMergeProgress) {
				this.fileMergeProgressList.addOrReplace((x) => x.id === fileMergeProgress.id, fileMergeProgress);
			} else {
				Log.error(`FileMergeProgress was undefined`);
			}
		});

		// Retrieve download list
		DownloadService.getDownloadList()
			.pipe(
				switchMap((x) => {
					return of({
						tvShows: x?.tvShows.filter((x) => x.plexServerId === this.serverId) ?? [],
					} as DownloadTaskContainerDTO);
				}),
			)
			.subscribe((data: DownloadTaskContainerDTO) => {
				Log.info('getDownloadList', data);
				if (data) {
					this.tvShowsDownloadRows = Convert.DownloadTaskTvShowToTreeViewTableRows(data.tvShows);
				}
			});
	}
}
</script>
