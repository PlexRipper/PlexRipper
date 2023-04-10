<template>
	<div>
		<q-tree-view-table :nodes="downloadRows" :columns="getHeaders" />
		<!--		<v-tree-view-table-->
		<!--			:items="downloadRows"-->
		<!--			:headers="getHeaders"-->
		<!--			height-auto-->
		<!--			media-icons-->
		<!--			load-children-->
		<!--			item-key="id"-->
		<!--			@action="tableAction"-->
		<!--			@selected="selectedAction"-->
		<!--		/>-->
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { computed, defineEmits, defineProps, ref } from 'vue';
import { DownloadProgressDTO, DownloadTaskDTO, FileMergeProgress } from '@dto/mainApi';
import { QTreeViewTableHeader } from '@props';

const props = defineProps<{
	loading?: boolean;
	serverId: number;
	downloadRows: DownloadProgressDTO[];
}>();

const emit = defineEmits<{
	(e: 'action', payload: { action: string; item: DownloadTaskDTO }): void;
	(e: 'selected', payload: number[]): void;
}>();

const fileMergeProgressList = ref<FileMergeProgress[]>([]);

const getHeaders = computed((): QTreeViewTableHeader[] => {
	return [
		// {
		// 	text: 'Id',
		// 	value: 'id',
		// 	maxWidth: 50,
		// },
		{
			label: 'Title',
			name: 'title',
			field: 'title',
		},
		{
			label: 'Status',
			name: 'status',
			field: 'status',
		},
		{
			label: 'Received',
			name: 'dataReceived',
			field: 'dataReceived',
		},
		{
			label: 'Size',
			name: 'dataTotal',
			field: 'dataTotal',
		},
		{
			label: 'Speed',
			name: 'downloadSpeed',
			field: 'downloadSpeed',
		},
		{
			label: 'ETA',
			name: 'timeRemaining',
			field: 'timeRemaining',
		},
		{
			label: 'Percentage',
			name: 'percentage',
			field: 'percentage',
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			sortable: false,
		},
	];
});

const flatDownloadRows = computed((): DownloadProgressDTO[] => {
	return [
		props.downloadRows,
		props.downloadRows.map((x) => x.children),
		props.downloadRows.map((x) => x.children?.map((y) => y.children)),
		props.downloadRows.map((x) => x.children?.map((y) => y.children?.map((z) => z.children))),
	]
		.flat(3)
		.filter((x) => !!x) as DownloadProgressDTO[];
});

function tableAction(payload: { action: string; item: DownloadTaskDTO }) {
	Log.info('command', payload);
	emit('action', payload);
}

function selectedAction(selected: number[]) {
	Log.info('selected', selected);
	emit('selected', selected);
}
</script>
