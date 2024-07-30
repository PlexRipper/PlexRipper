<template>
	<q-row
		align="center"
		justify="between"
		class="media-table-header"
	>
		<q-col
			v-if="selectable"
			cols="auto"
			class="media-table-header--column"
		>
			<q-checkbox
				dense
				:model-value="selected"
				class="q-ml-md"
				@update:model-value="$emit('selected', $event)"
			/>
		</q-col>
		<template
			v-for="(column, i) in columns"
			:key="i"
		>
			<!-- Index -->
			<template v-if="column['type'] === 'index'">
				<q-col
					cols="auto"
					style="min-width: 45px"
					class="media-table-header--column"
				>
					<MediaTableHeaderColumn :column="column" />
				</q-col>
			</template>
			<!-- Title -->
			<template v-else-if="column['type'] === 'title'">
				<q-col class="media-table-header--title media-table-header--column">
					<MediaTableHeaderColumn :column="column" />
				</q-col>
			</template>
			<!-- Duration format -->
			<template v-else-if="column['type'] === 'duration'">
				<q-col
					cols="1"
					class="media-table-header--column"
				>
					<MediaTableHeaderColumn :column="column" />
				</q-col>
			</template>
			<!-- Date format -->
			<template v-else-if="column['type'] === 'date'">
				<q-col
					cols="1"
					class="media-table-header--column"
				>
					<MediaTableHeaderColumn :column="column" />
				</q-col>
			</template>
			<!-- Filesize format -->
			<template v-else-if="column['type'] === 'file-size'">
				<q-col
					cols="1"
					class="media-table-header--column"
				>
					<MediaTableHeaderColumn :column="column" />
				</q-col>
			</template>
			<!-- Actions -->
			<template v-else-if="column['type'] === 'actions'">
				<q-col
					cols="auto"
					class="media-table-header--column"
				>
					<MediaTableHeaderColumn :column="column" />
				</q-col>
			</template>
		</template>
	</q-row>
</template>

<script setup lang="ts">
import type { QTreeViewTableHeader, QTreeViewTableItem } from '@props';

defineProps<{
	selected?: boolean | null;
	selectable?: boolean;
	columns: QTreeViewTableHeader[];
}>();

defineEmits<{
	(e: 'selected', payload: boolean): void;
	(e: 'action', payload: { action: string; data: QTreeViewTableItem }): void;
}>();
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';

.media-table-header {
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
	.media-table-header {
		border-bottom-color: $separator-dark-color;
	}
}

.body--light {
	.media-table-header {
		border-bottom-color: $separator-color;
	}
}
</style>
