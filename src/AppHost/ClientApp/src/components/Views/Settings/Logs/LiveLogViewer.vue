<template>
	<div
		class="live-log-viewer q-pa-md"
		data-cy="live-log-viewer">
		<div class="live-log-viewer__toolbar row items-center q-col-gutter-sm q-mb-md">
			<div class="col-12 col-md-auto">
				<QText
					class="text-weight-bold"
					:size="'h6'"
					:value="t('pages.settings.logs.title')" />
			</div>
			<div class="col-12 col-md">
				<QText
					class="text-grey-6"
					:value="t('pages.settings.logs.subtitle')" />
			</div>
			<div class="col-12 col-md-auto row items-center q-gutter-sm justify-end">
				<q-btn-toggle
					:model-value="sortDirection"
					unelevated
					toggle-color="primary"
					:text-color="'white'"
					:options="sortOptions"
					@update:model-value="onSortDirectionChanged" />
				<q-btn
					dense
					flat
					icon="mdi-delete-sweep"
					:label="t('pages.settings.logs.clear')"
					@click="logsStore.clearLogs()" />
			</div>
		</div>

		<div class="row q-col-gutter-md q-mb-md">
			<div class="col-12 col-md-5">
				<q-input
					:model-value="searchText"
					dense
					clearable
					outlined
					:label="t('pages.settings.logs.search')"
					@update:model-value="onSearchTextChanged">
					<template #prepend>
						<q-icon name="mdi-magnify" />
					</template>
				</q-input>
			</div>
			<div class="col-12 col-md-4">
				<q-select
					:model-value="selectedLevels"
					:options="levelOptions"
					option-label="label"
					option-value="value"
					multiple
					emit-value
					map-options
					dense
					outlined
					:label="t('pages.settings.logs.level-filter')"
					@update:model-value="onSelectedLevelsChanged" />
			</div>
		</div>

		<div class="row items-center justify-between q-mb-sm text-caption text-grey-6">
			<div>{{ t('pages.settings.logs.visible-count', { count: filteredLogs.length, total: logsStore.logs.length }) }}</div>
			<div>{{ t('pages.settings.logs.security-note') }}</div>
		</div>

		<div
			ref="scrollElement"
			class="live-log-viewer__scroll">
			<template v-if="filteredLogs.length === 0">
				<QAlert type="info">
					{{ t('pages.settings.logs.empty') }}
				</QAlert>
			</template>
			<template v-else>
				<QVirtualScroll
					:items="filteredLogs"
					virtual-scroll-item-size="88"
					separator>
					<template #default="{ item }">
						<div
							class="live-log-viewer__row"
							:class="levelClass(item.level)">
							<div class="live-log-viewer__row-header row items-center q-col-gutter-sm">
								<div class="col-auto text-grey-5 live-log-viewer__timestamp">
									{{ formatTimestamp(item.timestamp) }}
								</div>
								<div class="col-auto">
									<QBadge
										:color="levelColor(item.level)"
										text-color="black">
										{{ levelLabel(item.level) }}
									</QBadge>
								</div>
								<div
									v-if="item.sourceContext"
									class="col text-grey-6 ellipsis">
									{{ item.sourceContext }}
								</div>
							</div>
							<pre class="live-log-viewer__message">{{ item.message }}</pre>
							<pre
								v-if="item.exception"
								class="live-log-viewer__exception">{{ item.exception }}</pre>
						</div>
					</template>
				</QVirtualScroll>
			</template>
		</div>
	</div>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set } from '@vueuse/core';
import { format } from 'date-fns';
import { useLogsStore } from '@store';
import type { LiveLogEventDTO } from '@dto';
import { SortDirection } from '@enums';

type LogLevel = 'Verbose' | 'Debug' | 'Information' | 'Warning' | 'Error' | 'Fatal';

const logsStore = useLogsStore();
const { t } = useI18n();

const scrollElement = ref<HTMLElement | null>(null);
const searchText = ref('');
const selectedLevels = ref<LogLevel[]>(['Debug', 'Information', 'Warning', 'Error', 'Fatal']);
const sortDirection = ref<SortDirection>(get(logsStore.sortDirection));

