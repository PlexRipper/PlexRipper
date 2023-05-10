<template>
	<div class="media-table" data-cy="media-table">
		<MediaTableHeader :columns="mediaTableColumns" selectable class="media-table--header" />
		<div ref="qTableRef" class="media-table--content scroll" data-cy="media-table-scroll">
			<q-intersection v-for="(row, index) in rows" :key="index" class="media-table--intersection">
				<MediaTableRow :index="index" :columns="mediaTableColumns" :row="row" selectable />
			</q-intersection>
		</div>
	</div>
	<div v-if="false">
		<QTable
			v-model:pagination="pagination"
			style="height: 800px"
			:selected="getSelected"
			selection="multiple"
			row-key="id"
			:rows="rows"
			:columns="mediaTableColumns"
			hide-bottom
			class="table-class"
			@update:selected="updateSelected($event)"
			@virtual-scroll="onScroll">
			<!-- Title -->
			<template #body-cell-title="{ row }">
				<q-td v-if="!disableHoverClick" class="row-title--hover text-eclipse" @click="$emit('row-click', row)">
					<q-hover>
						<template #default>
							{{ row.title }}
						</template>
					</q-hover>
				</q-td>
				<q-td v-else class="row-title text-eclipse">
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
			<!-- Media size -->
			<template #body-cell-size="{ row }">
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
						<QDateTime :text="row.addedAt" short-date />
					</span>
				</q-td>
			</template>
			<!-- Updated At Date format -->
			<template #body-cell-updatedAt="{ row }">
				<q-td class="text-center">
					<span class="q-mr-md">
						<QDateTime :text="row.updatedAt" short-date />
					</span>
				</q-td>
			</template>
			<!-- Actions -->
			<template #body-cell-actions="{ row }">
				<q-td class="text-center">
					<q-btn flat :icon="Convert.buttonTypeToIcon(ButtonType.Download)" @click="downloadMedia(row)" />
				</q-td>
			</template>
			<template #loading>
				<q-inner-loading showing color="red" />
			</template>
		</QTable>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { computed, defineEmits, defineProps, ref, withDefaults } from 'vue';
import { QTable } from 'quasar';
import { get, set, useScroll } from '@vueuse/core';
import { toDownloadMedia, useProcessDownloadCommandBus } from '#imports';
import ButtonType from '@enums/buttonType';
import Convert from '@class/Convert';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';

const mediaTableColumns = getMediaTableColumns();
const qTableRef = ref<HTMLElement | null>(null);

const pagination = ref({
	rowsPerPage: 0,
});
const props = withDefaults(
	defineProps<{
		rows: PlexMediaSlimDTO[];
		selection: ISelection | null;
		scrollDict?: Record<string, number>;
		disableHoverClick?: boolean;
	}>(),
	{
		scrollDict: { '#': 0 } as any,
	},
);

const loading = ref(false);

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
	const { y } = useScroll(get(qTableRef), {
		behavior: 'auto',
		offset: {
			bottom: 184,
		},
	});

	// This functions as a reset of table sorting
	// qTableRef.value?.setPagination({ page: 1, sortBy: mediaTableColumns[0].field, descending: false, rowsPerPage: 0 });
	set(y, props.scrollDict[letter] * (42 + 1));
	// qTableRef.value?.scrollTo(value, 'start');
};

// region EventBus

const processDownloadCommandBus = useProcessDownloadCommandBus();
const downloadMedia = (row: PlexMediaSlimDTO) => {
	processDownloadCommandBus.emit(toDownloadMedia(row));
};
// endregion

defineExpose({
	scrollToIndex,
});
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.media-table {
	overflow-y: auto;

	&--header,
	&--intersection,
	&--intersection > div {
		height: $media-table-row-height;
	}

	&--content {
		max-height: calc(100vh - $app-bar-height - $media-overview-bar-height - $media-table-row-height);
	}
}

.row-title {
	font-weight: bold;
	min-width: 300px;
	max-width: 300px;

	&--hover {
		cursor: pointer;

		:hover {
			color: $primary;
		}
	}
}
</style>
