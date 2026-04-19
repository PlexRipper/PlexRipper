<template>
	<div
		class="live-log-viewer q-pa-md"
		data-cy="live-log-viewer">
		<QToolbar class="q-pa-none">
			<QToolbarTitle>
				{{ t('pages.settings.logs.title') }}

				<!-- Help Icon -->
				<HelpButton
					icon="mdi-help-circle-outline"
					class="q-ma-sm"
					@click="helpStore.openHelpDialog({
						label: t('pages.settings.logs.title'),
						title: t('pages.settings.logs.help-title'),
						text: t('pages.settings.logs.help-text'),
					})" />
			</QToolbarTitle>
			<q-checkbox
				:model-value="pauseScroll"
				dense
				size="sm"
				:label="t('pages.settings.logs.pause-scroll')"
				@update:model-value="onPauseScrollChanged" />

			<!-- Sort Logs -->
			<IconButton
				:icon="logsStore.sortDirection === SortDirection.Asc ? 'mdi-sort-ascending' : 'mdi-sort-descending'"
				:title="logsStore.sortDirection === SortDirection.Asc ? t('pages.settings.logs.sort.oldest-first') : t('pages.settings.logs.sort.newest-first')"
				@click="onSortDirectionToggled" />

			<!-- Clear Logs -->
			<IconButton
				icon="mdi-delete-sweep"
				:title="t('pages.settings.logs.clear')"
				@click="logsStore.clearLogs()" />

			<!-- Refresh Logs -->
			<IconButton
				icon="mdi-refresh"
				:title="t('pages.settings.logs.refresh')"
				@click="useSubscription(logsStore.refreshLogs().subscribe())" />

			<!-- Copy Selected -->
			<QBtn
				v-if="selectedEntries.size > 0"
				flat
				dense
				size="sm"
				icon="mdi-content-copy"
				:label="t('pages.settings.logs.copy-selected', { count: selectedEntries.size })"
				color="primary"
				class="q-ml-xs"
				@click="copySelectedEntries" />
		</QToolbar>

		<!-- Search Bar -->
		<div class="row q-col-gutter-md q-mb-md">
			<div class="col-12 col-md-5">
				<q-input
					v-model="logsStore.searchText"
					dense
					clearable
					outlined
					:label="t('pages.settings.logs.search')">
					<template #prepend>
						<q-icon name="mdi-magnify" />
					</template>
				</q-input>
			</div>
			<div class="col-12 col-md-4">
				<!-- Log Level Filter -->
				<q-select
					:model-value="logsStore.selectedLevels"
					:options="levelOptions"
					option-label="label"
					option-value="value"
					multiple
					emit-value
					map-options
					dense
					outlined
					:label="t('pages.settings.logs.level-filter')"
					@update:model-value="onSelectedLevelsChanged">
					<template #option="{ itemProps, opt, selected, toggleOption }">
						<q-item
							v-bind="itemProps"
							clickable
							@click="toggleOption(opt)">
							<q-item-section side>
								<q-checkbox
									:model-value="selected"
									dense
									@update:model-value="toggleOption(opt)" />
							</q-item-section>
							<q-item-section>
								<q-item-label>{{ opt.label }}</q-item-label>
							</q-item-section>
							<q-item-section side>
								<q-badge
									:color="levelColor(opt.value)"
									text-color="black">
									{{ levelCount(opt.value) }}
								</q-badge>
							</q-item-section>
						</q-item>
					</template>
				</q-select>
			</div>
		</div>

		<div class="row items-center q-mb-sm text-caption text-grey-6">
			<div>{{ t('pages.settings.logs.visible-count', { count: logsStore.getLogs.length, total: logsStore.logs.length }) }}</div>
		</div>

		<div
			ref="scrollElement"
			class="live-log-viewer__scroll">
			<template v-if="logsStore.getLogs.length === 0">
				<QAlert type="info">
					{{ t('pages.settings.logs.empty') }}
				</QAlert>
			</template>
			<template v-else>
				<QVirtualScroll
					:items="logsStore.getLogs"
					separator>
					<template #default="{ item }: {item: LiveLogEventDTO }">
						<div
							class="live-log-viewer__row"
							:class="levelClass(item.level)"
							@click="copyLogEntry(item)">
							<div class="live-log-viewer__row-header row items-center q-col-gutter-sm">
								<!-- Selection Checkbox -->
								<div class="col-auto">
									<q-checkbox
										:model-value="selectedEntries.has(item.sequence)"
										dense
										size="sm"
										@update:model-value="onEntrySelectionChanged(item.sequence, $event)"
										@click.stop />
								</div>
								<!-- Level Badge -->
								<div class="col-auto">
									<QBadge
										:color="levelColor(item.level)"
										text-color="black">
										{{ levelLabel(item.level) }}
									</QBadge>
								</div>
								<!-- TimeStamp -->
								<div class="col-auto text-grey-5 live-log-viewer__timestamp">
									<QDateTime
										short-date
										time
										:text="item.timestamp" />
								</div>
								<div
									v-if="item.sourceContext"
									class="col text-grey-6 ellipsis">
									{{ item.sourceContext }}
								</div>
								<div class="col-auto">
									<QBtn
										flat
										round
										dense
										size="xs"
										icon="mdi-content-copy"
										color="grey-6"
										:title="t('pages.settings.logs.copy')"
										@click.stop="copyLogEntry(item)" />
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
import { get, set } from '@vueuse/core';
import { format } from 'date-fns';
import { useLogsStore } from '@store';
import type { LiveLogEventDTO } from '@dto';
import { LogSeverity } from '@dto';
import { SortDirection } from '@enums';

