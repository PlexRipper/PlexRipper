<template>
	<TreeTable
		:page-link-size="10"
		:paginator="true"
		:row-hover="true"
		:rows="10"
		:rows-per-page-options="[10, 25, 50, 100]"
		:selection-keys="selected"
		:value="nodes"
		auto-layout
		paginator-position="both"
		scroll-height="flex"
		scrollable
		selection-mode="checkbox"
		size="small"
		@update:selection-keys="onSelectionChange">
		<Column
			expander
			field="title"
			header="Title">
			<template #header>
				<QCheckbox
					:model-value="headerSelected"
					@update:model-value="$emit('all-selected', $event)" />
			</template>
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QMediaTypeIcon
					v-if="node.mediaType"
					:media-type="node.mediaType"
					:size="26" />
				<QText
					:cy="`column-title-${node.id}`"
					:value="node.title" />
			</template>
		</Column>
		<!-- Download Status -->
		<Column
			field="status"
			header="Status"
			style="max-width: 10rem">
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QText
					:cy="`column-status-${node.id}`"
					:value="translateDownloadStatus(node.status)" />
			</template>
		</Column>
		<Column
			field="dataReceived"
			header="Received"
			style="max-width: 10rem">
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QFileSize
					:cy="`column-dataReceived-${node.id}`"
					:size="node.dataReceived" />
			</template>
		</Column>
		<Column
			field="dataTotal"
			header="Size"
			style="max-width: 10rem">
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QFileSize
					:cy="`column-dataTotal-${node.id}`"
					:size="node.dataTotal" />
			</template>
		</Column>
		<Column
			field="downloadSpeed"
			header="Speed"
			style="max-width: 10rem">
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QFileSize
					:cy="`column-downloadSpeed-${node.id}`"
					:size="node.downloadSpeed"
					speed />
			</template>
		</Column>
		<Column
			field="timeRemaining"
			header="ETA"
			style="max-width: 10rem">
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QDuration
					:cy="`column-timeRemaining-${node.id}`"
					:value="node.timeRemaining"
					short />
			</template>
		</Column>
		<Column
			field="percentage"
			header="Percentage"
			style="max-width: 10rem">
			<template #body="{ node }">
				<QProgressBar
					:cy="`column-percentage-${node.id}`"
					:value="node.percentage" />
			</template>
		</Column>
		<Column
			field="actions"
			header="Actions"
			style="max-width: 15rem">
			<template #body="{ node }: { node: IDownloadTableNode }">
				<QRow
					justify="start"
					no-wrap>
					<QCol cols="auto">
						<!-- Item Actions -->
						<IconSquareButton
							v-for="action in node.actions"
							:key="`${node.id}-${kebabCase(action.type)}`"
							:cy="`column-actions-${kebabCase(action.type)}-${node.id}`"
							:disabled="action.disabled"
							:icon="toButtonIcon(action.type)"
							:loading="action.loading"
							dense
							@click.stop="
								$emit('action', {
									action: action.type,
									data: node,
								})
							" />
					</QCol>
				</QRow>
			</template>
		</Column>
	</TreeTable>
</template>

<script lang="ts" setup>
import type { TreeTableSelectionKeys } from 'primevue/treetable';
import type { QTreeViewTableHeader } from '@props';
import type { IDownloadTableNode, IPTreeTableSelectionKeys } from '@interfaces';
import { ButtonType } from '@enums';
import { translateDownloadStatus } from '@composables';
import { kebabCase } from 'lodash-es';
import { DownloadActions } from '@dto';
import Convert from '@class/Convert';

defineProps<{
	nodes: IDownloadTableNode[];
	columns: QTreeViewTableHeader[];
	headerSelected?: boolean | null;
	selected: IPTreeTableSelectionKeys;
	maxSelectionCount?: number;
	notSelectable?: boolean;
}>();

function onSelectionChange(keys: IPTreeTableSelectionKeys) {
	const filtered = Object.fromEntries(
		Object.entries(keys).filter(
			([_, val]: [string, { checked: boolean; partialChecked: boolean }]) => val.checked || val.partialChecked,
		),
	);
	emits('selected', filtered);
}

