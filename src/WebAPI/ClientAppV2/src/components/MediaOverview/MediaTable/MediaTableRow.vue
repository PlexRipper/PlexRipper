<template>
	<q-row class="media-table-row" full-height align="center" justify="between">
		<template v-if="row">
			<!-- Checkbox -->
			<q-col v-if="selectable" cols="auto" class="q-ml-md q-pl-sm">
				<q-checkbox dense :model-value="selected" @update:model-value="$emit('selected', row)" />
			</q-col>
			<template v-for="(column, i) in columns" :key="i">
				<!-- Index -->
				<template v-if="column['type'] === 'index'">
					<q-col cols="auto" style="min-width: 50px" class="media-table-row--column">
						<span> #{{ index + 1 }} </span>
					</q-col>
				</template>
				<!-- Title -->
				<template v-else-if="column['type'] === 'title'">
					<q-col
						:class="[
							'media-table-row--column',
							'media-table-row--title',
							!disableHoverClick ? 'media-table-row--title--hover' : '',
						]"
						@click.stop="!disableHoverClick ? onRowAction({ command: 'open-details' }) : () => {}">
						<span>
							{{ row[column.field] }}
						</span>
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
				<!-- Media size -->
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
							@click.stop="onRowAction({ command: 'download' })" />
					</q-col>
				</template>
			</template>
		</template>
		<!-- No row -->
		<q-col v-else>{{ $t('components.q-tree-view-table-row.invalid-node') }}</q-col>
		<!--	Highlight animation effect	-->
		<svg v-if="!disableHighlight" class="glow-container">
			<!--suppress HtmlUnknownAttribute -->
			<rect pathLength="100" height="5" width="5" stroke-linecap="round" class="glow-blur" />
			<!--suppress HtmlUnknownAttribute -->
			<rect pathLength="100" height="5" width="5" stroke-linecap="round" class="glow-line" />
		</svg>
	</q-row>
</template>

<script setup lang="ts">
import { defineEmits, defineProps } from 'vue';
import { QTreeViewTableHeader } from '@props';
import { PlexMediaSlimDTO } from '@dto/mainApi';
import Convert from '@class/Convert';
import ButtonType from '@enums/buttonType';
import {
	IMediaOverviewCommands,
	sendMediaOverviewDownloadCommand,
	sendMediaOverviewOpenDetailsCommand
} from '@composables/event-bus';
import { toDownloadMedia } from '@composables/conversion';

const props = defineProps<{
	selected?: boolean | null;
	isHeader?: boolean;
	selectable?: boolean;
	row: PlexMediaSlimDTO;
	columns: QTreeViewTableHeader[];
	index: number;
	disableHoverClick?: boolean;
	disableHighlight: boolean;
}>();

defineEmits<{
	(e: 'selected', payload: PlexMediaSlimDTO): void;
}>();

function onRowAction(action: IMediaOverviewCommands) {
	switch (action.command) {
		case 'download':
			sendMediaOverviewDownloadCommand(toDownloadMedia[props.row]);
			break;
		case 'open-details':
			sendMediaOverviewOpenDetailsCommand(props.row.id);
			break;
		default:
			throw new Error(`Unknown action: ${action.command} in MediaTableRow.vue`);
	}
}
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
		white-space: nowrap;
		margin: auto 8px;
	}

	&--title {
		font-weight: bold;
		text-align: left;
		display: inline-block;
		text-overflow: ellipsis;
		white-space: nowrap;
		overflow: hidden;

		&--hover {
			cursor: pointer;

			&:hover {
				color: $primary;
			}
		}
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
