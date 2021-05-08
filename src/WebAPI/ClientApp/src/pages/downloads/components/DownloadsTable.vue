<template>
	<div>
		{{ downloadRows }}
		{{ fileMergeProgressList }}
		<v-tree-view-table
			:items="downloadRows"
			:headers="getHeaders"
			height-auto
			media-icons
			@action="tableAction"
			@selected="$emit('selected', $event)"
		/>
	</div>
</template>
<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadClientUpdate, DownloadStatus, DownloadTaskDTO, FileMergeProgress, PlexMediaType } from '@dto/mainApi';
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

	fileMergeProgressList: FileMergeProgress[] = [];
	downloadRows: DownloadTaskDTO[] = [];

	mergeDownloadRow(downloadTask: DownloadTaskDTO): DownloadTaskDTO {
		// Merge the various feeds
		// const downloadProgress = this.downloadProgressList.find((x) => x.id === downloadTask.id);
		// Note: Need to create a new one, then add to array and then use that array to overwrite the season.children,
		// otherwise result will not be updated.

		downloadTask.actions = this.availableActions(downloadTask.status);

		// if (downloadProgress) {
		// 	downloadTask.percentage = downloadProgress.percentage;
		// 	downloadTask.downloadSpeed = downloadProgress.downloadSpeed;
		// 	downloadTask.dataReceived = downloadProgress.dataReceived;
		// 	downloadTask.dataTotal = downloadProgress.dataTotal;
		// 	downloadTask.timeRemaining = downloadProgress.timeRemaining;
		// }

		if (downloadTask.status === DownloadStatus.Merging) {
			const fileMergeProgress = this.fileMergeProgressList.find((x) => x.downloadTaskId === downloadTask.id);
			downloadTask.percentage = fileMergeProgress?.percentage ?? 0;
			downloadTask.dataReceived = fileMergeProgress?.dataTransferred ?? 0;
			downloadTask.timeRemaining = fileMergeProgress?.timeRemaining ?? 0;
			downloadTask.downloadSpeed = fileMergeProgress?.transferSpeed ?? 0;
		}

		if (downloadTask.status === DownloadStatus.Completed) {
			downloadTask.percentage = 100;
			downloadTask.timeRemaining = 0;
			downloadTask.downloadSpeed = 0;
			downloadTask.dataReceived = downloadTask.dataTotal;
			this.cleanupProgress(downloadTask.id);
		}

		return downloadTask;
	}

	get getDownloads(): DownloadTaskDTO[] | DownloadClientUpdate[] {
		this.downloadRows.forEach((rootDownloadTask) => {
			// For movies download tasks
			if (rootDownloadTask.mediaType === PlexMediaType.Movie) {
				this.mergeDownloadRow(rootDownloadTask);
				rootDownloadTask.actions = this.availableActions(rootDownloadTask.status);
				rootDownloadTask.children = undefined;
			}

			// For tvShows download tasks
			if (rootDownloadTask.mediaType === PlexMediaType.TvShow) {
				if (rootDownloadTask.children && rootDownloadTask.children.length > 0) {
					rootDownloadTask?.children?.forEach((season) => {
						if (season.children && season.children.length > 0) {
							season.children?.forEach((episode) => {
								this.mergeDownloadRow(episode);
							});
						} else {
							Log.warn(`Season: ${season.title} had no episodes`);
						}
					});
				}
			}
		});
		return this.downloadRows;
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
				availableActions.push(ButtonType.Restart);
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
		// const downloadProgressIndex = this.downloadProgressList.findIndex((x) => x.id === downloadTaskId);
		// if (downloadProgressIndex > -1) {
		// 	this.downloadProgressList.splice(downloadProgressIndex, 1);
		// }

		const fileMergeProgressIndex = this.fileMergeProgressList.findIndex((x) => x.downloadTaskId === downloadTaskId);
		if (fileMergeProgressIndex > -1) {
			this.fileMergeProgressList.splice(fileMergeProgressIndex, 1);
		}
	}

	mounted(): void {
		SignalrService.getDownloadTaskUpdate()
			.pipe(filter((x) => x.plexServerId === this.serverId))
			.subscribe((downloadTaskUpdate) => {
				Log.info('downloadTaskUpdate', downloadTaskUpdate);

				const i = this.downloadRows.findIndex((x) => x.id === downloadTaskUpdate.id);
				if (i > -1) {
					// Update
					this.downloadRows.splice(i, 1, downloadTaskUpdate);
				} else {
					// Add
					this.downloadRows.push(downloadTaskUpdate);
				}
				//
				// const j = this.downloadProgressList.findIndex((x) => x.id === downloadProgress.id);
				// if (j > -1) {
				// 	// Update
				// 	this.downloadProgressList.splice(i, 1, downloadProgress);
				// } else {
				// 	// Add
				// 	this.downloadProgressList.push(downloadProgress);
				// }
				//
				this.downloadRows.forEach((rootDownloadTask) => {
					// For movies download tasks
					if (rootDownloadTask.mediaType === PlexMediaType.Movie) {
						this.mergeDownloadRow(rootDownloadTask);
						rootDownloadTask.actions = this.availableActions(rootDownloadTask.status);
						rootDownloadTask.children = undefined;
					}

					// For tvShows download tasks
					if (rootDownloadTask.mediaType === PlexMediaType.TvShow) {
						if (rootDownloadTask.children && rootDownloadTask.children.length > 0) {
							rootDownloadTask?.children?.forEach((season) => {
								if (season.children && season.children.length > 0) {
									season.children?.forEach((episode) => {
										this.mergeDownloadRow(episode);
									});
								} else {
									Log.warn(`Season: ${season.title} had no episodes`);
								}
							});
						}
					}
				});
				Log.info('downloadRows', this.downloadRows);
			});

		// this.$subscribeTo(SignalrService.getDownloadProgress().pipe(filter((x) => x.plexServerId === this.serverId)), (data) => {
		// 	if (data) {
		// 		const i = this.downloadProgressList.findIndex((x) => x.id === data.id);
		// 		if (i > -1) {
		// 			// Update
		// 			this.downloadProgressList.splice(i, 1, data);
		// 		} else {
		// 			// Add
		// 			this.downloadProgressList.push(data);
		// 		}
		// 	} else {
		// 		Log.error(`DownloadProgress was undefined.`);
		// 	}
		// });
		//
		// // Retrieve download status from SignalR
		// this.$subscribeTo(SignalrService.getDownloadTaskUpdate().pipe(filter((x) => x.plexServerId === this.serverId)), (data) => {
		// 	const i = this.downloadRows.findIndex((x) => x.id === data.id);
		// 	if (i > -1) {
		// 		// Update
		// 		this.downloadRows.splice(i, 1, data);
		// 	} else {
		// 		// Add
		// 		this.downloadRows.push(data);
		// 	}
		// });

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
					data.forEach((x) => {
						if (x.children === null) {
							x.children = [];
						}
					});
					this.downloadRows = data as DownloadTaskDTO[];
				}
			},
		);
	}
}
</script>