const logsStore = useLogsStore();
const helpStore = useHelpStore();

const { t } = useI18n();

const scrollElement = ref<HTMLElement | null>(null);
const pauseScroll = ref(false);
const selectedEntries = ref<Set<number>>(new Set());

const levelOptions = computed(() => [
	{ label: t('pages.settings.logs.levels.verbose'), value: LogSeverity.Verbose },
	{ label: t('pages.settings.logs.levels.debug'), value: LogSeverity.Debug },
	{ label: t('pages.settings.logs.levels.info'), value: LogSeverity.Information },
	{ label: t('pages.settings.logs.levels.warning'), value: LogSeverity.Warning },
	{ label: t('pages.settings.logs.levels.error'), value: LogSeverity.Error },
	{ label: t('pages.settings.logs.levels.fatal'), value: LogSeverity.Fatal },
] as { label: string; value: LogSeverity }[]);

function levelCount(value: LogSeverity): number {
	return get(logsStore.logs).filter((entry) => entry.level === value).length;
}

function levelLabel(level: LogSeverity): string {
	switch (level) {
		case LogSeverity.Debug:
			return t('general.logs.level.debug');
		case LogSeverity.Information:
			return t('general.logs.level.information');
		case LogSeverity.Warning:
			return t('general.logs.level.warning');
		case LogSeverity.Error:
			return t('general.logs.level.error');
		case LogSeverity.Fatal:
			return t('general.logs.level.fatal');
		default:
			return t('general.commands.unknown');
	}
}

function levelColor(level: LogSeverity): string {
	switch (level) {
		case LogSeverity.Debug:
			return 'grey-6';
		case LogSeverity.Information:
			return 'blue-4';
		case LogSeverity.Warning:
			return 'yellow-6';
		case LogSeverity.Error:
			return 'red-5';
		case LogSeverity.Fatal:
			return 'red-8';
		default:
			return 'black-1';
	}
}

function levelClass(level: LogSeverity): string {
	return `live-log-viewer__row--${level.toString().toLowerCase()}`;
}

function formatTimestamp(timestamp: string): string {
	return format(new Date(timestamp), 'HH:mm:ss.SSS');
}

function formatLogEntry(item: LiveLogEventDTO): string {
	const parts = [
		`[${formatTimestamp(item.timestamp)}] [${levelLabel(item.level)}]`,
		item.sourceContext ? `[${item.sourceContext}]` : null,
		item.message,
		item.exception ? `\n${item.exception}` : null,
	].filter(Boolean);
	return parts.join(' ');
}

async function copyLogEntry(item: LiveLogEventDTO): Promise<void> {
	await navigator.clipboard.writeText(formatLogEntry(item));
	showSuccessNotification(t('pages.settings.logs.copied-to-clipboard'), 2000);
}

function onEntrySelectionChanged(sequence: number, selected: boolean): void {
	const entries = get(selectedEntries);
	const updated = new Set(entries);
	if (selected) {
		updated.add(sequence);
	} else {
		updated.delete(sequence);
	}
	set(selectedEntries, updated);
}

async function copySelectedEntries(): Promise<void> {
	const sequences = get(selectedEntries);
	const entries = logsStore.getLogs.filter((x) => sequences.has(x.sequence));
	const text = entries.map(formatLogEntry).join('\n');
	await navigator.clipboard.writeText(text);
	showSuccessNotification(t('pages.settings.logs.copied-to-clipboard'), 2000);
}

function onSelectedLevelsChanged(value: LogSeverity[] | null): void {
	logsStore.selectedLevels = value ?? [];
}

function onSortDirectionToggled(): void {
	logsStore.toggleSortDirection();
	if (logsStore.sortDirection === SortDirection.Asc) {
		void scrollToBottom();
	} else {
		void scrollToTop();
	}
}

function onPauseScrollChanged(value: boolean): void {
	set(pauseScroll, value);

	if (!value) {
		if (logsStore.sortDirection === SortDirection.Asc) {
			void scrollToBottom();
		} else {
			void scrollToTop();
		}
	}
}

async function scrollToBottom(): Promise<void> {
	if (logsStore.sortDirection !== SortDirection.Asc || get(pauseScroll)) {
		return;
	}

	await nextTick();
	const element = get(scrollElement);
	if (!element) {
		return;
	}

	element.scrollTop = element.scrollHeight;
}

async function scrollToTop(): Promise<void> {
	if (logsStore.sortDirection !== SortDirection.Desc || get(pauseScroll)) {
		return;
	}

	await nextTick();
	const element = get(scrollElement);
	if (!element) {
		return;
	}

	element.scrollTop = 0;
}

watch(
	() => logsStore.getLogs.length,
	async () => {
		if (logsStore.sortDirection === SortDirection.Asc) {
			await scrollToBottom();
		} else {
			await scrollToTop();
		}
	},
);

onMounted(() => {
	useSubscription(logsStore.refreshLogs().subscribe());

	void scrollToBottom();
});

onUnmounted(() => logsStore.$reset());
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.live-log-viewer {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 120px);

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
    position: relative;
    padding: 0.75rem 1rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
    font-family: monospace;
    cursor: pointer;

    &:hover {
      background: rgba(255, 255, 255, 0.03);
    }

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