function toButtonIcon(action: DownloadActions): string {
	switch (action) {
		case DownloadActions.Details:
			return Convert.buttonTypeToIcon(ButtonType.Details);

		case DownloadActions.Delete:
			return Convert.buttonTypeToIcon(ButtonType.Delete);

		case DownloadActions.Start:
			return Convert.buttonTypeToIcon(ButtonType.Start);

		case DownloadActions.Pause:
			return Convert.buttonTypeToIcon(ButtonType.Pause);

		case DownloadActions.Stop:
			return Convert.buttonTypeToIcon(ButtonType.Stop);

		case DownloadActions.Clear:
			return Convert.buttonTypeToIcon(ButtonType.Clear);

		case DownloadActions.Restart:
			return Convert.buttonTypeToIcon(ButtonType.Restart);

		default:
			return Convert.buttonTypeToIcon(ButtonType.None);
	}
}

const emits = defineEmits<{
	(e: 'selected', payload: TreeTableSelectionKeys): void;
	(e: 'update:model-value' | 'all-selected', payload: boolean): void;
	(e: 'action', payload: { action: DownloadActions; data: IDownloadTableNode }): void;
}>();
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;

.p-treetable {
  table {
    white-space: nowrap;
    width: 100%;
  }

  .p-treetable-header {
    color: inherit;
    background: transparent;
    border: none;
  }

  .p-treetable-thead > tr > th {
    color: inherit;
    background: transparent;
    border-top: rgba(255, 255, 255, 0.28) 0.13rem solid;
    border-bottom: rgba(255, 255, 255, 0.28) 0.13rem solid;
  }

  .p-treetable-tbody > tr {
    color: inherit;
    background: transparent;
    //outline: 0.15rem solid white;
    border-bottom: rgba(255, 255, 255, 0.28) 0.13rem solid;

    &:focus {
      outline: none;
    }
  }

  .p-checkbox {
    .p-checkbox-box {
      border-width: 2px;
      border-color: rgba(255, 255, 255, 0.69);
      border-radius: 0.125rem;

    }

    &[data-p-checked="false"][data-p-partialchecked="false"]:hover {
      .p-checkbox-box {
        border-color: $primary;
      }
    }

    .p-checkbox-icon {
      display: none !important;
    }

    // CHECKED: Quasar checkmark
    &[data-p-checked="true"] {
      .p-checkbox-box {
        border-color: transparent;

        &::after {
          content: "";
          position: absolute;
          inset: 0;
          background-color: currentColor;
          -webkit-mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath fill='none' stroke='%23000' stroke-width='3' stroke-linecap='round' stroke-linejoin='round' d='M1.73 12.91 8.1 19.28 22.79 4.59'/%3E%3C/svg%3E") no-repeat center / 70% 70%;
          mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath fill='none' stroke='%23000' stroke-width='3' stroke-linecap='round' stroke-linejoin='round' d='M1.73 12.91 8.1 19.28 22.79 4.59'/%3E%3C/svg%3E") no-repeat center / 70% 70%;
        }
      }
    }

    // PARTIAL: Quasar indeterminate bar
    &[data-p-partialchecked="true"] {
      .p-checkbox-box {
        border-color: transparent;
        background-color: var(--p-checkbox-checked-background);

        &::after {
          content: "";
          position: absolute;
          inset: 0;
          background-color: currentColor;
          -webkit-mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M4 14H20V10H4Z'/%3E%3C/svg%3E") no-repeat center / 75% 75%;
          mask: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M4 14H20V10H4Z'/%3E%3C/svg%3E") no-repeat center / 75% 75%;
        }
      }
    }
  }

  .p-paginator {
    color: inherit;
    background: transparent;
  }
}

//this creates a pseudochild of the button the size of the first anscestor with "relative" size
button.p-treetable-toggler.p-link::before {
  content: '';
  display: block;
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

//this element originally had "relative" size, overwriting it allow the pseudochild to be sized relative to a later anscestor
.p-treetable-toggler {
  position: static;
}

// this element contains the full row, by making it relative the pseudochild can size itself based on this
.p-treetable .p-treetable-tbody > tr {
  position: relative;
}
</style>
