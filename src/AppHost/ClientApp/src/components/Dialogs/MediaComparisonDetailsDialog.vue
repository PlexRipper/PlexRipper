<template>
	<QCardDialog
		:name="DialogType.MediaComparisonDetailsDialog"
		:loading="loading"
		close-button
		full-height
		full-width
		cy="media-comparison-details-dialog"
		@opened="onOpen">
		<template #title>
			<QRow
				align="center"
				gutter="sm">
				<QCol cols="auto">
					<MediaComparisonStateButton
						v-if="selectedMediaItem"
						show-tooltip
						:comparison-state="getPlexMediaComparisonState(selectedMediaItem)"
						dense
						cy="media-comparison-details-dialog-state" />
				</QCol>
				<QCol>
					<QText size="h4">
						{{ selectedMediaItem?.title ?? t('general.error.unknown') }}
					</QText>
				</QCol>
			</QRow>
		</template>

		<template #default>
			<div
				v-if="selectedMediaItem"
				class="media-comparison-details">
				<QText
					class="media-comparison-details__description q-mb-md"
					:value="descriptionFor(selectedMediaItem)" />

				<QTreeTable
					class="media-comparison-details__table"
					data-cy="media-comparison-details-table"
					:loading="loading"
					:nodes="detailTreeRows"
					:columns="comparisonColumns"
					:selection-keys="selectedRows"
					@selected="onSelectionChange">
					<template #cell-title="{ data }: { data: IComparisonDetailRow }">
						<QRow
							align="center"
							no-wrap>
							<QCol cols="auto">
								<MediaComparisonStateButton
									:comparison-state="data.comparisonState"
									show-tooltip
									dense />
							</QCol>
							<QCol>
								<QText
									:cy="`media-comparison-details-title-${data.key}`"
									:value="data.title" />
							</QCol>
						</QRow>
					</template>
					<template #cell-ownedQuality="{ data }: { data: IComparisonDetailRow }">
						<MediaVideoQuality :quality="data.ownedQuality" />
					</template>
					<template #cell-remoteQuality="{ data }: { data: IComparisonDetailRow }">
						<MediaVideoQuality :quality="data.remoteQuality" />
					</template>
					<template #cell-location="{ data }: { data: IComparisonDetailRow }">
						<QRow
							align="center"
							no-wrap>
							<QCol cols="auto">
								<QText :value="data.remoteServerName" />
							</QCol>
							<QCol cols="auto">
								<QIcon
									class="q-mx-xs"
									name="mdi-arrow-right-thin" />
							</QCol>
							<QCol>
								<QText :value="data.remoteLibraryTitle" />
							</QCol>
						</QRow>
					</template>
					<template #cell-actions="{ data }: { data: IComparisonDetailRow }">
						<QRow justify="end">
							<QCol cols="auto">
								<IconSquareButton
									:cy="`media-comparison-details-download-${data.key}`"
									:disabled="!canDownload(data)"
									icon="mdi-download"
									:tooltip-text="t('components.media-overview.comparison.download-selected')"
									dense
									@click.stop="downloadRows([data])" />
							</QCol>
						</QRow>
					</template>
					<template #empty>
						<div class="full-width text-center q-pa-md">
							{{ t('components.media-overview.comparison.details-no-actionable-rows') }}
						</div>
					</template>
				</QTreeTable>
			</div>
		</template>

		<template #actions="{ close }">
			<QRow justify="end">
				<QCol cols="auto">
					<BaseButton
						:label="t('general.commands.close')"
						cy="media-comparison-details-close-button"
						@click="close" />
				</QCol>
				<QCol cols="auto">
					<BaseButton
						:label="t('components.media-overview.comparison.download-selected')"
						:disabled="selectedDownloadRows.length === 0"
						cy="media-comparison-details-download-selected-button"
						@click="downloadRows(selectedDownloadRows)" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { PlexMediaComparisonState, PlexMediaType, VideoQuality } from '@dto';
import type { DownloadMediaDTO, PlexMediaComparisonDetailsRowDTO, PlexMediaSlimDTO } from '@dto';
import { DialogType } from '@enums';
import type { QTreeTableColumn } from '@props';
import { QTreeTableColumnType } from '@props';
import { useDialogStore, useMediaStore, useSettingsStore } from '@store';
import { getPlexMediaComparisonState, getPlexMediaComparisonStateFromId } from '@composables';
import type { TreeNode } from 'primevue/treenode';
import type { TreeTableSelectionKeys } from 'primevue/treetable';

