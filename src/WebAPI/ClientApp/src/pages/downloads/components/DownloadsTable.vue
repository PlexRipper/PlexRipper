<template>
	<v-tree-view-table :items="getDownloads" :headers="getHeaders" media-icons @action="tableAction" />
</template>
<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadProgress, DownloadStatus, DownloadStatusChanged, DownloadTaskDTO, FileMergeProgress } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ITreeViewTableHeader from '@vTreeViewTable/ITreeViewTableHeader';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';
import SignalrService from '@service/signalrService';
import { filter, switchMap } from 'rxjs/operators';
import ButtonType from '@enums/buttonType';
import DownloadService from '@state/downloadService';
import { of } from 'rxjs';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class DownloadsTable extends Vue {
	@Prop({ type: Boolean })
	readonly loading: Boolean = false;

	@Prop({ required: true, type: Array as () => DownloadTaskDTO[] })
	readonly value!: DownloadTaskDTO[];

	@Prop({ required: true, type: Number })
	readonly serverId!: number;

	downloadProgressList: DownloadProgress[] = [];
	fileMergeProgressList: FileMergeProgress[] = [];
	downloadStatusList: DownloadStatusChanged[] = [];
	tvShowsDownloadRows: DownloadTaskDTO[] = [];

	get getDownloads(): DownloadTaskDTO[] {
		this.tvShowsDownloadRows.forEach((tvShow) => {
			tvShow?.children?.forEach((season) => {
				if (season.children && season.children.length > 0) {
					season.children?.forEach((episode) => {
						// Merge the various feeds
						const downloadProgress = this.downloadProgressList.find((x) => x.id === episode.id);
						const downloadStatusUpdate = this.downloadStatusList.find((x) => x.id === episode.id);
						// Note: Need to create a new one, then add to array and then use that array to overwrite the season.children,
						// otherwise result will not be updated.

						// Status priority: downloadStatusUpdate > getDownloadList
						const newStatus = downloadStatusUpdate?.status ?? episode?.status ?? DownloadStatus.Unknown;
						if (newStatus !== episode.status) {
							episode.status = newStatus;
						}
						episode.actions = this.availableActions(newStatus);

						if (downloadProgress) {
							episode.percentage = downloadProgress.percentage;
							episode.downloadSpeed = downloadProgress.downloadSpeed;
							episode.dataReceived = downloadProgress.dataReceived;
							episode.dataTotal = downloadProgress.dataTotal;
							episode.timeRemaining = downloadProgress.timeRemaining;
						}

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
				type: TreeViewTableHeaderEnum.Duration,
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

	tableAction({ action, item }: { action: string; item: any }) {
		Log.info('command', { action, item });
		this.$emit(action, item);
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

	mounted(): void {
		this.$subscribeTo(SignalrService.getDownloadProgress().pipe(filter((x) => x.plexServerId === this.serverId)), (data) => {
			if (data) {
				const i = this.downloadProgressList.findIndex((x) => x.id === data.id);
				if (i > -1) {
					// Update
					this.downloadProgressList.splice(i, 1, data);
				} else {
					// Add
					this.downloadProgressList.push(data);
				}
			} else {
				Log.error(`DownloadProgress was undefined.`);
			}
		});

		// Retrieve download status from SignalR
		this.$subscribeTo(SignalrService.getDownloadStatus().pipe(filter((x) => x.plexServerId === this.serverId)), (data) => {
			if (data) {
				const i = this.downloadStatusList.findIndex((x) => x.id === data.id);
				if (i > -1) {
					// Update
					this.downloadStatusList.splice(i, 1, data);
				} else {
					// Add
					this.downloadStatusList.push(data);
				}
			} else {
				Log.error(`DownloadStatusChanged was undefined.`);
			}
		});

		this.$subscribeTo(
			SignalrService.getFileMergeProgress().pipe(filter((x) => x.plexServerId === this.serverId)),
			(fileMergeProgress) => {
				if (fileMergeProgress) {
					const i = this.fileMergeProgressList.findIndex((x) => x.id === fileMergeProgress.id);
					if (i > -1) {
						// Update
						this.fileMergeProgressList.splice(i, 1, fileMergeProgress);
					} else {
						// Add
						this.fileMergeProgressList.push(fileMergeProgress);
					}
				} else {
					Log.error(`FileMergeProgress was undefined`);
				}
			},
		);

		// Retrieve download list
		this.$subscribeTo(
			DownloadService.getDownloadList().pipe(switchMap((x) => of(x?.filter((x) => x.plexServerId === this.serverId) ?? []))),
			(data: DownloadTaskDTO[]) => {
				if (data) {
					this.tvShowsDownloadRows = data as DownloadTaskDTO[];
				}
			},
		);
	}
}
</script>
