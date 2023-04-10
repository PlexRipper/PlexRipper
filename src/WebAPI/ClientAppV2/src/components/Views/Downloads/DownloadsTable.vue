<template>
	<div>
		<q-tree-view-table :nodes="getData" :columns="getHeaders" @action="tableAction" />
		<!--		<v-tree-view-table-->
		<!--			:items="downloadRows"-->
		<!--			:headers="getHeaders"-->
		<!--			height-auto-->
		<!--			media-icons-->
		<!--			load-children-->
		<!--			item-key="id"-->
		<!--		-->
		<!--			@selected="selectedAction"-->
		<!--		/>-->
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { computed, defineEmits, defineProps, ref } from 'vue';
import { DownloadProgressDTO, DownloadTaskDTO, FileMergeProgress } from '@dto/mainApi';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';

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

const getData = computed((): QTreeViewTableItem[] => {
	return props.downloadRows as QTreeViewTableItem[];
});

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
			type: 'file-size',
		},
		{
			label: 'Size',
			name: 'dataTotal',
			field: 'dataTotal',
			type: 'file-size',
		},
		{
			label: 'Speed',
			name: 'downloadSpeed',
			field: 'downloadSpeed',
			type: 'file-speed',
		},
		{
			label: 'ETA',
			name: 'timeRemaining',
			field: 'timeRemaining',
			type: 'duration',
		},
		{
			label: 'Percentage',
			name: 'percentage',
			field: 'percentage',
			type: 'percentage',
			width: 150,
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			type: 'actions',
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

function tableAction(payload: { action: string; data: DownloadTaskDTO | QTreeViewTableItem }) {
	Log.info('command', payload);
	emit('action', payload);
}

function selectedAction(selected: number[]) {
	Log.info('selected', selected);
	emit('selected', selected);
}
</script>