interface IComparisonDetailRow {
	key: string;
	label: string;
	title: string;
	remoteQuality: VideoQuality;
	ownedQuality: VideoQuality;
	remoteServerName: string;
	remoteLibraryTitle: string;
	comparisonState: PlexMediaComparisonState;
	plexMediaId: number;
	remotePlexLibraryId: number;
	remotePlexServerId: number;
	type: PlexMediaComparisonDetailsRowDTO['type'];
	children?: IComparisonDetailRow[];
}

interface IComparisonDetailTreeNode extends TreeNode {
	data: IComparisonDetailRow;
	children?: IComparisonDetailTreeNode[];
}

const { t } = useI18n();
const mediaStore = useMediaStore();
const dialogStore = useDialogStore();
const settingsStore = useSettingsStore();

const loading = ref(false);
const comparisonRows = ref<PlexMediaComparisonDetailsRowDTO[]>([]);
const selectedMediaItem = ref<PlexMediaSlimDTO | null>(null);
const selectedRows = ref<TreeTableSelectionKeys>({});

const detailRows = computed(() => get(comparisonRows)
	.filter((row) => row.isActionable)
	.map(mapDetailRow));

const detailRowsTree = computed(() => buildDetailTree(get(detailRows)));

const detailTreeRows = computed(() => mapToTreeNodes(get(detailRowsTree)));

const selectedDownloadRows = computed(() => getSelectedDownloadRows(get(detailRowsTree)));

const comparisonColumns: QTreeTableColumn[] = [
	{
		header: t('components.media-overview.comparison.details-column-title'),
		field: 'title',
	},
	{
		header: t('components.media-overview.comparison.details-column-owned-quality'),
		field: 'ownedQuality',
		type: QTreeTableColumnType.Custom,
		width: 160,
		align: 'left',
	},
	{
		header: t('components.media-overview.comparison.details-column-remote-quality'),
		field: 'remoteQuality',
		type: QTreeTableColumnType.Custom,
		width: 160,
		align: 'left',
	},
	{
		header: t('components.media-overview.comparison.details-column-location'),
		field: 'location',
		type: QTreeTableColumnType.Custom,
		width: 240,
		align: 'left',
	},
	{
		header: t('components.downloads-table.columns.actions'),
		field: 'actions',
		type: QTreeTableColumnType.Actions,
		width: 110,
		align: 'right',
		sortable: false,
	},
];

function mapDetailRow(row: PlexMediaComparisonDetailsRowDTO): IComparisonDetailRow {
	return {
		key: `${row.type}-${row.id}`,
		label: row.title,
		title: row.title,
		ownedQuality: row.ownedQuality ?? VideoQuality.None,
		remoteQuality: row.remoteQuality ?? VideoQuality.None,
		remoteServerName: sourceServerLabel(row),
		remoteLibraryTitle: sourceLibraryLabel(row),
		comparisonState: getPlexMediaComparisonStateFromId(row.comparisonId),
		plexMediaId: row.plexMediaId,
		remotePlexLibraryId: row.remotePlexLibraryId,
		remotePlexServerId: row.remotePlexServerId,
		type: row.type,
	};
}

function mapToTreeNodes(rows: IComparisonDetailRow[]): IComparisonDetailTreeNode[] {
	return rows.map((row) => ({
		key: row.key,
		label: row.label,
		data: row,
		children: mapToTreeNodes(row.children ?? []),
	}));
}

function buildDetailTree(rows: IComparisonDetailRow[]): IComparisonDetailRow[] {
	const nodesById = new Map<string, IComparisonDetailRow>();
	const parentKeysByChildKey = new Map<string, string>();

	for (const row of get(comparisonRows).filter((comparisonRow) => comparisonRow.isActionable)) {
		const rowKey = `${row.type}-${row.id}`;
		nodesById.set(rowKey, rows.find((detailRow) => detailRow.key === rowKey) as IComparisonDetailRow);
		if (row.parentId) {
			parentKeysByChildKey.set(rowKey, `${row.type === PlexMediaType.Episode ? PlexMediaType.Season : row.type}-${row.parentId}`);
		}
	}

	const roots: IComparisonDetailRow[] = [];
	for (const row of rows) {
		const parentKey = parentKeysByChildKey.get(row.key);
		const parent = parentKey ? nodesById.get(parentKey) : undefined;
		if (!parent) {
			roots.push(row);
			continue;
		}

		parent.children ??= [];
		parent.children.push(row);
	}

	return roots;
}

