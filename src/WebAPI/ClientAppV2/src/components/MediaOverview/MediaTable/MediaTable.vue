<!--suppress VueUnrecognizedSlot -->
<template>
	<q-table
		v-model:selected="selected"
		selection="multiple"
		row-key="title"
		:rows="rows"
		:loading="loading"
		:columns="getHeaders"
		virtual-scroll
		:rows-per-page-options="[0]">
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
import { defineProps, ref, computed, defineEmits } from 'vue';
import { QTableColumnProps } from '@props';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import Convert from '@class/Convert';
import { MediaService } from '@service';

defineOptions({
	inheritAttrs: false,
});

const props = defineProps<{
	libraryId: number;
	mediaType: PlexMediaType;
}>();

const loading = ref(true);
const selected = ref<string[]>([]);

const rows = ref<PlexMediaDTO[]>([]);
const onRowClick = (row) => alert(`${row.title} clicked`);

defineEmits<{
	(e: 'download', visible: boolean[]): void;
	(e: 'selected', visible: boolean[]): void;
	(e: 'request-media', visible: boolean[]): void;
}>();

const getHeaders = computed<QTableColumnProps[]>(() => {
	return [
		{
			label: 'Title',
			field: 'title',
			name: 'title',
			align: 'left',
			sortable: true,
			required: true,
		},
		{
			label: 'Year',
			name: 'year',
			field: 'year',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Size',
			field: 'mediaSize',
			name: 'size',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Added At',
			name: 'addedAt',
			field: 'addedAt',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Updated At',
			name: 'updatedAt',
			field: 'updatedAt',
			align: 'left',
			sortable: true,
		},
		{
			label: 'Actions',
			name: 'actions',
			field: 'actions',
			align: 'left',
			required: true,
		},
	];
});

const downloadMedia = (row: PlexMediaDTO) => {
	alert('download');
};

onMounted(() => {
	useSubscription(
		MediaService.getMediaData(props.libraryId).subscribe((data) => {
			rows.value = data;
			loading.value = false;
		}),
	);
});
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
