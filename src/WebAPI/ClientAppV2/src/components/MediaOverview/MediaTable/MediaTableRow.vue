<template>
	<q-row class="media-table-row" full-height align="center" justify="between">
		<template v-if="row">
			<!-- Checkbox -->
			<q-col v-if="selectable" cols="auto" class="q-ml-md q-pl-sm">
				<q-checkbox dense :model-value="selected" @update:model-value="$emit('selected', $event)" />
			</q-col>
			<template v-for="(column, i) in columns" :key="i">
				<!-- Index -->
				<template v-if="column['type'] === 'index'">
					<q-col cols="auto" class="media-table-row--column">
						<span> #{{ index + 1 }} </span>
					</q-col>
				</template>
				<!-- Title -->
				<template v-else-if="column['type'] === 'title'">
					<q-col class="media-table-row--title media-table-row--column">
						<span>{{ row[column.field] }} </span>
					</q-col>
				</template>
				<!-- Duration format -->
				<template v-else-if="column['type'] === 'duration'">
					<q-col cols="1" class="media-table-row--column">
						<QDuration short :value="row[column.field]" />
					</q-col>
				</template>
				<!-- Date format -->
				<template v-else-if="column['type'] === 'date'">
					<q-col cols="1" class="media-table-row--column">
						<QDateTime short-date :text="row[column.field]" />
					</q-col>
				</template>
				<!-- Filesize format -->
				<template v-else-if="column['type'] === 'file-size'">
					<q-col cols="1" class="media-table-row--column">
						<QFileSize :size="row[column.field]" />
					</q-col>
				</template>
				<!-- Actions -->
				<template v-else-if="column['type'] === 'actions'">
					<q-col cols="auto" class="media-table-row--column">
						<q-btn
							flat
							:icon="Convert.buttonTypeToIcon(ButtonType.Download)"
							@click="$emit('action', { action: 'download', data: row })" />
					</q-col>
				</template>
			</template>
		</template>
		<!-- No row -->
		<q-col v-else>{{ $t('components.q-tree-view-table-row.invalid-node') }}</q-col>
	</q-row>
</template>

<script setup lang="ts">
import { defineEmits, defineProps } from 'vue';
import { QTreeViewTableHeader, QTreeViewTableItem } from '@props';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import Convert from '@class/Convert';
import ButtonType from '@enums/buttonType';

defineProps<{
	selected?: boolean | null;
	isHeader?: boolean;
	selectable?: boolean;
	row: PlexMediaSlimDTO;
	columns: QTreeViewTableHeader[];
	index: number;
}>();

defineEmits<{
	(e: 'selected', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.media-table-row-container,
.media-table-row-container > div {
	height: 42px;
}

.media-table-row {
	border-bottom: 1px solid;

	&--column {
		text-align: center;
		margin: auto 8px;
	}

	&--title {
		font-weight: bold;
		text-align: left;
		display: inline-block;
		text-overflow: ellipsis;
		white-space: nowrap;
		overflow: hidden;
	}
}

.body--dark {
	.media-table-row {
		border-bottom-color: $separator-dark-color;
	}
}

.body--light {
	.media-table-row {
		border-bottom-color: $separator-color;
	}
}
</style>
