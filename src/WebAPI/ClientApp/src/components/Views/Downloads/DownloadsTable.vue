<template>
	<q-expansion-item default-opened class="background-sm q-ma-md">
		<template #header>
			<q-row justify="between" align="center">
				<!-- Download Server Settings -->
				<q-col cols="auto" style="white-space: nowrap">
					<server-download-status v-if="false" style="display: inline-block" />
				</q-col>
				<q-col> </q-col>
				<!-- Download Server Title -->
				<q-col cols="auto">
					<QStatus :value="serverConnectionStore.isServerConnected(plexServer.id)" />
					<span class="title q-ml-md">{{ serverStore.getServerName(plexServer.id) }}</span>
				</q-col>
				<q-col class="q-py-none"></q-col>
			</q-row>
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
import type { QTreeViewTableItem } from '@props';
import { getDownloadTableColumns } from '#imports';
import { IDownloadTableNode, ISelection } from '@interfaces';
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

function mapToTreeNodes(value: DownloadProgressDTO[]): IDownloadTableNode[] {
	if (!value) {
		return [];
	}
	return value.map((x) => {
		return {
			...x,
			key: `${x.mediaType}-${x.id}`,
			label: x.title,
			children: mapToTreeNodes(x.children),
		};
	});
}

function tableAction(payload: { action: string; data: QTreeViewTableItem }) {
	Log.info('command', payload);
	emit('action', {
		action: payload.action,
		item: payload.data as unknown as DownloadProgressDTO,
	});
}
</script>
