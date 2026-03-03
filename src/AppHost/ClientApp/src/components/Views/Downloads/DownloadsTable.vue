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
						:plex-server-id="plexServer.id"
						:is-downloads-paused-by-user="plexServer.isDownloadsPausedByUser"
						style="display: inline-block" />
				</QCol>
				<QCol />
				<!-- Download Server Title -->
				<QCol cols="auto">
					<QStatus :value="serverConnectionStore.isServerConnected(plexServer.id)" />
					<span class="title q-ml-md">{{ serverStore.getServerName(plexServer.id) }}</span>
					<QBadge
						v-if="plexServer.isDownloadsPausedByUser"
						class="q-ml-sm"
						color="warning"
						text-color="black"
						:label="t('components.server-download-status.pause')" />
				</QCol>
				<QCol class="q-py-none" />
			</QRow>
		</template>
		<template #default>
			<PrimeTreeTable
				:nodes="nodes"
				:columns="getDownloadTableColumns"
				:data-key="'id' as keyof DownloadProgressDTO"
				:header-selected="downloadStore.getHeaderSelection(plexServer.id)"
				:selected="downloadStore.getSelectedDownloadTasks(plexServer.id)"
				:max-selection-count="downloadStore.getDownloadSelection(plexServer.id)?.maxSelectionCount"
				@action="onTableAction($event)"
				@all-selected="downloadStore.setAllSelectedDownloadTasks(plexServer.id, $event)"
				@selected="downloadStore.updateSelectedDownloadTasks(plexServer.id, $event)" />
		</template>
	</q-expansion-item>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { DownloadProgressDTO, PlexServerDTO } from '@dto';
import { DownloadActions } from '@dto';
import type { IDownloadTableNode, ISelection } from '@interfaces';
import type { QTreeViewTableHeader } from '@props';
import { flatMapDeep } from 'lodash-es';
import { useDownloadStore, useServerConnectionStore, useDialogStore, useServerStore } from '@store';
import { toDownloadActions } from '@composables';
import { useI18n } from '#imports';

const serverStore = useServerStore();
const downloadStore = useDownloadStore();
const dialogStore = useDialogStore();
const serverConnectionStore = useServerConnectionStore();

const { t } = useI18n();

const loadingIds = ref<{
	id: string;
	action: DownloadActions;
}[]>([]);

const props = defineProps<{
	loading?: boolean;
	plexServer: PlexServerDTO;
	downloadRows: DownloadProgressDTO[];
}>();

defineEmits<{
	(e: 'selected', payload: ISelection): void;
}>();

const nodes = computed((): IDownloadTableNode[] => {
	// TODO: Move property mapping to back-end to increase performance
	return mapToTreeNodes(downloadStore.getDownloadsByServerId(props.plexServer.id));
});

function mapToTreeNodes(value: DownloadProgressDTO[]): IDownloadTableNode[] {
	return value?.map((node) => {
		return {
			...node,
			label: node.title,
			children: mapToTreeNodes(node.children),
			actions: toDownloadActions(node.status).map((action) => ({
				type: action,
				// show loading icon on action and disable the rest
				loading: get(loadingIds).some((y) => node.id === y.id && action === y.action),
				disabled: get(loadingIds).some((y) => node.id === y.id && action !== y.action),
			})),
		};
	}) ?? [];
}

const getDownloadTableColumns: QTreeViewTableHeader[] = [
	{
		label: t('components.downloads-table.columns.title'),
		field: 'title',
		type: 'title',
	},
	{
		label: t('components.downloads-table.columns.status'),
		field: 'status',
		align: 'right',
		width: 120,
	},
	{
		label: t('components.downloads-table.columns.data-received'),
		field: 'dataReceived',
		type: 'file-size',
		align: 'right',
		width: 120,
	},
	{
		label: t('components.downloads-table.columns.data-total'),
		field: 'dataTotal',
		type: 'file-size',
		width: 120,
		align: 'right',
	},
	{
		label: t('components.downloads-table.columns.speed'),
		field: 'downloadSpeed',
		type: 'file-speed',
		align: 'right',
		width: 120,
	},
	{
		label: t('components.downloads-table.columns.time-remaining'),
		field: 'timeRemaining',
		type: 'duration',
		align: 'right',
		width: 120,
	},
	{
		label: t('components.downloads-table.columns.percentage'),
		field: 'percentage',
		type: 'percentage',
		align: 'right',
		width: 120,
	},
	{
		label: t('components.downloads-table.columns.actions'),
		field: 'actions',
		type: 'actions',
		width: 200,
		align: 'right',
		sortable: false,
	},
];

function onTableAction({ action, data }: { action: DownloadActions; data: IDownloadTableNode }) {
	const ids: string[] = [data.id];

	if (action === DownloadActions.Details) {
		dialogStore.openDownloadTaskDetailsDialog(data.id);
		return;
	}

	const newIds = getAllIds([data]);
	get(loadingIds).push(...newIds.map((id) => ({ id, action })));

	useSubscription(downloadStore.executeDownloadCommand(action, ids).subscribe(() => {
		set(loadingIds, get(loadingIds).filter((x) => !newIds.includes(x.id)));
	}));
}

function getAllIds(nodes: IDownloadTableNode[]): string[] {
	return flatMapDeep(nodes, (node) => [
		node.id, // Assuming `key` holds the ID in TreeNode
		...getAllIds(node.children || []),
	]);
}
</script>
