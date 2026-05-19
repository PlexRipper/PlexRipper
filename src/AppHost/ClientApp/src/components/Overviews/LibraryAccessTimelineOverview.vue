<template>
	<QRow
		cy="library-access-timeline-overview"
		class="q-gutter-y-md">
		<QCol cols="12">
			<q-card class="q-pa-md">
				<QRow
					align="center"
					class="q-col-gutter-md">
					<QCol
						cols="12"
						md="3">
						<q-select
							v-model="selectedAccountId"
							:data-cy="'library-access-timeline-account-filter'"
							emit-value
							map-options
							dense
							filled
							:label="t('components.library-access-timeline.filters.account')"
							:options="accountOptions" />
					</QCol>
					<QCol
						cols="12"
						md="3">
						<q-select
							v-model="selectedServerId"
							:data-cy="'library-access-timeline-server-filter'"
							emit-value
							map-options
							clearable
							dense
							filled
							:label="t('components.library-access-timeline.filters.server')"
							:options="serverOptions" />
					</QCol>
					<QCol
						cols="12"
						md="3">
						<q-select
							v-model="selectedLibraryId"
							:data-cy="'library-access-timeline-library-filter'"
							emit-value
							map-options
							clearable
							dense
							filled
							:label="t('components.library-access-timeline.filters.library')"
							:options="libraryOptions" />
					</QCol>
					<QCol
						cols="12"
						md="3">
						<QRow class="q-gutter-sm">
							<q-btn-toggle
								v-model="selectedZoomPreset"
								:data-cy="'library-access-timeline-zoom-filter'"
								dense
								spread
								unelevated
								:options="zoomOptions" />
						</QRow>
					</QCol>
				</QRow>
			</q-card>
		</QCol>

		<QCol cols="12">
			<q-card class="q-pa-md">
				<QText
					class="q-mb-md"
					size="h5"
					:value="t('components.library-access-timeline.chart.title')" />
				<div
					class="library-access-timeline"
					data-cy="library-access-timeline-chart">
					<div
						v-if="timelineStore.isLoading"
						class="text-center q-pa-xl">
						<QSpinner
							color="primary"
							size="3rem" />
					</div>
					<LibraryAccessGanttChart
						v-if="timelineStore.timelineIntervals.length > 0"
						cy="library-access-timeline-gantt"
						:intervals="timelineStore.timelineIntervals"
						:selected-row-id="timelineStore.selectedRowId"
						:zoom-preset="timelineStore.zoomPreset"
						@select-row="timelineStore.selectRow" />
					<QText
						v-else
						align="center"
						class="q-pa-xl"
						:value="t('components.library-access-timeline.chart.empty')" />
				</div>
			</q-card>
		</QCol>

		<QCol cols="12">
			<q-table
				:data-cy="'library-access-timeline-current-state-table'"
				:columns="columns"
				:rows="timelineStore.currentStateRows"
				:loading="timelineStore.isLoading"
				:row-key="getCurrentStateRowKey"
				flat
				bordered
				@click:row="onTableRowClick" />
		</QCol>
	</QRow>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { useAccountStore, useLibraryAccessTimelineStore, useLibraryStore, useServerStore } from '@store';
import type { PlexLibraryAccessCurrentStateLibraryDTO } from '@dto';
import type { LibraryAccessTimelineZoomPreset } from '@store';

const { t } = useI18n();
const accountStore = useAccountStore();
const serverStore = useServerStore();
const libraryStore = useLibraryStore();
const timelineStore = useLibraryAccessTimelineStore();
const refreshTimeline$ = new Subject<void>();

const selectedAccountId = computed<number | null>({
	get: () => timelineStore.filters.plexAccountId,
	set: (value) => {
		timelineStore.setAccount(value);
		refreshTimeline$.next();
	},
});

const selectedServerId = computed({
	get: () => timelineStore.filters.plexServerId ?? null,
	set: (value: number | null) => {
		timelineStore.setServer(value);
		timelineStore.setLibrary(null);
		refreshTimeline$.next();
	},
});

const selectedLibraryId = computed({
	get: () => timelineStore.filters.plexLibraryId ?? null,
	set: (value: number | null) => {
		timelineStore.setLibrary(value);
		refreshTimeline$.next();
	},
});