const levelOptions = computed(() => [
	{ label: t('pages.settings.logs.levels.debug'), value: 'Debug' },
	{ label: t('pages.settings.logs.levels.info'), value: 'Information' },
	{ label: t('pages.settings.logs.levels.warning'), value: 'Warning' },
	{ label: t('pages.settings.logs.levels.error'), value: 'Error' },
	{ label: t('pages.settings.logs.levels.fatal'), value: 'Fatal' },
] as { label: string; value: LogLevel }[]);

const sortOptions = computed(() => [
	{ label: t('pages.settings.logs.sort.oldest-first'), value: SortDirection.Asc },
	{ label: t('pages.settings.logs.sort.newest-first'), value: SortDirection.Desc },
]);

const filteredLogs = computed((): LiveLogEventDTO[] => {
	const query = get(searchText).trim().toLowerCase();
	const levels = new Set(get(selectedLevels));

	return logsStore.getSortedLogs.filter((entry) => {
		if (!levels.has(normalizeLevel(entry.level))) {
			return false;
		}

		if (!query) {
			return true;
		}

		const haystack = `${entry.message}\n${entry.exception ?? ''}\n${entry.sourceContext ?? ''}`.toLowerCase();
		return haystack.includes(query);
	});
});

function normalizeLevel(level: string): LogLevel {
	switch (level) {
		case 'Verbose':
		case 'Debug':
		case 'Information':
		case 'Warning':
		case 'Error':
		case 'Fatal':
			return level;
		default:
			return 'Information';
	}
}

function levelLabel(level: string): string {
	switch (normalizeLevel(level)) {
		case 'Debug':
			return t('pages.settings.logs.levels.debug');
		case 'Information':
			return t('pages.settings.logs.levels.info');
		case 'Warning':
			return t('pages.settings.logs.levels.warning');
		case 'Error':
			return t('pages.settings.logs.levels.error');
		case 'Fatal':
			return t('pages.settings.logs.levels.fatal');
		default:
			return t('pages.settings.logs.levels.info');
	}
}

function levelColor(level: string): string {
	switch (normalizeLevel(level)) {
		case 'Debug':
			return 'grey-6';
		case 'Information':
			return 'blue-4';
		case 'Warning':
			return 'yellow-6';
		case 'Error':
			return 'red-5';
		case 'Fatal':
			return 'red-8';
		default:
			return 'blue-4';
	}
}

function levelClass(level: string): string {
	return `live-log-viewer__row--${normalizeLevel(level).toLowerCase()}`;
}

function formatTimestamp(timestamp: string): string {
	return format(new Date(timestamp), 'HH:mm:ss.SSS');
}

function onSearchTextChanged(value: string | number | null): void {
	set(searchText, String(value ?? ''));
}

function onSelectedLevelsChanged(value: LogLevel[] | null): void {
	set(selectedLevels, value ?? []);
}

function onSortDirectionChanged(value: SortDirection): void {
	set(sortDirection, value);
	logsStore.setSortDirection(value);

	if (value === SortDirection.Asc) {
		void scrollToBottom();
	}
}

async function scrollToBottom(): Promise<void> {
	if (get(sortDirection) !== SortDirection.Asc) {
		return;
	}

	await nextTick();
	const element = get(scrollElement);
	if (!element) {
		return;
	}

	element.scrollTop = element.scrollHeight;
}

watch(
	() => filteredLogs.value.length,
	async () => {
		await scrollToBottom();
	},
);

onMounted(() => {
	logsStore.setup().subscribe((result) => {
		if (!result.isSuccess) {
			Log.error('Failed to initialize live log viewer');
		}
	});

	void scrollToBottom();
});

onUnmounted(() => {
	logsStore.$reset();
});
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.live-log-viewer {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 180px);

  &__scroll {
    flex: 1;
    overflow: auto;
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 8px;
    background: rgba(0, 0, 0, 0.25);
  }

  &__spacer {
    position: relative;
    width: 100%;
  }

  &__row {
    padding: 0.75rem 1rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
    font-family: monospace;

    &--debug {
      border-left: 4px solid #9e9e9e;
    }

    &--information {
      border-left: 4px solid #42a5f5;
    }

    &--warning {
      border-left: 4px solid #fdd835;
    }

    &--error,
    &--fatal {
      border-left: 4px solid #ef5350;
    }
  }

  &__timestamp {
    min-width: 86px;
  }

  &__message,
  &__exception {
    margin: 0.5rem 0 0;
    white-space: pre-wrap;
    word-break: break-word;
    font-family: inherit;
  }

  &__exception {
    color: #ef9a9a;
  }
}
</style>
