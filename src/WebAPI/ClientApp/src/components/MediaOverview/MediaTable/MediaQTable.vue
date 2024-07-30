<template>
	<QTable
		:selected="getSelected"
		selection="multiple"
		row-key="id"
		:columns="qTableProps.columns"
		:rows="rows"
		:rows-per-page-options="[0]"
		hide-pagination
		flat
		@update:selected="updateSelected($event as PlexMediaSlimDTO[])"
	>
		<!-- Title -->
		<template #body-cell-title="{ row }">
			<q-td class="row-title text-eclipse">
				{{ row.title }}
			</q-td>
		</template>
		<!-- Media size -->
		<template #body-cell-year="{ row }">
			<q-td class="text-center">
				<span class="q-mr-md">
					{{ row.year }}
				</span>
			</q-td>
		</template>
		<!-- Duration -->
		<template #body-cell-duration="{ row }">
			<q-td class="text-center">
				<QDuration
					short
					:value="row.duration"
				/>
			</q-td>
		</template>
		<!-- Media size -->
		<template #body-cell-mediaSize="{ row }">
			<q-td class="text-center">
				<span class="q-mr-md">
					<QFileSize :size="row.mediaSize" />
				</span>
			</q-td>
		</template>
		<!-- Added At Date format -->
		<template #body-cell-addedAt="{ row }">
			<q-td class="text-center">
				<span class="q-mr-md">
					<QDateTime
						:text="row.addedAt"
						short-date
					/>
				</span>
			</q-td>
		</template>
		<!-- Updated At Date format -->
		<template #body-cell-updatedAt="{ row }">
			<q-td class="text-center">
				<span class="q-mr-md">
					<QDateTime
						:text="row.updatedAt"
						short-date
					/>
				</span>
			</q-td>
		</template>
		<!-- Actions -->
		<template #body-cell-actions="{ row }">
			<q-td class="text-center">
				<q-btn
					flat
					:icon="Convert.buttonTypeToIcon(ButtonType.Download)"
					@click.stop="onRowAction(row, { command: 'download' })"
				/>
			</q-td>
		</template>
	</QTable>
</template>

<script setup lang="ts">
import type { QTableProps } from 'quasar';
import Convert from '@class/Convert';
import ButtonType from '@enums/buttonType';
import type { PlexMediaSlimDTO } from '@dto';
import type { ISelection } from '@interfaces';
import { getMediaTableColumns } from '@composables/mediaTableColumns';
import {
	type IMediaOverviewCommands,
	sendMediaOverviewDownloadCommand,
	sendMediaOverviewOpenDetailsCommand,
} from '@composables/event-bus';
import { toDownloadMedia } from '@composables/conversion';

const mediaTableColumns = getMediaTableColumns();

const props = defineProps<{
	rows: PlexMediaSlimDTO[];
	selection: ISelection | null;
}>();

const emit = defineEmits<{
	(e: 'selection', payload: ISelection): void;
	(e: 'row-click', payload: PlexMediaSlimDTO): void;
}>();

/**
 * The selected rows cannot be returned as just keys, they need to be the same object as the rows.
 */
const getSelected = computed((): PlexMediaSlimDTO[] => {
	return props.rows.filter((row) => (props.selection?.keys ?? []).includes(row.id));
});

const qTableProps = computed((): QTableProps => {
	return {
		rows: [],
		columns: mediaTableColumns.map((x) => {
			return {
				label: x.label,
				field: x.field,
				name: x.field,
				align: x.align,
				type: x.type,
				sortable: x.sortable,
			};
		}),
	};
});

function onRowAction(row: PlexMediaSlimDTO, action: IMediaOverviewCommands) {
	switch (action.command) {
		case 'download':
			sendMediaOverviewDownloadCommand(toDownloadMedia(row));
			break;
		case 'open-details':
			sendMediaOverviewOpenDetailsCommand(row.id);
			break;
		default:
			throw new Error(`Unknown action: ${action.command} in MediaQTable.vue`);
	}
}

function updateSelected(selected: PlexMediaSlimDTO[]) {
	emit('selection', {
		keys: selected.map((x) => x.id) as number[],
		allSelected: selected.length === props.rows.length ? true : selected.length === 0 ? false : null,
		indexKey: 0,
	});
}
</script>
