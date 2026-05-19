<template>
	<div
		ref="chartElement"
		:data-cy="cy"
		class="library-access-gantt-chart" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { get } from '@vueuse/core';
import Gantt from 'frappe-gantt';
import '../../../node_modules/frappe-gantt/dist/frappe-gantt.css';
import { compact } from 'lodash-es';
import type { LibraryAccessTimelineInterval, LibraryAccessTimelineZoomPreset } from '@store';

const props = withDefaults(defineProps<{
	intervals: LibraryAccessTimelineInterval[];
	selectedRowId?: string;
	zoomPreset: LibraryAccessTimelineZoomPreset;
	cy?: string;
}>(), {
	selectedRowId: '',
	cy: 'library-access-gantt-chart',
});

const emit = defineEmits<{
	(e: 'select-row', rowId: string): void;
}>();

const chartElement = ref<HTMLElement | null>(null);
const tasks = computed(() => compact(props.intervals.map((interval) => {
	const startTime = interval.start.getTime();
	const endTime = interval.end.getTime();
	if (!Number.isFinite(startTime) || !Number.isFinite(endTime)) {
		Log.warn('Skipping invalid library access interval for gantt chart', interval);
		return null;
	}

	return {
		id: interval.id,
		name: `${interval.serverName} / ${interval.libraryName}`,
		start: interval.start.toISOString().slice(0, 10),
		end: interval.end.toISOString().slice(0, 10),
		progress: interval.revokedAt ? 100 : 0,
		custom_class: interval.rowId === props.selectedRowId ? 'library-access-gantt-chart__bar--selected' : 'library-access-gantt-chart__bar',
		rowId: interval.rowId,
		interval,
	};
})));

watch([tasks, () => props.zoomPreset], () => renderChart(), { deep: true });

onMounted(() => renderChart());

function renderChart() {
	const element = get(chartElement);
	if (!element) {
		return;
	}

	element.innerHTML = '';
	if (get(tasks).length === 0) {
		return;
	}

	new Gantt(element, get(tasks), {
		holidays: null,
		view_mode: getViewMode(props.zoomPreset),
		readonly: true,
		on_click: (task) => emit('select-row', task.rowId),
		custom_popup_html: (task) => {
			const interval = task.interval as LibraryAccessTimelineInterval;
			return `
				<div class="library-access-gantt-chart__tooltip">
					<strong>${interval.accountName}</strong><br />
					${interval.serverName} / ${interval.libraryName}<br />
					${interval.grantedAt} - ${interval.revokedAt ?? 'Now'}<br />
					${interval.durationLabel}
				</div>
			`;
		},
	});
}

function getViewMode(zoomPreset: LibraryAccessTimelineZoomPreset): string {
	switch (zoomPreset) {
		case '1d':
		case '7d':
			return 'Day';
		case '30d':
		case '90d':
			return 'Week';
		case 'All':
		default:
			return 'Month';
	}
}
</script>

<style lang="scss">
@use '@/assets/scss/variables.scss' as *;

.library-access-gantt-chart {
  overflow-x: auto;
}

.library-access-gantt-chart__bar .bar {
  fill: $positive;
}

.library-access-gantt-chart__bar--selected .bar {
  fill: $primary;
}

.library-access-gantt-chart__tooltip {
  padding: 0.5rem;
}
</style>
