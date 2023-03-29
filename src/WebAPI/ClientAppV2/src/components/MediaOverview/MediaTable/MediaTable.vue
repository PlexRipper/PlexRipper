<!--suppress VueUnrecognizedSlot -->
<template>
	<q-table
		:selected="getSelected"
		selection="multiple"
		:row-key="rowKey"
		:rows="rows"
		:loading="loading"
		:columns="mediaTableColumns"
		virtual-scroll
		hide-bottom
		:rows-per-page-options="[0]"
		@update:selected="updateSelected($event)">
		<!-- Title -->
		<template #body-cell-title="{ row }">
			<q-td class="row-title" @click="onRowClick(row)">
				<q-hover>
					<template #default>
						{{ row.title }}
					</template>
				</q-hover>
			</q-td>
		</template>
		<!-- Media size -->
		<template #body-cell-size="{ row }">
			<q-td>
				<QFileSize :size="row.mediaSize" />
			</q-td>
		</template>
		<!-- Added At Date format -->
		<template #body-cell-addedAt="{ row }">
			<q-td>
				<QDateTime :text="row.addedAt" short-date />
			</q-td>
		</template>
		<!-- Updated At Date format -->
		<template #body-cell-updatedAt="{ row }">
			<q-td>
				<QDateTime :text="row.updatedAt" short-date />
			</q-td>
		</template>
		<!-- Actions -->
		<template #body-cell-actions="{ row }">
			<q-td>
				<q-btn flat :icon="Convert.buttonTypeToIcon(ButtonType.Download)" @click="downloadMedia(row)" />
			</q-td>
		</template>
		<template #loading>
			<q-inner-loading showing color="red" />
		</template>
	</q-table>
</template>

<script setup lang="ts">
import { defineProps, ref, withDefaults, defineEmits } from 'vue';
import Log from 'consola';
import type { PlexMediaDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import Convert from '@class/Convert';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';

defineOptions({
	inheritAttrs: false,
});

const props = withDefaults(
	defineProps<{
		rows: PlexMediaDTO[];
		loading: boolean;
		rowKey: string;
		selected?: string[] | number[];
	}>(),
	{
		loading: true,
		selected: () => [],
	},
);

const mediaTableColumns = getMediaTableColumns();

const onRowClick = (row) => alert(`${row.title} clicked`);

const emit = defineEmits<{
	(e: 'download', visible: boolean[]): void;
	(e: 'selection', payload: { allSelected: boolean | null; selection: string[] }): void;
	(e: 'request-media', visible: boolean[]): void;
}>();

const downloadMedia = (row: PlexMediaDTO) => {
	alert('download');
};

const getSelected = computed((): PlexMediaDTO[] => {
	if (!props.selected) {
		return [];
	}
	return props.rows.filter((row) => props.selected.includes(row[props.rowKey]));
});

const updateSelected = (selected: PlexMediaDTO[]) => {
	Log.info('updateSelected', selected);
	emit('selection', {
		selection: selected.map((row) => row[props.rowKey]),
		allSelected: selected.length === props.rows.length ? true : selected.length === 0 ? false : null,
	});
};
</script>

<style lang="scss">
@import './src/assets/scss/_variables.scss';

.row-title {
	cursor: pointer;
	font-weight: bold;

	:hover {
		color: $primary;
	}
}
</style>
