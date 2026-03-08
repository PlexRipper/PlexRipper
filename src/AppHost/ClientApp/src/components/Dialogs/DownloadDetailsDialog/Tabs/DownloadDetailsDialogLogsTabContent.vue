<template>
	<div class="logs-panel">
		<!-- Logs Toolbar -->
		<q-toolbar
			class="q-px-none q-pb-xs">
			<!-- Log Copy -->
			<q-btn
				dense
				flat
				icon="mdi-content-copy"
				round
				@click="copyLogs">
				<q-tooltip>{{ t('components.download-details-dialog.logs.copy-all') }}</q-tooltip>
			</q-btn>
			<!-- Log Sort -->
			<q-btn
				:icon="sortAsc ? 'mdi-sort-ascending' : 'mdi-sort-descending'"
				dense
				flat
				round
				@click="toggleSort">
				<q-tooltip>{{ sortAsc ? t('components.download-details-dialog.logs.sort-oldest-first') : t('components.download-details-dialog.logs.sort-newest-first') }}</q-tooltip>
			</q-btn>
			<!-- Log Filter -->
			<q-btn
				dense
				flat
				icon="mdi-filter-outline"
				round>
				<q-tooltip>{{ t('components.download-details-dialog.logs.filter-by-level') }}</q-tooltip>
				<q-menu auto-close>
					<q-list dense>
						<q-item
							v-for="level in allLevels"
							:key="level"
							clickable
							@click.stop="toggleLevel(level)">
							<q-item-section avatar>
								<q-checkbox
									:model-value="activeFilters.includes(level)"
									:val="level"
									dense
									@update:model-value="toggleLevel(level)" />
							</q-item-section>
							<q-item-section avatar>
								<q-icon
									:color="Convert.logLevelToColor(level)"
									:name="Convert.logLevelToIcon(level)"
									size="xs" />
							</q-item-section>
							<q-item-section>{{ logLevelLabels[level] }}</q-item-section>
						</q-item>
					</q-list>
				</q-menu>
			</q-btn>
			<!-- Spacer -->
			<q-toolbar-title />
			<!-- Delete Logs -->
			<q-btn
				dense
				flat
				icon="mdi-delete-sweep"
				round
				@click="dialogStore.openDialog(DialogType.DownloadDetailsDeleteLogsConfirmationDialog)">
				<q-tooltip>{{ t('components.download-details-dialog.logs.delete-all') }}</q-tooltip>
			</q-btn>
		</q-toolbar>

		<!-- Logs Virtual List -->
		<div
			ref="scrollEl"
			class="logs-scroll-container">
			<!-- Empty State Banner -->
			<template v-if="!logsLoading && filteredLogs.length === 0">
				<QAlert type="info">
					{{ t('components.download-details-dialog.logs.no-logs') }}
				</QAlert>
			</template>
			<template v-else>
				<!-- Total height spacer — required by TanStack Virtual -->
				<div :style="{ height: `${virtualizer.getTotalSize()}px`, position: 'relative' }">
					<div
						v-for="row in virtualizer.getVirtualItems()"
						:key="String(row.key)"
						:ref="el => measureRow(el as Element | null, row)"
						:style="{
							position: 'absolute',
							top: 0,
							left: 0,
							width: '100%',
							transform: `translateY(${row.start}px)`,
						}">
						<QTimeline
							class="log-timeline"
							color="grey-6"
							layout="dense">
							<QTimelineEntry
								:color="Convert.logLevelToColor(getLogAtIndex(row.index).logLevel)"
								:icon="Convert.logLevelToIcon(getLogAtIndex(row.index).logLevel)"
								:title="translateDownloadStatus(getLogAtIndex(row.index).status)">
								<template #subtitle>
									<QDateTime
										:text="getLogAtIndex(row.index).createdAt"
										short-date
										time />
								</template>
								<QText :value="getLogAtIndex(row.index).message" />
							</QTimelineEntry>
						</QTimeline>
					</div>
				</div>
			</template>
			<!-- Loading Indicator -->
			<QRow
				v-if="logsLoading"
				align="center"
				column>
				<QCol cols="auto">
					<QSpinnerDots
						color="primary"
						size="40px" />
				</QCol>
			</QRow>
		</div>

		<!-- Delete Logs Confirmation Dialog -->
		<ConfirmationDialog
			:name="DialogType.DownloadDetailsDeleteLogsConfirmationDialog"
			:title="t('components.download-details-dialog.logs.delete-confirmation.title')"
			:text="t('components.download-details-dialog.logs.delete-confirmation.text')"
			@confirm="deleteLogs" />
	</div>
