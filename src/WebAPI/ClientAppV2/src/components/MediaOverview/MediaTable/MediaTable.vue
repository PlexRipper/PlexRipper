<!--suppress VueUnrecognizedSlot -->
<template>
	<QTable
		ref="qTableRef"
		:selected="getSelected"
		selection="multiple"
		row-key="id"
		:rows="rows"
		:loading="loading"
		:columns="mediaTableColumns"
		hide-bottom
		virtual-scroll
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
	</QTable>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineProps, ref, withDefaults, defineEmits } from 'vue';
import { QTable } from 'quasar';
import { toDownloadMedia } from '#imports';
import type { PlexLibraryDTO, PlexMediaDTO } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import Convert from '@class/Convert';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { DownloadMediaDTO, PlexMediaSlimDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';

const mediaTableColumns = getMediaTableColumns();
const qTableRef = ref<QTable | null>(null);

const props = withDefaults(
	defineProps<{
		rows: PlexMediaSlimDTO[];
		library: PlexLibraryDTO | null;
		selection: ISelection;
		scrollDict: Record<string, number>;
	}>(),
	{},
);

const loading = ref(false);

const onRowClick = (row) => alert(`${row.title} clicked`);

const emit = defineEmits<{
	(e: 'download', downloadMedia: DownloadMediaDTO[]): void;
	(e: 'selection', payload: ISelection): void;
	(e: 'request-media', request: { page: number; size: number; refresh: () => void }): void;
}>();

const downloadMedia = (row: PlexMediaDTO) => {
	emit('download', toDownloadMedia(row));
};

/**
 * The selected rows cannot be returned as just keys, they need to be the same object as the rows.
 */
const getSelected = computed((): PlexMediaSlimDTO[] => {
	return props.rows.filter((row) => props.selection.keys.includes(row.id));
});

const updateSelected = (selected: PlexMediaSlimDTO[]) => {
	emit('selection', {
		keys: selected.map((x) => x.id) as number[],
		allSelected: selected.length === props.rows.length ? true : selected.length === 0 ? false : null,
		indexKey: 0,
	});
};

const scrollToIndex = (letter: string) => {
	if (!qTableRef.value) {
		Log.error('qTableRef is null');
		return;
	}

	const value = props.scrollDict[letter];
	qTableRef.value?.scrollTo(value, 'start-force');
};

defineExpose({
	scrollToIndex,
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

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