function onOpen(value: unknown) {
	const mediaItem = value as PlexMediaSlimDTO;
	set(selectedMediaItem, mediaItem);
	set(loading, true);
	set(comparisonRows, []);
	set(selectedRows, {});
	useSubscription(
		mediaStore.getMediaComparisonDetails(mediaItem.id, mediaItem.type).subscribe({
			next: (details) => set(comparisonRows, details.rows),
			complete: () => set(loading, false),
			error: () => set(loading, false),
		}),
	);
}

function onSelectionChange(keys: TreeTableSelectionKeys) {
	set(selectedRows, Object.fromEntries(
		Object.entries(keys).filter(([, value]) => value.checked || value.partialChecked),
	));
}

function getSelectedDownloadRows(rows: IComparisonDetailRow[]): IComparisonDetailRow[] {
	return rows.flatMap((row) => {
		const children = getSelectedDownloadRows(row.children ?? []);
		if (!get(selectedRows)[row.key]?.checked)
			return children;

		return [row, ...children].filter(canDownload);
	});
}

function canDownload(row: IComparisonDetailRow): boolean {
	return row.plexMediaId > 0 && row.remoteQuality !== VideoQuality.None;
}

function downloadRows(rows: IComparisonDetailRow[]) {
	const downloadCommands = rows
		.filter(canDownload)
		.map(toDownloadMediaCommand);

	if (downloadCommands.length === 0)
		return;

	dialogStore.openMediaConfirmationDownloadDialog(downloadCommands);
}

function toDownloadMediaCommand(row: IComparisonDetailRow): DownloadMediaDTO {
	return {
		type: row.type,
		mediaIds: [row.plexMediaId],
		plexLibraryId: row.remotePlexLibraryId,
		plexServerId: row.remotePlexServerId,
		qualities: [],
		keepCompletedInDownloadFolder: settingsStore.downloadManagerSettings.keepCompletedInDownloadFolder,
	};
}

function sourceServerLabel(row: PlexMediaComparisonDetailsRowDTO): string {
	return row.remoteServerName || t('general.error.unknown');
}

function sourceLibraryLabel(row: PlexMediaComparisonDetailsRowDTO): string {
	return row.remoteLibraryTitle || t('general.error.unknown');
}

function descriptionFor(mediaItem: PlexMediaSlimDTO): string {
	switch (getPlexMediaComparisonState(mediaItem)) {
		case PlexMediaComparisonState.Missing:
			return t('components.media-overview.comparison.details-missing', { title: mediaItem.title });
		case PlexMediaComparisonState.HigherQuality:
			return t('components.media-overview.comparison.details-higher-quality', { title: mediaItem.title });
		case PlexMediaComparisonState.Partial:
			return t('components.media-overview.comparison.details-partial', { title: mediaItem.title });
		case PlexMediaComparisonState.PartialAndHigherQuality:
			return t('components.media-overview.comparison.details-partial-and-higher-quality', { title: mediaItem.title });
		default:
			return t('components.media-overview.comparison.details-not-actionable', { title: mediaItem.title });
	}
}
</script>

<style lang="scss">
.media-comparison-details {
	display: flex;
	flex-direction: column;
	height: 100%;
	min-height: 0;
}

.media-comparison-details__description {
	flex: 0 0 auto;
}

.media-comparison-details__table {
	display: flex;
	flex: 1 1 auto;
	flex-direction: column;
	min-height: 0;

	.p-treetable-table-container {
		flex: 1 1 auto;
		min-height: 0;
		overflow: auto;
	}

	.p-treetable-thead {
		position: sticky;
		top: 0;
		z-index: 2;
	}

	.p-paginator {
		position: sticky;
		bottom: 0;
		z-index: 2;
		flex: 0 0 auto;
	}
}
</style>
