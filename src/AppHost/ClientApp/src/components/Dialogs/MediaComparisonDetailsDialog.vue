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
				justify="between"
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
				<QCol cols="auto">
					<VerticalButton
						class="q-mr-xl"
						:label="t('components.media-overview.comparison.download-selected')"
						icon="mdi-download"
						:disabled="selectedDownloadRows.length === 0"
						cy="media-comparison-details-dialog-download-button"
						@click="downloadRows(selectedDownloadRows)" />
				</QCol>
			</QRow>
		</template>

		<template #default>
			<div
				v-if="selectedMediaItem"
				class="media-comparison-details">
				<QTreeTable
					class="media-comparison-details__table"
					data-cy="media-comparison-details-table"
					:loading="loading"
					:nodes="detailTreeRows"
					:columns="comparisonColumns"
					:selection-keys="selectedRows"
					@selected="onSelectionChange">
					<template #cell-title="{ node, data }: { node: IComparisonDetailTreeNode; data: IComparisonDetailsRow }">
						<QRow
							align="center"
							no-wrap>
							<QCol cols="auto">
								<MediaComparisonStateButton
									:comparison-state="getComparisonState(data)"
									show-tooltip
									dense />
							</QCol>
							<QCol>
								<QText
									:cy="`media-comparison-details-title-${node.key}`"
									:value="data.title" />
							</QCol>
						</QRow>
					</template>
					<template #cell-ownedQuality="{ data }: { data: IComparisonDetailsRow }">
						<MediaVideoQuality :quality="data.ownedQuality ?? VideoQuality.None" />
					</template>
					<template #cell-remoteQuality="{ data }: { data: IComparisonDetailsRow }">
						<MediaVideoQuality :quality="data.remoteQuality ?? VideoQuality.None" />
					</template>
					<template #cell-location="{ data }: { data: IComparisonDetailsRow }">
						<QRow
							align="center"
							no-wrap>
							<QCol cols="auto">
								<QText :value="serverStore.getServerName(data.plexServerId)" />
							</QCol>
							<QCol cols="auto">
								<QIcon
									class="q-mx-xs"
									name="mdi-arrow-right-thin" />
							</QCol>
							<QCol>
								<QText :value="libraryStore.getLibraryName(data.plexLibraryId)" />
							</QCol>
						</QRow>
					</template>
					<template #cell-actions="{ node, data }: { node: IComparisonDetailTreeNode; data: IComparisonDetailsRow }">
						<QRow justify="end">
							<QCol cols="auto">
								<IconSquareButton
									:cy="`media-comparison-details-download-${node.key}`"
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
	</QCardDialog>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { VideoQuality } from '@dto';
import type {
	DownloadMediaDTO,
	PlexMediaComparisonDetailsRowDTO,
	PlexMediaSlimDTO,
	PlexMediaComparisonState,
} from '@dto';
import { DialogType } from '@enums';
import type { QTreeTableColumn } from '@props';
import { QTreeTableColumnType } from '@props';
import { useDialogStore, useMediaStore, useSettingsStore } from '@store';
import { getPlexMediaComparisonState, getPlexMediaComparisonStateFromId } from '@composables';
import type { TreeNode } from 'primevue/treenode';
import type { TreeTableSelectionKeys } from 'primevue/treetable';
import { uniqueId } from 'lodash-es';

interface IComparisonDetailsRow extends Omit<PlexMediaComparisonDetailsRowDTO, 'children'> {
	key: string;
	children: IComparisonDetailsRow[];
}

interface IComparisonDetailTreeNode extends TreeNode {
	data: IComparisonDetailsRow;
	children?: IComparisonDetailTreeNode[];
}

const { t } = useI18n();
const mediaStore = useMediaStore();
const dialogStore = useDialogStore();
const settingsStore = useSettingsStore();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();

const loading = ref(false);
const comparisonRows = ref<IComparisonDetailsRow[]>([]);
const selectedMediaItem = ref<PlexMediaSlimDTO | null>(null);
const selectedRows = ref<TreeTableSelectionKeys>({});

const detailTreeRows = computed(() => mapToTreeNodes(get(comparisonRows)));

const selectedDownloadRows = computed(() => getSelectedDownloadRows(get(comparisonRows)));

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

function mapToTreeNodes(rows: IComparisonDetailsRow[]): IComparisonDetailTreeNode[] {
	return rows.map((row) => ({
		key: row.key,
		label: row.title,
		data: row,
		children: mapToTreeNodes(row.children),
	}));
}

function toComparisonRow(row: PlexMediaComparisonDetailsRowDTO): IComparisonDetailsRow {
	return {
		...row,
		key: uniqueId(`${row.type}-`),
		children: row.children.map(toComparisonRow),
	};
}

function getComparisonState(row: IComparisonDetailsRow): PlexMediaComparisonState {
	return typeof row.state === 'number' ? getPlexMediaComparisonStateFromId(row.state) : row.state;
}

function onOpen(value: unknown) {
	const mediaItem = value as PlexMediaSlimDTO;
	set(selectedMediaItem, mediaItem);
	set(loading, true);
	set(comparisonRows, []);
	set(selectedRows, {});
	useSubscription(
		mediaStore.getMediaComparisonDetails(mediaItem.id, mediaItem.type).subscribe({
			next: (details) => set(comparisonRows, details.rows.map(toComparisonRow)),
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

function getSelectedDownloadRows(rows: IComparisonDetailsRow[]): IComparisonDetailsRow[] {
	return rows.flatMap((row) => {
		const children = getSelectedDownloadRows(row.children ?? []);
		if (!get(selectedRows)[row.key]?.checked)
			return children;

		return [row, ...children];
	});
}

function downloadRows(rows: IComparisonDetailsRow[]) {
	const downloadCommands = rows
		.map(toDownloadMediaCommand);

	if (downloadCommands.length === 0)
		return;

	dialogStore.openMediaConfirmationDownloadDialog(downloadCommands);
}

function toDownloadMediaCommand(row: IComparisonDetailsRow): DownloadMediaDTO {
	return {
		type: row.type,
		mediaIds: [row.plexMediaId],
		plexLibraryId: row.plexLibraryId,
		plexServerId: row.plexServerId,
		qualities: [],
		keepCompletedInDownloadFolder: settingsStore.downloadManagerSettings.keepCompletedInDownloadFolder,
	};
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
