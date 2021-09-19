<template>
	<div>
		<v-tree-view-table
			:items="downloadRows"
			:headers="getHeaders"
			height-auto
			media-icons
			@action="tableAction"
			@selected="selectedAction"
		/>
	</div>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadStatus, DownloadTaskDTO, FileMergeProgress, PlexMediaType } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import ITreeViewTableHeader from '@vTreeViewTable/ITreeViewTableHeader';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';
import ButtonType from '@enums/buttonType';
import { DownloadService, ProgressService } from '@service';

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

	get flatDownloadRows(): DownloadTaskDTO[] {
		// Concat is to get all un-nested DownloadTasks such as those of type Movie
		return this.downloadRows
			.map((x) => x.children?.map((y) => y.children))
			.concat(this.downloadRows)
			.flat(3)
			.filter((x) => !!x);
	}

	availableActions(status: DownloadStatus): ButtonType[] {
		const availableActions: ButtonType[] = [ButtonType.Details];
		switch (status) {
			case DownloadStatus.Unknown:
				availableActions.push(ButtonType.Delete);
				break;
			case DownloadStatus.Initialized:
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

	tableAction(payload: { action: string; item: DownloadTaskDTO }) {
		Log.info('command', payload);
		this.$emit('action', payload);
	}

	selectedAction(selected: number[]) {
		// Convert downloadTask keys to downloadTask Ids
		const ids = this.flatDownloadRows.filter((x) => selected.includes(x.key)).map((x) => x.id);
		this.$emit('selected', ids);
	}

	mounted(): void {
		// Retrieve initial download list
		this.$subscribeTo(DownloadService.getDownloadList(this.serverId), (data: DownloadTaskDTO[]) => {
			if (data && data.length > 0) {
				for (const rootDownloadTask of data) {
					// For movies download tasks
					if (rootDownloadTask.mediaType === PlexMediaType.Movie) {
						rootDownloadTask.actions = this.availableActions(rootDownloadTask.status);
						rootDownloadTask.children = undefined;
					}

					// For tvShows download tasks
					if (rootDownloadTask.mediaType === PlexMediaType.TvShow) {
						if (rootDownloadTask.children && rootDownloadTask.children.length > 0) {
							rootDownloadTask?.children?.forEach((season) => {
								if (season.children && season.children.length > 0) {
									season.children?.forEach(() => {
										// this.mergeDownloadRow(episode);
									});
								} else {
									Log.warn(`Season: ${season.title} had no episodes`);
								}
							});
						}
					}
				}

				this.downloadRows = [...data] as DownloadTaskDTO[];
			} else {
				this.downloadRows = [];
			}
		});

		this.$subscribeTo(ProgressService.getFileMergeProgress(this.serverId), (x) => (this.fileMergeProgressList = x));
	}
}
</script>
