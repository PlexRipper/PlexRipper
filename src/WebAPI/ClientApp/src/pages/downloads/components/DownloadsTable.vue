<template>
	<div>
		<v-tree-view-table
			:items="downloadRows"
			:headers="getHeaders"
			height-auto
			media-icons
			load-children
			item-key="id"
			@action="tableAction"
			@selected="selectedAction"
		/>
	</div>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadProgressDTO, DownloadTaskDTO, FileMergeProgress } from '@dto/mainApi';
import ITreeViewTableHeader from '@vTreeViewTable/ITreeViewTableHeader';
import TreeViewTableHeaderEnum from '@enums/treeViewTableHeaderEnum';

@Component
export default class DownloadsTable extends Vue {
	@Prop({ type: Boolean })
	readonly loading: Boolean = false;

	@Prop({ required: true, type: Array as () => DownloadTaskDTO[] })
	readonly value!: DownloadTaskDTO[];

	@Prop({ required: true, type: Number })
	readonly serverId!: number;

	fileMergeProgressList: FileMergeProgress[] = [];

	@Prop({ required: true, type: Array as () => DownloadProgressDTO[] })
	readonly downloadRows!: DownloadProgressDTO[];

	get getHeaders(): ITreeViewTableHeader[] {
		return [
			{
				text: 'Id',
				value: 'id',
				maxWidth: 50,
			},
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

	get flatDownloadRows(): DownloadProgressDTO[] {
		return [
			this.downloadRows,
			this.downloadRows.map((x) => x.children),
			this.downloadRows.map((x) => x.children?.map((y) => y.children)),
			this.downloadRows.map((x) => x.children?.map((y) => y.children?.map((z) => z.children))),
		]
			.flat(3)
			.filter((x) => !!x) as DownloadProgressDTO[];
	}

	tableAction(payload: { action: string; item: DownloadTaskDTO }) {
		Log.info('command', payload);
		this.$emit('action', payload);
	}

	selectedAction(selected: number[]) {
		this.$emit('selected', selected);
	}
}
</script>
