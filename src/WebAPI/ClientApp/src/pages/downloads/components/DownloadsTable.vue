<template>
	<v-tree-view-table :items="getDownloads" :headers="getHeaders" />
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
import { filter } from 'rxjs/operators';
import IDownloadRow from '../types/IDownloadRow';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class DownloadsTable extends Vue {
	@Prop({ required: true, type: Object as () => DownloadTaskContainerDTO })
	readonly downloads!: DownloadTaskContainerDTO;

	@Prop({ type: Boolean })
	readonly loading: Boolean = false;

	@Prop({ required: true, type: Array as () => IDownloadRow[] })
	readonly value!: IDownloadRow[];

	@Prop({ required: true, type: Number })
	readonly serverId!: number;

	downloadProgressList: DownloadProgress[] = [];
	fileMergeProgressList: FileMergeProgress[] = [];
	downloadStatusList: DownloadStatusChanged[] = [];

	get getDownloads(): IDownloadRow[] {
		const downloadContainer = Convert.DownloadTaskTvShowToTreeViewTableRows(this.downloads?.tvShows ?? []);

		downloadContainer.tvShows?.forEach((tvShow) => {
			tvShow.seasons?.forEach((season) => {
				season.episodes?.forEach((episode) => {
					const downloadProgress = this.downloadProgressList.find((x) => x.id === episode.id);
					const downloadStatusUpdate = this.downloadStatusList.find((x) => x.id === episode.id);
					// Merge the various feeds
					episode = {
						...episode,
						...downloadProgress,
						// Status priority: downloadStatusUpdate > getDownloadList
						status: downloadStatusUpdate?.status ?? DownloadStatus.Unknown,
					} as IDownloadRow;

					if (episode.status === DownloadStatus.Merging) {
						const fileMergeProgress = this.fileMergeProgressList.find((x) => x.downloadTaskId === episode.id);
						episode.percentage = fileMergeProgress?.percentage ?? 0;
						episode.dataReceived = fileMergeProgress?.dataTransferred ?? 0;
						episode.timeRemaining = fileMergeProgress?.timeRemaining ?? 0;
						episode.downloadSpeed = fileMergeProgress?.transferSpeed ?? 0;
					}

					if (episode.status === DownloadStatus.Completed) {
						episode.percentage = 100;
						episode.timeRemaining = 0;
						episode.downloadSpeed = 0;
						episode.dataReceived = episode.dataTotal;
						this.cleanupProgress(episode.id);
					}
				});
			});
		});
		return downloadContainer;
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
				actions: [
					{
						command: 'details',
					},
					{
						command: 'stop',
					},
					{
						command: 'delete',
					},
					{
						command: 'pause',
					},
					{
						command: 'stop',
					},
					{
						command: 'start',
					},
				],
				width: 120,
				sortable: false,
			},
		];
	}

	get getButtons(): any {
		return [
			{
				name: 'Restart',
				value: 'restart',
				icon: 'mdi-refresh',
			},
			{
				name: 'Start / Resume',
				value: 'start',
				icon: 'mdi-play',
			},
			{
				name: 'Pause',
				value: 'pause',
				icon: 'mdi-pause',
			},
			{
				name: 'Stop',
				value: 'stop',
				icon: 'mdi-stop',
			},
			{
				name: 'Delete',
				value: 'delete',
				icon: 'mdi-delete',
			},
			{
				name: 'Clear',
				value: 'clear',
				icon: 'mdi-notification-clear-all',
			},
			{
				name: 'Details',
				value: 'details',
				icon: 'mdi-chart-box-outline',
			},
		];
	}

	formatCountdown(seconds: number): string {
		if (!seconds || seconds <= 0) {
			return '0:00';
		}
		return new Date(seconds * 1000)?.toISOString()?.substr(11, 8)?.toString() ?? '?';
	}

	availableActions(item: IDownloadRow): string[] {
		const actions: string[] = ['details'];
		switch (item.status) {
			case DownloadStatus.Initialized:
				actions.push('delete');
				break;
			case DownloadStatus.Starting:
				actions.push('stop');
				actions.push('delete');
				break;
			case DownloadStatus.Queued:
				actions.push('start');
				actions.push('delete');
				break;
			case DownloadStatus.Downloading:
				actions.push('pause');
				actions.push('stop');
				break;
			case DownloadStatus.Paused:
				actions.push('start');
				actions.push('restart');
				actions.push('stop');
				break;
			case DownloadStatus.Completed:
				actions.push('clear');
				break;
			case DownloadStatus.Stopping:
				actions.push('delete');
				break;
			case DownloadStatus.Stopped:
				actions.push('restart');
				actions.push('delete');
				break;
			case DownloadStatus.Merging:
				break;
			case DownloadStatus.Error:
				actions.push('restart');
				actions.push('delete');
				break;
		}
		return actions;
	}

	command(action: string, itemId: number): void {
		this.$emit(action, itemId);
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
					this.downloadStatusList.addOrReplace((x) => x.id === data.id);
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
	}
}
</script>
