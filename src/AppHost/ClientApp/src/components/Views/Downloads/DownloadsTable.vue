<template>
	<q-expansion-item
		v-model="isExpanded"
		default-opened
		hide-expand-icon
		class="background-sm q-ma-md">
		<template #header>
			<QRow
				align="center"
				class="full-width relative-position">
				<!-- Download Server Title -->
				<QCol class="q-px-md absolute-center row items-center no-wrap">
					<QStatus :value="serverConnectionStore.isServerConnected(plexServer.id)" />
					<span
						class="title q-ml-md"
						:class="{ 'inaccessible-item-text': !accountStore.getHasAccountServerAccess(plexServer.id) }">
						{{ serverStore.getServerName(plexServer.id) }}
					</span>
					<QBadge
						v-if="plexServer.isDownloadsPausedByUser"
						class="q-ml-sm"
						color="warning"
						text-color="black"
						:label="t('components.server-download-status.pause')" />
				</QCol>
				<QCol
					cols="auto"
					class="q-py-none q-ml-auto">
					<QRow
						align="center"
						no-gutters
						class="q-gutter-sm">
						<!-- Clean Download Tasks -->
						<QCol cols="auto">
							<IconButton
								cy="clear-completed-by-server-button"
								icon="mdi-notification-clear-all"
								:tooltip-text="t('components.downloads-table.clear-completed.button')"
								:disabled="!hasCompletedDownloads || clearCompletedLoading"
								@click.stop="openClearCompletedDialog" />
						</QCol>
						<!-- Toggle Expansion button -->
						<QCol cols="auto">
							<IconButton
								cy="toggle-download-table-button"
								:icon="isExpanded ? 'mdi-chevron-up' : 'mdi-chevron-down'"
								@click.stop="toggleExpanded" />
						</QCol>
					</QRow>
				</QCol>
			</QRow>
		</template>
		<template #default>
			<!-- Download Table Per Server -->
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

	<!-- Clear Completed Confirmation Dialog  -->
	<ConfirmationDialog
		:confirm-loading="clearCompletedLoading"
		:name="DialogType.ClearCompletedDownloadsConfirmationDialog"
		:title="t('components.downloads-table.clear-completed.confirmation.title')"
		:text="t('components.downloads-table.clear-completed.confirmation.text')"
		@confirm="clearCompletedByServer" />
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { DownloadProgressDTO, PlexServerDTO } from '@dto';
import { DownloadActions, DownloadStatus } from '@dto';
import { DialogType } from '@enums';
import type { IDownloadTableNode, ISelection } from '@interfaces';
import type { QTreeViewTableHeader } from '@props';
import { flatMapDeep } from 'lodash-es';
import { useDownloadStore, useServerConnectionStore, useDialogStore, useServerStore, useAccountStore } from '@store';
import { toDownloadActions } from '@composables';
import { useI18n } from '#imports';

const serverStore = useServerStore();
const downloadStore = useDownloadStore();
const dialogStore = useDialogStore();
const serverConnectionStore = useServerConnectionStore();
const accountStore = useAccountStore();

const { t } = useI18n();

const loadingIds = ref<{
	id: string;
	action: DownloadActions;
}[]>([]);
const isExpanded = ref(true);
const clearCompletedLoading = ref(false);

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

const hasCompletedDownloads = computed((): boolean => {
	return containsCompletedTasks(props.downloadRows);
});

function mapToTreeNodes(value: DownloadProgressDTO[]): IDownloadTableNode[] {
	return value?.map((node) => {
		return {
			...node,
			key: node.id,
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
		width: 200,
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

	useSubscription(downloadStore.executeDownloadCommand(action, ids, props.plexServer.id).subscribe({
		next: () => {
			set(loadingIds, get(loadingIds).filter((x) => !newIds.includes(x.id)));
		},
		error: () => {
			set(loadingIds, get(loadingIds).filter((x) => !newIds.includes(x.id)));
		},
	}));
}

function toggleExpanded() {
	set(isExpanded, !get(isExpanded));
}

function openClearCompletedDialog() {
	if (!hasCompletedDownloads.value) {
		return;
	}

	dialogStore.openDialog(DialogType.ClearCompletedDownloadsConfirmationDialog);
}

function closeClearCompletedDialog() {
	dialogStore.closeDialog(DialogType.ClearCompletedDownloadsConfirmationDialog);
}

function clearCompletedByServer() {
	if (get(clearCompletedLoading)) {
		return;
	}

	set(clearCompletedLoading, true);
	useSubscription(
		downloadStore.executeDownloadCommand(DownloadActions.Clear, [], props.plexServer.id).subscribe({
			next: (result) => {
				if (result.isSuccess) {
					closeClearCompletedDialog();
				}
			},
			error: () => {
				set(clearCompletedLoading, false);
			},
			complete: () => {
				set(clearCompletedLoading, false);
			},
		}),
	);
}

function containsCompletedTasks(downloadRows: DownloadProgressDTO[]): boolean {
	for (const downloadRow of downloadRows) {
		if (downloadRow.status === DownloadStatus.Completed || containsCompletedTasks(downloadRow.children ?? [])) {
			return true;
		}
	}

	return false;
}

function getAllIds(nodes: IDownloadTableNode[]): string[] {
	return flatMapDeep(nodes, (node) => [
		node.id, // Assuming `key` holds the ID in TreeNode
		...getAllIds(node.children || []),
	]);
}
</script>

<style lang="scss">
.inaccessible-item-text {
	text-decoration: line-through;
	opacity: 0.62;
}
</style>