</template>

<script lang="ts" setup>
import { get, set, useClipboard } from '@vueuse/core';
import { useVirtualizer, type VirtualItem } from '@tanstack/vue-virtual';
import { format } from 'date-fns';
import { downloadApi } from '@api';
import type { DownloadTaskDTO, DownloadTaskLogDTO } from '@dto';
import { NotificationLevel } from '@dto';
import Convert from '@class/Convert';
import { translateDownloadStatus, showSuccessNotification } from '@composables';
import { DialogType } from '@enums';
import { useDialogStore } from '@store';

const INITIAL_TAKE = 50;

const { t } = useI18n();
const { copy } = useClipboard({ legacy: true });
const dialogStore = useDialogStore();

const props = defineProps<{
	downloadTaskId: string;
	downloadTask?: DownloadTaskDTO;
	initialLogs?: DownloadTaskLogDTO[];
}>();

const emit = defineEmits<{
	(e: 'logs-deleted'): void;
	(e: 'logs-refreshed', logs: DownloadTaskLogDTO[]): void;
}>();

const scrollEl = ref<HTMLElement | null>(null);
const sortAsc = ref(true);
const activeFilters = ref<NotificationLevel[]>([...Object.values(NotificationLevel).filter((v) => v !== NotificationLevel.None)]);
const logs = ref<DownloadTaskLogDTO[]>([]);
const logsLoading = ref(false);
const highestSeenId = ref<number | undefined>(undefined);
const logRefreshTimer = useIntervalFn(() => refreshLogs(), 1000, { immediate: false });

// Sync initial logs from parent pre-fetch (first 50)
watch(
	() => props.initialLogs,
	(newLogs) => {
		if (newLogs && newLogs.length > 0 && get(logs).length === 0) {
			set(logs, [...newLogs]);
			const maxId = Math.max(...newLogs.map((l) => l.id));
			set(highestSeenId, maxId);
		}
	},
	{ immediate: true },
);

const allLevels = Object.values(NotificationLevel).filter((v) => v !== NotificationLevel.None && v !== NotificationLevel.Verbose);

const logLevelLabels = computed<Record<NotificationLevel, string>>(() => ({
	[NotificationLevel.None]: '',
	[NotificationLevel.Verbose]: '',
	[NotificationLevel.Debug]: t('components.download-details-dialog.logs.levels.debug'),
	[NotificationLevel.Information]: t('components.download-details-dialog.logs.levels.information'),
	[NotificationLevel.Success]: t('components.download-details-dialog.logs.levels.success'),
	[NotificationLevel.Warning]: t('components.download-details-dialog.logs.levels.warning'),
	[NotificationLevel.Error]: t('components.download-details-dialog.logs.levels.error'),
	[NotificationLevel.Fatal]: t('components.download-details-dialog.logs.levels.fatal'),
}));

const filteredLogs = computed(() => {
	const filtered = get(logs).filter((item) => get(activeFilters).includes(item.logLevel));
	return [...filtered].sort((a, b) => {
		const diff = a.id - b.id;
		return get(sortAsc) ? diff : -diff;
	});
});

// TanStack Virtual — variable height rows via measureElement
const virtualizer = useVirtualizer(computed(() => ({
	count: filteredLogs.value.length,
	getScrollElement: () => get(scrollEl),
	estimateSize: () => 120,
	overscan: 5,
	getItemKey: (i: number) => filteredLogs.value[i]!.id,
})));

function getLogAtIndex(index: number) {
	return filteredLogs.value[index]!;
}

function measureRow(el: Element | null, _row: VirtualItem) {
	if (el) {
		get(virtualizer).measureElement(el);
	}
}

