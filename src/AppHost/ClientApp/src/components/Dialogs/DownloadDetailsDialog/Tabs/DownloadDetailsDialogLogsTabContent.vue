<template>
	<div class="logs-panel">
		<!-- Logs Toolbar -->
		<q-toolbar class="q-px-none q-pb-xs">
			<!-- Log Copy -->
			<q-btn
				dense
				flat
				icon="mdi-content-copy"
				round
				@click="copyLogs">
				<q-tooltip>Copy all logs</q-tooltip>
			</q-btn>
			<!-- Log Sort -->
			<q-btn
				:icon="sortAsc ? 'mdi-sort-ascending' : 'mdi-sort-descending'"
				dense
				flat
				round
				@click="sortAsc = !sortAsc">
				<q-tooltip>{{ sortAsc ? 'Oldest first' : 'Newest first' }}</q-tooltip>
			</q-btn>
			<!-- Log Filter -->
			<q-btn
				dense
				flat
				icon="mdi-filter-outline"
				round>
				<q-tooltip>Filter by log level</q-tooltip>
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
							<q-item-section>{{ level }}</q-item-section>
						</q-item>
					</q-list>
				</q-menu>
			</q-btn>
		</q-toolbar>
		<!-- Logs Display -->
		<q-scroll-area class="logs-scroll-area">
			<q-timeline layout="dense">
				<QTimelineEntry
					v-for="(item, index) in filteredLogs"
					:key="index"
					:color="Convert.logLevelToColor(item.logLevel)"
					:icon="Convert.logLevelToIcon(item.logLevel)"
					:title="translateDownloadStatus(item.status)">
					<template #subtitle>
						<QDateTime
							:text="item.createdAt"
							short-date
							time />
					</template>
					<QText :value="item.message" />
				</QTimelineEntry>
			</q-timeline>
			<!-- Logs Loading Indicator -->
			<QRow
				column
				align="center">
				<QCol cols="auto">
					<QSpinnerDots
						color="primary"
						size="40px" />
				</QCol>
			</QRow>
		</q-scroll-area>
	</div>
</template>

<script lang="ts" setup>
import { get, set, useClipboard } from '@vueuse/core';
import { format } from 'date-fns';
import { downloadApi } from '@api';
import type { DownloadTaskLogDTO } from '@dto';
import { NotificationLevel } from '@dto';
import Convert from '@class/Convert';
import { translateDownloadStatus, showSuccessNotification } from '@composables';

const { copy } = useClipboard({ legacy: true });

const props = defineProps<{
	downloadTaskId: string;
}>();

const sortAsc = ref(true);
const activeFilters = ref<NotificationLevel[]>([...Object.values(NotificationLevel).filter((v) => v !== NotificationLevel.None)]);
const logs = ref<DownloadTaskLogDTO[]>([]);
const logsLoading = ref(false);
const logRefreshTimer = useIntervalFn(() => refreshLogs(), 1000);

const allLevels = Object.values(NotificationLevel).filter((v) => v !== NotificationLevel.None && v !== NotificationLevel.Verbose);

const filteredLogs = computed(() => {
	const filtered = get(logs).filter((item) => get(activeFilters).includes(item.logLevel));
	return [...filtered].sort((a, b) => {
		const diff = new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
		return get(sortAsc) ? diff : -diff;
	});
});

function copyLogs() {
	const text = get(logs)
		.map((item) => `[${format(new Date(item.createdAt), 'yyyy-MM-dd HH:mm:ss')}] [${item.logLevel}] ${item.message}`)
		.join('\n');
	copy(text);
	showSuccessNotification('Logs copied to clipboard');
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
	if (!get(props.downloadTaskId)) {
		return;
	}

	set(logsLoading, true);
	logRefreshTimer.pause();

	useSubscription(
		downloadApi.getDownloadTaskLogsByDownloadTaskIdEndpoint(get(props.downloadTaskId)).subscribe((data) => {
			if (data.isSuccess && data.value) {
				set(logs, [...data.value]);
			}
			set(logsLoading, false);
			logRefreshTimer.resume();
		}));
}

onMounted(() => {
	logRefreshTimer.resume();
});

onUnmounted(() => {
	logRefreshTimer.pause();

	set(logs, []);
});

defineExpose({ reset });
</script>

<style lang="scss">
.logs-panel {
  display: flex;
  flex-direction: column;
  height: 100%;
  padding: 0 8px;

  .logs-scroll-area {
    flex: 1;
    max-width: 100%;
  }
}

.q-timeline {
  padding-left: 12px;

  .q-timeline__entry--icon {
    .q-timeline__dot {
      width: 16px;

      &::before {
        display: none;
      }

      &::after {
        top: 22px;
        left: 7px;
      }

      .q-icon {
        font-size: 16px;
        height: 16px;
        line-height: 16px;
        top: 4px;
        color: currentColor;
      }
    }
  }
}
</style>
