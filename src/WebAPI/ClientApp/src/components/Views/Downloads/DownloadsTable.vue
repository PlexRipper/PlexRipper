<template>
	<q-expansion-item
		default-opened
		class="background-sm q-ma-md">
		<template #header>
			<QRow
				justify="between"
				align="center">
				<!-- Download Server Settings -->
				<QCol
					cols="auto"
					style="white-space: nowrap">
					<ServerDownloadStatus
						v-if="false"
						style="display: inline-block" />
				</QCol>
				<QCol />
				<!-- Download Server Title -->
				<QCol cols="auto">
					<QStatus :value="serverConnectionStore.isServerConnected(plexServer.id)" />
					<span class="title q-ml-md">{{ serverStore.getServerName(plexServer.id) }}</span>
				</QCol>
				<QCol class="q-py-none" />
			</QRow>
		</template>
		<template #default>
			<PrimeTreeTable
				:nodes="nodes"
				:columns="getDownloadTableColumns"
				:header-selected="downloadStore.getHeaderSelection(plexServer.id)"
				:selected="downloadStore.getSelectedDownloadTasks(plexServer.id)"
				:max-selection-count="downloadStore.getDownloadSelection(plexServer.id)?.maxSelectionCount"
				@action="tableAction($event)"
				@all-selected="downloadStore.setAllSelectedDownloadTasks(plexServer.id, $event)"
				@selected="downloadStore.updateSelectedDownloadTasks(plexServer.id, $event)" />
		</template>
	</q-expansion-item>
</template>

<script setup lang="ts">
import Log from 'consola';
import type { DownloadProgressDTO, PlexServerDTO } from '@dto';
import type { IDownloadTableNode, ISelection } from '@interfaces';
import type { QTreeViewTableHeader } from '@props';
import { useDownloadStore, useServerConnectionStore, useServerStore } from '~/store';

const downloadStore = useDownloadStore();
const serverConnectionStore = useServerConnectionStore();
const serverStore = useServerStore();
const props = defineProps<{
	loading?: boolean;
	plexServer: PlexServerDTO;
	downloadRows: DownloadProgressDTO[];
}>();

const emit = defineEmits<{
	(e: 'action', payload: { action: string; item: DownloadProgressDTO }): void;
	(e: 'selected', payload: ISelection): void;
}>();

const nodes = computed((): IDownloadTableNode[] => {
	// TODO: Move this to the back-end to increase performance
	return mapToTreeNodes(downloadStore.getDownloadsByServerId(props.plexServer.id));
});

const getDownloadTableColumns: QTreeViewTableHeader[] = [
	{
		label: 'Title',
		field: 'title',
		type: 'title',
	},
	{
		label: 'Status',
		field: 'status',
		align: 'right',
		width: 120,
	},
	{
		label: 'Received',
		field: 'dataReceived',
		type: 'file-size',
		align: 'right',
		width: 120,
	},
	{
		label: 'Size',
		field: 'dataTotal',
		type: 'file-size',
		width: 120,
		align: 'right',
	},
	{
		label: 'Speed',
		field: 'downloadSpeed',
		type: 'file-speed',
		align: 'right',
		width: 120,
	},
	{
		label: 'ETA',
		field: 'timeRemaining',
		type: 'duration',
		align: 'right',
		width: 120,
	},
	{
		label: 'Percentage',
		field: 'percentage',
		type: 'percentage',
		align: 'center',
		width: 120,
	},
	{
		label: 'Actions',
		field: 'actions',
		type: 'actions',
		width: 200,
		align: 'center',
		sortable: false,
	},
];

function mapToTreeNodes(value: DownloadProgressDTO[]): IDownloadTableNode[] {
	if (!value) {
		return [];
	}
	return value.map((x) => {
		return {
			...x,
			key: x.id,
			label: x.title,
			children: mapToTreeNodes(x.children),
		};
	});
}

function tableAction(payload: { action: string; data: IDownloadTableNode }) {
	Log.info('command', payload);
	emit('action', {
		action: payload.action,
		item: payload.data as unknown as DownloadProgressDTO,
	});
}
</script>