function toggleSort() {
	set(sortAsc, !get(sortAsc));
	// After sort flip, scroll to top so the user sees the reordered list
	get(scrollEl)?.scrollTo({ top: 0 });
}

function copyLogs() {
	const text = get(logs)
		.map((item) => `[${format(new Date(item.createdAt), 'yyyy-MM-dd HH:mm:ss')}] [${item.logLevel}] ${item.message}`)
		.join('\n');
	copy(text);
	showSuccessNotification(t('components.download-details-dialog.logs.copied-to-clipboard'));
}

function deleteLogs() {
	if (!props.downloadTaskId || !props.downloadTask) {
		return;
	}

	set(logsLoading, true);
	logRefreshTimer.pause();
	dialogStore.closeDialog(DialogType.DownloadDetailsDeleteLogsConfirmationDialog);

	useSubscription(
		downloadApi.deleteAllDownloadTaskLogsByDownloadTaskIdEndpoint(props.downloadTaskId, {
			type: props.downloadTask.downloadTaskType,
			plexLibraryId: props.downloadTask.plexLibraryId,
			plexServerId: props.downloadTask.plexServerId,
		}).subscribe(() => {
			set(logs, []);
			set(highestSeenId, undefined);
			emit('logs-deleted');
			showSuccessNotification(t('components.download-details-dialog.logs.deleted'));
			set(logsLoading, false);
			logRefreshTimer.resume();
		}),
	);
}

function toggleLevel(level: NotificationLevel) {
	const current = get(activeFilters);
	if (current.includes(level)) {
		set(activeFilters, current.filter((l) => l !== level));
	} else {
		set(activeFilters, [...current, level]);
	}
}

function reset() {
	set(sortAsc, true);
	set(activeFilters, [...Object.values(NotificationLevel).filter((v) => v !== NotificationLevel.None)]);
}

function refreshLogs() {
	if (!props.downloadTaskId || !props.downloadTask) {
		return;
	}

	logRefreshTimer.pause();

	const sinceId = get(highestSeenId);
	// On the very first poll (no initialLogs from parent), use take to limit initial load
	const take = sinceId === undefined ? INITIAL_TAKE : undefined;

	useSubscription(
		downloadApi.getDownloadTaskLogsByDownloadTaskIdEndpoint(props.downloadTaskId, {
			type: props.downloadTask.downloadTaskType,
			plexLibraryId: props.downloadTask.plexLibraryId,
			plexServerId: props.downloadTask.plexServerId,
			sinceId,
			take,
		}).subscribe((data) => {
			if (data.isSuccess && data.value && data.value.length > 0) {
				set(logs, [...get(logs), ...data.value]);
				const maxId = Math.max(...data.value.map((l) => l.id));
				set(highestSeenId, Math.max(get(highestSeenId) ?? 0, maxId));
				emit('logs-refreshed', get(logs));
			}
			set(logsLoading, false);
			logRefreshTimer.resume();
		}),
	);
}

onMounted(() => {
	// If initialLogs were pre-fetched by parent, skip the initial full fetch — just poll for new ones
	if (get(logs).length === 0) {
		set(logsLoading, true);
	}
	logRefreshTimer.resume();
});

onUnmounted(() => {
	logRefreshTimer.pause();
	set(logs, []);
	set(highestSeenId, undefined);
});

defineExpose({ reset });
</script>

<style lang="scss">
// The tab panel adds 16px padding — strip it so the logs panel owns its own spacing
.q-tab-panel:has(.logs-panel) {
  padding: 0;
  height: 100%;
  min-height: 0;
}

.logs-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  padding: 0 8px;
}

.logs-scroll-container {
  flex: 1;
  min-height: 0;
  max-width: 100%;
  overflow-y: auto;
  overflow-x: hidden;
}

.log-timeline {
  padding-left: 0.5rem;

  .q-timeline__entry--icon {
    .q-timeline__dot {
      &::before {
        display: none;
      }

      .q-icon {
        font-size: 1.5rem;
        color: currentColor;
      }
    }
  }
}
</style>
