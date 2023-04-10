<template>
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
	(e: 'action', payload: { action: string; item: DownloadProgressDTO }): void;
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
			align: 'right',
			width: 120,
		},
		{
			label: 'Received',
			name: 'dataReceived',
			field: 'dataReceived',
			type: 'file-size',
			align: 'right',
			width: 120,
		},
		{
			label: 'Size',
			name: 'dataTotal',
			field: 'dataTotal',
			type: 'file-size',
			width: 120,
			align: 'right',
		},
		{
			label: 'Speed',
			name: 'downloadSpeed',
			field: 'downloadSpeed',
			type: 'file-speed',
			align: 'right',
			width: 120,
		},
		{
			label: 'ETA',
			name: 'timeRemaining',
			field: 'timeRemaining',
			type: 'duration',
			align: 'right',
			width: 120,
		},
		{
			label: 'Percentage',
			name: 'percentage',
			field: 'percentage',
			type: 'percentage',
			align: 'center',
			width: 120,
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			type: 'actions',
			width: 160,
			align: 'center',
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

function tableAction(payload: { action: string; data: QTreeViewTableItem }) {
	Log.info('command', payload);
	emit('action', {
		action: payload.action,
		item: payload.data as DownloadProgressDTO,
	});
}

function selectedAction(selected: number[]) {
	Log.info('selected', selected);
	emit('selected', selected);
}
</script>