const selectedZoomPreset = computed({
	get: () => timelineStore.zoomPreset,
	set: (value: LibraryAccessTimelineZoomPreset) => {
		timelineStore.setZoomPreset(value);
		applyZoomPreset(value);
		refreshTimeline$.next();
	},
});

const accountOptions = computed(() => accountStore.getAccounts.map((account) => ({
	label: accountStore.getAccountDisplayName(account.id),
	value: account.id,
})));

const serverOptions = computed(() => get(serverStore.getVisibleServers).map((server) => ({
	label: serverStore.getServerName(server.id),
	value: server.id,
})));

const libraryOptions = computed(() => libraryStore.getLibrariesByServerId(timelineStore.filters.plexServerId ?? 0).map((library) => ({
	label: libraryStore.getLibraryName(library.id),
	value: library.id,
})));

const zoomOptions: { label: string; value: LibraryAccessTimelineZoomPreset }[] = [
	{ label: '1d', value: '1d' },
	{ label: '7d', value: '7d' },
	{ label: '30d', value: '30d' },
	{ label: '90d', value: '90d' },
	{ label: 'All', value: 'All' },
];

const columns = [
	{
		name: 'account',
		label: t('components.library-access-timeline.table.account'),
		field: 'plexAccountName',
		align: 'left' as const,
	},
	{
		name: 'server',
		label: t('components.library-access-timeline.table.server'),
		field: 'plexServerName',
		align: 'left' as const,
	},
	{
		name: 'library',
		label: t('components.library-access-timeline.table.library'),
		field: 'plexLibraryName',
		align: 'left' as const,
	},
	{
		name: 'state',
		label: t('components.library-access-timeline.table.current-state'),
		field: 'currentState',
		align: 'left' as const,
	},
	{
		name: 'grantedAt',
		label: t('components.library-access-timeline.table.granted-at'),
		field: 'grantedAt',
		align: 'left' as const,
	},
	{
		name: 'revokedAt',
		label: t('components.library-access-timeline.table.revoked-at'),
		field: 'revokedAt',
		align: 'left' as const,
	},
	{
		name: 'lastChangedAt',
		label: t('components.library-access-timeline.table.last-changed-at'),
		field: 'lastChangedAt',
		align: 'left' as const,
	},
];

useSubscription(refreshTimeline$.pipe(
	debounceTime(100),
	switchMap(() => timelineStore.refreshTimeline()),
).subscribe());

onMounted(() => {
	if (accountStore.getAccounts.length > 0 && !timelineStore.filters.plexAccountId) {
		timelineStore.setAccount(accountStore.getAccounts[0]!.id);
		refreshTimeline$.next();
	} else {
		refreshTimeline$.next();
	}

	applyZoomPreset(timelineStore.zoomPreset);
});

watch(() => accountStore.getAccounts, (accounts) => {
	if (accounts.length > 0 && !timelineStore.filters.plexAccountId) {
		timelineStore.setAccount(accounts[0]!.id);
		refreshTimeline$.next();
	}
}, { deep: true });

function applyZoomPreset(zoomPreset: LibraryAccessTimelineZoomPreset) {
	if (zoomPreset === 'All') {
		timelineStore.setDateRange(null, null);
		return;
	}

	const days = Number.parseInt(zoomPreset.replace('d', ''), 10);
	const toUtc = new Date();
	const fromUtc = new Date(toUtc);
	fromUtc.setUTCDate(toUtc.getUTCDate() - days);
	timelineStore.setDateRange(fromUtc.toISOString(), toUtc.toISOString());
}

function getCurrentStateRowKey(row: PlexLibraryAccessCurrentStateLibraryDTO): string {
	return `account-${row.plexAccountId}-server-${row.plexServerId ?? 'unknown'}-library-${row.plexLibraryId ?? 'unknown'}`;
}

function onTableRowClick(_event: Event, row: PlexLibraryAccessCurrentStateLibraryDTO) {
	timelineStore.selectRow(`server-${row.plexServerId ?? 'unknown'}-library-${row.plexLibraryId ?? 'unknown'}`);
}
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.library-access-timeline {
	min-height: 18rem;
}
</style>
