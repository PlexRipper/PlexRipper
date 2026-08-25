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
			<QTreeTable
				:nodes="nodes"
				:columns="getDownloadTableColumns"
				:selection-keys="downloadStore.getSelectedDownloadTasks(plexServer.id)"
				@selected="downloadStore.updateSelectedDownloadTasks(plexServer.id, $event)">
				<template #cell-title="{ data }: { data: IDownloadTableNode }">
					<QMediaTypeIcon
						v-if="data.mediaType"
						:media-type="data.mediaType"
						class="q-mr-sm"
						:size="26" />
					<QText
						:cy="`column-title-${data.id}`"
						:value="data.title" />
				</template>
				<template #cell-status="{ data }: { data: IDownloadTableNode }">
					<QText
						:cy="`column-status-${data.id}`"
						:value="translateDownloadStatus(data.status)" />
				</template>
				<template #cell-actions="{ data }: { data: IDownloadTableNode }">
					<QRow
						justify="start"
						no-wrap>
						<QCol cols="auto">
							<IconSquareButton
								v-for="action in data.actions"
								:key="`${data.id}-${kebabCase(action.type)}`"
								:cy="`column-actions-${kebabCase(action.type)}-${data.id}`"
								:disabled="action.disabled"
								:icon="toButtonIcon(action.type)"
								:loading="action.loading"
								dense
								@click.stop="onTableAction({ action: action.type, data })" />
						</QCol>
					</QRow>
				</template>
			</QTreeTable>
		</template>
	</q-expansion-item>

	<!-- Clear Completed Confirmation Dialog  -->
	<ConfirmationDialog
		:id="plexServer.id"
		:confirm-loading="clearCompletedLoading"
		:name="DialogType.ClearCompletedDownloadsConfirmationDialog"
		:title="t('components.downloads-table.clear-completed.confirmation.title')"
		:text="t('components.downloads-table.clear-completed.confirmation.text')"
		@confirm="clearCompletedByServer" />
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { TreeNode } from 'primevue/treenode';
import type { DownloadProgressDTO, PlexServerDTO } from '@dto';
import { DownloadActions, DownloadStatus } from '@dto';
import { ButtonType, DialogType } from '@enums';
import type { IDownloadTableNode, ISelection } from '@interfaces';
import type { QTreeTableColumn } from '@props';
import { QTreeTableColumnType } from '@props';
import { flatMapDeep, kebabCase } from 'lodash-es';
import { useDownloadStore, useServerConnectionStore, useDialogStore, useServerStore, useAccountStore } from '@store';
import { toDownloadActions, translateDownloadStatus } from '@composables';
import Convert from '@class/Convert';
import { useI18n } from '#imports';

const serverStore = useServerStore();
const downloadStore = useDownloadStore();
const dialogStore = useDialogStore();
const serverConnectionStore = useServerConnectionStore();
const accountStore = useAccountStore();

const { t } = useI18n();

interface DownloadTreeNode extends TreeNode {
	data: IDownloadTableNode;
	children?: DownloadTreeNode[];
}

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

const nodes = computed((): DownloadTreeNode[] => {
	// TODO: Move property mapping to back-end to increase performance
	return mapToTreeNodes(downloadStore.getDownloadsByServerId(props.plexServer.id));
});

const hasCompletedDownloads = computed((): boolean => {
	return containsCompletedTasks(props.downloadRows);
});

function mapToTreeNodes(value: DownloadProgressDTO[]): DownloadTreeNode[] {
	return value?.map((node) => {
		const children = mapToTreeNodes(node.children);
		const data = {
			...node,
			key: node.id,
			label: node.title,
			children: children.map((child) => child.data!),
			actions: toDownloadActions(node.status).map((action) => ({
				type: action,
				// show loading icon on action and disable the rest
				loading: get(loadingIds).some((y) => node.id === y.id && action === y.action),
				disabled: get(loadingIds).some((y) => node.id === y.id && action !== y.action),
			})),
		};

		return {
			key: node.id,
			label: node.title,
			data,
			children,
		};
	}) ?? [];
}

const getDownloadTableColumns: QTreeTableColumn[] = [
	{
		header: t('components.downloads-table.columns.title'),
		field: 'title',
	},
	{
		header: t('components.downloads-table.columns.status'),
		field: 'status',
		align: 'right',
		width: 200,
	},
	{
		header: t('components.downloads-table.columns.data-received'),
		field: 'dataReceived',
		type: QTreeTableColumnType.FileSize,
		align: 'right',
		width: 120,
	},
	{
		header: t('components.downloads-table.columns.data-total'),
		field: 'dataTotal',
		type: QTreeTableColumnType.FileSize,
		width: 120,
		align: 'right',
	},
	{
		header: t('components.downloads-table.columns.speed'),
		field: 'downloadSpeed',
		type: QTreeTableColumnType.FileSpeed,
		align: 'right',
		width: 120,
	},
	{
		header: t('components.downloads-table.columns.time-remaining'),
		field: 'timeRemaining',
		type: QTreeTableColumnType.Duration,
		align: 'left',
		width: 120,
	},
	{
		header: t('components.downloads-table.columns.percentage'),
		field: 'percentage',
		type: QTreeTableColumnType.Percentage,
		align: 'right',
		width: 120,
	},
	{
		header: t('components.downloads-table.columns.actions'),
		field: 'actions',
		type: QTreeTableColumnType.Actions,
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

function toButtonIcon(action: DownloadActions): string {
	switch (action) {
		case DownloadActions.Details:
			return Convert.buttonTypeToIcon(ButtonType.Details);

		case DownloadActions.Delete:
			return Convert.buttonTypeToIcon(ButtonType.Delete);

		case DownloadActions.Start:
			return Convert.buttonTypeToIcon(ButtonType.Start);

		case DownloadActions.Pause:
			return Convert.buttonTypeToIcon(ButtonType.Pause);

		case DownloadActions.Stop:
			return Convert.buttonTypeToIcon(ButtonType.Stop);

		case DownloadActions.Clear:
			return Convert.buttonTypeToIcon(ButtonType.Clear);

		case DownloadActions.Restart:
			return Convert.buttonTypeToIcon(ButtonType.Restart);

		default:
			return Convert.buttonTypeToIcon(ButtonType.None);
	}
}

function toggleExpanded() {
	set(isExpanded, !get(isExpanded));
}

function openClearCompletedDialog() {
	if (!get(hasCompletedDownloads)) {
		return;
	}

	dialogStore.openDialog(DialogType.ClearCompletedDownloadsConfirmationDialog, props.plexServer.id);
}

function closeClearCompletedDialog() {
	dialogStore.closeDialog(DialogType.ClearCompletedDownloadsConfirmationDialog, props.plexServer.id);
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
