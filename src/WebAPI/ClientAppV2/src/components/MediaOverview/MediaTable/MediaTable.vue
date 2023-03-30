<!--suppress VueUnrecognizedSlot -->
<template>
	{{ pagination }}
	<QTable
		v-model:pagination="pagination"
		:selected="getSelected"
		style="height: 400px"
		selection="multiple"
		:row-key="rowKey"
		:rows="rows"
		:loading="loading"
		:columns="mediaTableColumns"
		hide-bottom
		virtual-scroll
		:rows-per-page-options="[0]"
		:virtual-scroll-item-size="48"
		@virtual-scroll="onVirtualScroll"
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
	</QTable>
</template>

<script setup lang="ts">
import { defineProps, ref, withDefaults, defineEmits } from 'vue';
import Log from 'consola';
import { QTable } from 'quasar';
import { toDownloadMedia } from '#imports';
import type { PlexLibraryDTO, PlexMediaDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import Convert from '@class/Convert';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { DownloadMediaDTO, PlexMediaSlimDTO } from '@dto/mainApi';

defineOptions({
	inheritAttrs: false,
});

const mediaTableColumns = getMediaTableColumns();

const props = withDefaults(
	defineProps<{
		rows: PlexMediaSlimDTO[];
		library: PlexLibraryDTO | null;
		loading: boolean;
		rowKey: keyof PlexMediaSlimDTO;
		selected?: string[] | number[];
	}>(),
	{
		loading: true,
		selected: () => [],
	},
);

const pageSize = 20;
const totalRows = computed(() => props.library?.count ?? pageSize);
const lastPage = computed(() => Math.ceil(totalRows.value / pageSize));
const loading = ref(false);
const nextPage = ref(1);
const pagination = ref({
	rowsPerPage: 0,
});

const onRowClick = (row) => alert(`${row.title} clicked`);

const emit = defineEmits<{
	(e: 'download', downloadMedia: DownloadMediaDTO[]): void;
	(e: 'selection', payload: { allSelected: boolean | null; selection: string[] }): void;
	(e: 'request-media', request: { page: number; size: number; refresh: () => void }): void;
}>();

const downloadMedia = (row: PlexMediaDTO) => {
	emit('download', toDownloadMedia(row));
};

const onVirtualScroll = (payload: { index: number; from: number; to: number; direction: string; ref: QTable }) => {
	const size = pageSize;
	//	const page = ((payload.to + 1) % size) + (direction === 'increase' ? 1 : -1);

	const next = nextPage.value;
	const last = lastPage.value;
	const currentPage = pageSize * (next - 1);
	const lastIndex = currentPage - 1;

	if (loading.value !== true && next < last && payload.to === lastIndex) {
		loading.value = true;

		emit('request-media', {
			page: nextPage.value,
			size,
			refresh: () => {
				nextPage.value++;
				nextTick(() => {
					// @ts-ignore
					payload.ref?.refresh();
					loading.value = false;
				});
			},
		});
	}
};

const getSelected = computed((): PlexMediaSlimDTO[] => {
	if (!props.selected) {
		return [];
	}
	return props.rows.filter((row) => props.selected.includes(row[props.rowKey]));
});

const updateSelected = (selected: PlexMediaDTO[]) => {
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

//.my-sticky-dynamic {
//	/* height or max-height is important */
//	height: 410px;
//
//	.q-table__top,
//	.q-table__bottom,
//	thead tr:first-child th {
//		/* bg color is important for th; just specify one */
//		background-color: transparent;
//		backdrop-filter: blur(50px);
//	}
//
//	thead tr th {
//		position: sticky;
//		z-index: 1;
//	}
//
//	/* this will be the loading indicator */
//
//	thead tr:last-child th {
//		/* height of all previous header rows */
//		top: 48px;
//	}
//
//	thead tr:first-child th {
//		top: 0;
//	}
//
//	/* prevent scrolling behind sticky top row on focus */
//
//	tbody {
//		/* height of all previous header rows */
//		scroll-margin-top: 48px;
//	}
//}
</style>
