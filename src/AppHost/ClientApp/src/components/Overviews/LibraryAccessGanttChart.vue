<template>
	<div
		ref="ganttContainerRef"
		class="library-access-gantt-chart"
		:data-cy="cy" />
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import dayjs from 'dayjs';
import 'jsgantt-improved/dist/jsgantt.css';
import JSGantt from 'jsgantt-improved';
import type { GanttChartFormat } from 'jsgantt-improved';
import type { LibraryAccessTimelineInterval, LibraryAccessTimelineZoomPreset } from '@store';

const props = withDefaults(defineProps<{
	intervals: LibraryAccessTimelineInterval[];
	selectedRowId?: string;
	zoomPreset?: LibraryAccessTimelineZoomPreset;
	cy?: string;
}>(), {
	selectedRowId: '',
	zoomPreset: '30d',
	cy: undefined,
});

const emit = defineEmits<{
	(e: 'select-row', rowId: string): void;
}>();

const validGanttFormats = new Set<GanttChartFormat>(['hour', 'day', 'week', 'month', 'quarter']);
const ganttContainerRef = ref<HTMLElement>();
let ganttChart: ReturnType<typeof createGanttChart> | undefined;
let refreshNowLineInterval: ReturnType<typeof setInterval> | undefined;
let pendingResizeRenderFrame: number | undefined;
let lastMeasuredContainerWidth = 0;

const LEFT_PANE_WIDTH_PX = 240;
const COLUMN_BORDER_COMPENSATION_PX = 2;

const sortedIntervals = computed(() => [...props.intervals].sort((left, right) => {
	const libraryCompare = left.libraryName.localeCompare(right.libraryName);
	if (libraryCompare !== 0) {
		return libraryCompare;
	}

	return left.start.getTime() - right.start.getTime();
}));

watch([sortedIntervals, () => props.selectedRowId, () => props.zoomPreset], () => {
	renderGanttChart();
}, { deep: true });

onMounted(() => {
	renderGanttChart();
	refreshNowLineInterval = setInterval(() => {
		renderGanttChart();
	}, 60000);
});

useResizeObserver(ganttContainerRef, (entries) => {
	const width = entries[0]?.contentRect.width ?? 0;
	if (Math.abs(width - lastMeasuredContainerWidth) < 1) {
		return;
	}

	lastMeasuredContainerWidth = width;
	if (pendingResizeRenderFrame) {
		cancelAnimationFrame(pendingResizeRenderFrame);
	}

	pendingResizeRenderFrame = requestAnimationFrame(() => {
		pendingResizeRenderFrame = undefined;
		renderGanttChart();
	});
});

onBeforeUnmount(() => {
	if (refreshNowLineInterval) {
		clearInterval(refreshNowLineInterval);
		refreshNowLineInterval = undefined;
	}

	if (pendingResizeRenderFrame) {
		cancelAnimationFrame(pendingResizeRenderFrame);
		pendingResizeRenderFrame = undefined;
	}
});

function renderGanttChart() {
	const ganttContainer = get(ganttContainerRef);
	if (!ganttContainer) {
		return;
	}

	const displayFormat = resolveDisplayFormat(props.zoomPreset);
	if (!validGanttFormats.has(displayFormat)) {
		return;
	}

	ganttContainer.innerHTML = '';
	ganttContainer.dataset.zoomPreset = props.zoomPreset;
	ganttChart = createGanttChart(ganttContainer, displayFormat);
	const chartWidth = Math.max(320, ganttContainer.clientWidth - LEFT_PANE_WIDTH_PX);
	const isSevenDayPreset = props.zoomPreset === '7d';
	const isThirtyDayPreset = props.zoomPreset === '30d';
	const dayColumns = isSevenDayPreset ? 7 : 30;
	const dayColWidth = Math.max(24, Math.floor(chartWidth / dayColumns) - COLUMN_BORDER_COMPENSATION_PX);
	const weekColumns = isThirtyDayPreset ? 6 : 5;
	const weekColWidth = Math.max(28, Math.floor(chartWidth / weekColumns) - COLUMN_BORDER_COMPENSATION_PX);
	const monthColWidth = Math.max(24, Math.floor(chartWidth / 30) - COLUMN_BORDER_COMPENSATION_PX);
	const quarterColWidth = Math.max(48, Math.floor(chartWidth / 10) - COLUMN_BORDER_COMPENSATION_PX);

	const dayPresetOptions = {
		vDayMajorDateDisplayFormat: 'mon yyyy - Week ww',
		vDayMinorDateDisplayFormat: 'dd',
	};

	if (isSevenDayPreset) {
		dayPresetOptions.vDayMajorDateDisplayFormat = 'dd mon yyyy';
	}

	ganttChart.setOptions({
		vCaptionType: 'Caption',
		vDayColWidth: dayColWidth,
		vWeekColWidth: weekColWidth,
		vMonthColWidth: monthColWidth,
		vQuarterColWidth: quarterColWidth,
		vDateTaskDisplayFormat: 'day dd month yyyy',
		...dayPresetOptions,
		vWeekMinorDateDisplayFormat: 'dd mon',
		vLang: 'en',
		vShowTaskInfoLink: 0,
		vShowSelector: [],
		vShowRes: 0,
		vShowDur: 0,
		vShowComp: 0,
		vShowStartDate: 0,
		vShowEndDate: 0,
		vShowTaskInfoComp: 0,
		vShowTaskInfoStartDate: 0,
		vShowTaskInfoEndDate: 0,
		vShowEndWeekDate: 0,
		vShowWeekends: 1,
		vUseSingleCell: 10000,
		vFormatArr: ['day', 'week', 'month', 'quarter'] satisfies GanttChartFormat[],
	});

	const sevenDayRange = props.zoomPreset === '7d' ? buildLocalSevenDayRange() : null;
	const visibleIntervals = sevenDayRange
		? clampIntervalsToRange(get(sortedIntervals), sevenDayRange.start, sevenDayRange.end)
		: get(sortedIntervals);

	const tasks = buildChartTasks(visibleIntervals);
	for (const task of tasks) {
		ganttChart.AddTaskItemObject(task);
	}

	applyVisibleRangePadding(ganttChart, visibleIntervals, displayFormat);
	if (sevenDayRange) {
		chartSetDateBounds(ganttChart, sevenDayRange.start, sevenDayRange.end);
	}
	ganttChart.Draw();
	bindHeaderScrollSync(ganttContainer);
	bindTaskSelection(ganttContainer);
	applyPostDrawViewportAdjustments(ganttContainer);
}

function createGanttChart(container: HTMLElement, format: GanttChartFormat) {
	return new JSGantt.GanttChart(container, format);
}

function chartSetDateBounds(chart: ReturnType<typeof createGanttChart>, min: Date, max: Date) {
	chart.setMinDate(min);
	chart.setMaxDate(max);
}

function buildLocalSevenDayRange() {
	const now = new Date();
	const end = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 59, 59, 999);
	const start = new Date(end);
	start.setDate(end.getDate() - 6);
	start.setHours(0, 0, 0, 0);

	return { start, end };
}

function clampIntervalsToRange(
	intervals: LibraryAccessTimelineInterval[],
	start: Date,
	end: Date,
): LibraryAccessTimelineInterval[] {
	const startMs = start.getTime();
	const endMs = end.getTime();

	return intervals
		.map((interval) => {
			const intervalStartMs = interval.start.getTime();
			const intervalEndMs = interval.end.getTime();
			if (intervalEndMs < startMs || intervalStartMs > endMs) {
				return null;
			}

			const clampedStart = new Date(Math.max(intervalStartMs, startMs));
			const clampedEnd = new Date(Math.min(intervalEndMs, endMs));
			if (clampedEnd.getTime() < clampedStart.getTime()) {
				return null;
			}

			return {
				...interval,
				start: clampedStart,
				end: clampedEnd,
			};
		})
		.filter((interval): interval is LibraryAccessTimelineInterval => interval !== null);
}

function applyVisibleRangePadding(
	chart: ReturnType<typeof createGanttChart>,
	intervals: LibraryAccessTimelineInterval[],
	format: GanttChartFormat,
) {
	if (intervals.length === 0 || format === 'day') {
		return;
	}

	let min = Number.POSITIVE_INFINITY;
	let max = Number.NEGATIVE_INFINITY;
	for (const interval of intervals) {
		const startMs = interval.start.getTime();
		const endMs = interval.end.getTime();
		min = Math.min(min, startMs, endMs);
		max = Math.max(max, startMs, endMs);
	}

	if (!Number.isFinite(min) || !Number.isFinite(max)) {
		return;
	}

	const minVisibleDaysByFormat: Partial<Record<GanttChartFormat, number>> = {
		week: 30,
		month: 30,
		quarter: 30,
	};
	const minVisibleDays = minVisibleDaysByFormat[format] ?? 0;
	if (minVisibleDays <= 0) {
		return;
	}

	const visibleWindowMs = minVisibleDays * 24 * 60 * 60 * 1000;
	const dataSpanMs = Math.max(1, max - min);
	if (dataSpanMs >= visibleWindowMs) {
		return;
	}

	const midpoint = min + (dataSpanMs / 2);
	const paddedStart = new Date(midpoint - (visibleWindowMs / 2));
	const paddedEnd = new Date(midpoint + (visibleWindowMs / 2));

	chartSetDateBounds(chart, paddedStart, paddedEnd);
}

function buildChartTasks(intervals: LibraryAccessTimelineInterval[]) {
	const serverGroups = groupIntervalsByServer(intervals);
	const tasks: Record<string, unknown>[] = [];

	for (const serverGroup of serverGroups) {
		tasks.push(buildServerGroupTask(serverGroup));
		for (const interval of serverGroup.intervals) {
			tasks.push(mapIntervalToLibraryTask(interval, serverGroup.id));
		}
	}

	return tasks;
}

function groupIntervalsByServer(intervals: LibraryAccessTimelineInterval[]) {
	const groups = new Map<string, {
		id: string;
		name: string;
		intervals: LibraryAccessTimelineInterval[];
		min: Date;
		max: Date;
	}>();

	for (const interval of intervals) {
		const serverName = interval.serverName || 'Unknown Server';
		const groupId = `server-${slugify(serverName)}`;
		const existing = groups.get(groupId);
		if (existing) {
			existing.intervals.push(interval);
			if (interval.start < existing.min) existing.min = interval.start;
			if (interval.end > existing.max) existing.max = interval.end;
			continue;
		}

		groups.set(groupId, {
			id: groupId,
			name: serverName,
			intervals: [interval],
			min: interval.start,
			max: interval.end,
		});
	}

	return Array.from(groups.values()).sort((left, right) => left.name.localeCompare(right.name));
}

function buildServerGroupTask(serverGroup: { id: string; name: string; min: Date; max: Date }) {
	return {
		pID: serverGroup.id,
		pName: serverGroup.name,
		pStart: formatGanttDate(serverGroup.min),
		pEnd: formatGanttDate(serverGroup.max),
		pClass: 'ggroupblack',
		pLink: '',
		pMile: 0,
		pRes: '',
		pComp: 100,
		pGroup: 1,
		pParent: 0,
		pOpen: 1,
		pDepend: '',
		pCaption: '',
		pNotes: '',
	};
}

function mapIntervalToLibraryTask(interval: LibraryAccessTimelineInterval, parentServerId: string) {
	const isSelected = interval.rowId === props.selectedRowId;
	const isActive = interval.revokedAt === null;

	return {
		pID: interval.id,
		pName: interval.libraryName,
		pStart: formatGanttDate(interval.start),
		pEnd: formatGanttDate(interval.end),
		pClass: isSelected ? 'gtaskblue' : isActive ? 'gtaskgreen' : 'gtaskred',
		pLink: '',
		pMile: 0,
		pRes: '',
		pComp: 100,
		pGroup: 0,
		pParent: parentServerId,
		pOpen: 1,
		pDepend: '',
		pCaption: buildTaskCaption(interval),
		pNotes: buildTaskNotes(interval),
	};
}

function bindHeaderScrollSync(ganttContainer: HTMLElement) {
	const grid = ganttContainer.querySelector<HTMLElement>('.gchartgrid');
	const header = ganttContainer.querySelector<HTMLElement>('.gchartlbl');
	if (!grid || !header) {
		return;
	}

	const sync = () => {
		header.scrollLeft = grid.scrollLeft;
	};

	grid.onscroll = sync;
	sync();
}

function applyPostDrawViewportAdjustments(ganttContainer: HTMLElement) {
	const grid = ganttContainer.querySelector<HTMLElement>('.gchartgrid');
	const header = ganttContainer.querySelector<HTMLElement>('.gchartlbl');
	if (!grid || !header) {
		return;
	}

	const zoomPreset = ganttContainer.dataset.zoomPreset as LibraryAccessTimelineZoomPreset | undefined;
	if (!zoomPreset) {
		return;
	}

	if (zoomPreset === '7d') {
		constrainSevenDayViewport(ganttContainer);
		alignCurrentDayCell(ganttContainer);
	}

	if (zoomPreset === '30d') {
		grid.scrollLeft = 0;
		header.scrollLeft = 0;
	}
}

function constrainSevenDayViewport(ganttContainer: HTMLElement) {
	const grid = ganttContainer.querySelector<HTMLElement>('.gchartgrid');
	const header = ganttContainer.querySelector<HTMLElement>('.gchartlbl');
	const headerTable = ganttContainer.querySelector<HTMLElement>('.gcharttableh');
	const bodyTable = ganttContainer.querySelector<HTMLElement>('.gcharttable');
	const footerTables = Array.from(ganttContainer.querySelectorAll<HTMLElement>('.gchartgrid tfoot .gcharttableh'));
	const headerCells = Array.from(ganttContainer.querySelectorAll<HTMLElement>('.gcharttableh .gminorheading, .gcharttableh .gminorheadingwkend'));
	if (!grid || !header || !headerTable || !bodyTable || headerCells.length === 0) {
		return;
	}

	const expectedDays = buildLocalSevenDayLabels();
	const firstIndex = findConsecutiveDayLabelIndex(headerCells, expectedDays);
	if (firstIndex < 0) {
		return;
	}

	const targetCells = headerCells.slice(firstIndex, firstIndex + expectedDays.length);
	const firstCell = targetCells[0];
	const lastCell = targetCells[targetCells.length - 1];
	if (!firstCell || !lastCell) {
		return;
	}

	const dayWidth = Math.floor(grid.clientWidth / expectedDays.length);
	const tableWidth = dayWidth * expectedDays.length;
	const sourceLeft = firstCell.offsetLeft;
	const sourceRight = lastCell.offsetLeft + lastCell.offsetWidth;
	const sourceWidth = Math.max(1, sourceRight - sourceLeft);
	const scale = tableWidth / sourceWidth;
	const translatedLeft = -(sourceLeft * scale);

	header.style.width = `${tableWidth}px`;
	grid.style.width = `${tableWidth}px`;
	header.style.maxWidth = `${tableWidth}px`;
	grid.style.maxWidth = `${tableWidth}px`;
	header.style.overflowX = 'hidden';
	grid.style.overflowX = 'hidden';
	header.scrollLeft = 0;
	grid.scrollLeft = 0;

	applySevenDayTableClip(headerTable, tableWidth, translatedLeft, scale);
	applySevenDayTableClip(bodyTable, tableWidth, translatedLeft, scale);
	for (const footerTable of footerTables) {
		applySevenDayTableClip(footerTable, tableWidth, translatedLeft, scale);
	}
}

function applySevenDayTableClip(table: HTMLElement, tableWidth: number, translatedLeft: number, scale: number) {
	const wrapper = table.parentElement;
	if (wrapper) {
		wrapper.style.setProperty('width', `${tableWidth}px`, 'important');
		wrapper.style.setProperty('max-width', `${tableWidth}px`, 'important');
		wrapper.style.setProperty('overflow', 'hidden', 'important');
	}

	table.style.setProperty('position', 'relative', 'important');
	table.style.setProperty('left', `${translatedLeft}px`, 'important');
	table.style.setProperty('width', `${tableWidth}px`, 'important');
	table.style.setProperty('min-width', `${tableWidth}px`, 'important');
	table.style.setProperty('max-width', `${tableWidth}px`, 'important');
	table.style.setProperty('transform', `scaleX(${scale})`, 'important');
	table.style.setProperty('transform-origin', 'left top', 'important');
}

function buildLocalSevenDayLabels(): string[] {
	const today = new Date();
	return Array.from({ length: 7 }, (_, index) => {
		const day = new Date(today);
		day.setDate(today.getDate() - (6 - index));
		return dayjs(day).format('DD');
	});
}

function findConsecutiveDayLabelIndex(headerCells: HTMLElement[], expectedDays: string[]): number {
	const headerDays = headerCells.map((cell) => (cell.textContent || '').trim());
	for (let index = 0; index <= headerDays.length - expectedDays.length; index++) {
		const matches = expectedDays.every((day, offset) => headerDays[index + offset] === day);
		if (matches) {
			return index;
		}
	}

	return -1;
}

function alignCurrentDayCell(ganttContainer: HTMLElement) {
	const headerCells = Array.from(ganttContainer.querySelectorAll<HTMLElement>('.gcharttableh .gminorheading, .gcharttableh .gminorheadingwkend'));
	if (headerCells.length === 0) {
		return;
	}

	const todayDay = dayjs().format('DD');
	const todayCellIndex = headerCells.findIndex((cell) => (cell.textContent || '').trim() === todayDay);
	if (todayCellIndex < 0) {
		return;
	}

	const rows = Array.from(ganttContainer.querySelectorAll<HTMLElement>('.gcharttable tbody tr'));
	for (const row of rows) {
		const cells = Array.from(row.querySelectorAll<HTMLElement>('td'));
		for (const cell of cells) {
			cell.classList.remove('gtaskcellcurrent');
		}
		if (cells[todayCellIndex]) {
			cells[todayCellIndex].classList.add('gtaskcellcurrent');
		}
	}

	const todayCell = headerCells[todayCellIndex];
	const grid = ganttContainer.querySelector<HTMLElement>('.gchartgrid');
	if (!todayCell || !grid) {
		return;
	}

	const gridRect = grid.getBoundingClientRect();
	const todayRect = todayCell.getBoundingClientRect();
	const markerLeft = todayRect.left - gridRect.left + (todayRect.width / 2);
	grid.style.setProperty('--library-access-current-day-left', `${markerLeft}px`);
}

function bindTaskSelection(ganttContainer: HTMLElement) {
	for (const interval of get(sortedIntervals)) {
		const taskElement = ganttContainer.querySelector(`[id*="${CSS.escape(interval.id)}"]`);
		if (!taskElement) {
			continue;
		}

		taskElement.setAttribute('data-row-id', interval.rowId);
		taskElement.addEventListener('click', () => emit('select-row', interval.rowId));
	}
}

function buildTaskCaption(interval: LibraryAccessTimelineInterval): string {
	return interval.revokedAt === null
		? `${interval.durationLabel} active`
		: interval.durationLabel;
}

function buildTaskNotes(interval: LibraryAccessTimelineInterval): string {
	const revokedAt = interval.revokedAt ? dayjs(interval.revokedAt).format('YYYY-MM-DD HH:mm') : 'Active';
	return [
		`Account: ${interval.accountName}`,
		`Server: ${interval.serverName}`,
		`Library: ${interval.libraryName}`,
		`Granted: ${dayjs(interval.grantedAt).format('YYYY-MM-DD HH:mm')}`,
		`Revoked: ${revokedAt}`,
	].join('<br>');
}

function resolveDisplayFormat(zoomPreset: LibraryAccessTimelineZoomPreset): GanttChartFormat {
	if (zoomPreset === '1d' || zoomPreset === '7d') {
		return 'day';
	}

	if (zoomPreset === '90d' || zoomPreset === 'All') {
		return 'month';
	}

	return 'week';
}

function formatGanttDate(value: Date): string {
	return dayjs(value).format('YYYY-MM-DD');
}

function slugify(value: string): string {
	return value
		.toLowerCase()
		.replace(/[^a-z0-9]+/g, '-')
		.replace(/(^-|-$)/g, '') || 'unknown';
}
</script>

<style lang="scss">
@use '@/assets/scss/_variables.scss' as *;

.library-access-gantt-chart {
  min-height: 18rem;
  width: 100%;
  max-width: 100%;
  overflow-x: auto;
  overflow-y: visible;
  color: #fff;

  .gantt,
  .gchartcontainer,
  .gmain,
  .gmainleft,
  .gmainright,
  .glistlbl,
  .gchartlbl,
  .glistgrid,
  .gchartgrid,
  .gtasktable,
  .gtasktableh,
  .gcharttable,
  .gcharttableh {
    background: transparent !important;
  }

  .gchartlbl,
  .gchartgrid {
    width: 100% !important;
    min-width: 0 !important;
    max-width: 100% !important;
    overflow-x: hidden !important;
  }

  .gcharttable,
  .gcharttableh {
    width: max-content !important;
    min-width: 100% !important;
    table-layout: auto !important;
  }

  .gantt,
  .gchartcontainer,
  .gmain {
    width: 100% !important;
    max-width: 100% !important;
  }

  .gmain {
    resize: none !important;
    overflow: hidden !important;
  }

  .gmainleft {
    min-width: 240px;
    flex: 0 0 240px;
  }

  .gmainright {
    flex: 1 1 auto;
    min-width: 0;
    overflow: hidden !important;
  }

  .gchartgrid {
    overflow-x: hidden !important;
    overflow-y: hidden !important;
    width: 100% !important;
  }

  .gchartlbl {
    width: 100% !important;
  }

  .gtasktablewrapper {
    overflow-y: hidden !important;
    overflow-x: hidden !important;
  }

  .gtasktableouterwrapper {
    overflow: hidden !important;
  }

  &[data-zoom-preset='7d'] .gchartgrid {
    position: relative !important;
  }

  &[data-zoom-preset='7d'] .gchartgrid::after {
    content: '';
    position: absolute;
    top: 0;
    bottom: 0;
    left: var(--library-access-current-day-left, -9999px);
    width: 2px;
    transform: translateX(-1px);
    background: $blue;
    box-shadow: 0 0 8px rgba($blue, 0.7);
    pointer-events: none;
    z-index: 4;
  }

  .gmajorheading,
  .gminorheading,
  .gminorheadingwkend,
  .gtaskheading,
  .gname,
  .gtaskname,
  .gres,
  .gdur,
  .gcomp,
  .gstartdate,
  .genddate,
  .gplanstartdate,
  .gplanenddate,
  .gcost,
  .gtasklist,
  .gtaskcell,
  .gtaskcellwkend,
  .gtaskcellcurrent,
  .gadditional {
    background: rgba(0, 0, 0, 0.35) !important;
    border-color: rgba(255, 255, 255, 0.15) !important;
    color: #fff !important;
  }

  .ggroupitem,
  .gmileitem,
  .glineitem {
    background: transparent !important;
  }

  .gcaption,
  .ggroupcaption,
  .gmilecaption,
  .gTaskLabel,
  .gTaskText,
  .gTaskNotes,
  .gTtTitle,
  .gfoldercollapse {
    color: #fff !important;
  }

  .JSGanttToolTipcont,
  .gTaskInfo {
    background: rgba(25, 25, 25, 0.35) !important;
    backdrop-filter: blur(14px) saturate(140%);
    -webkit-backdrop-filter: blur(14px) saturate(140%);
    border: 1px solid rgba(255, 0, 0, 0.65) !important;
    box-shadow: 0 0 14px rgba(255, 0, 0, 0.4);
    color: #fff !important;
  }

  .JSGanttToolTipcont *,
  .gTaskInfo * {
    color: #fff !important;
  }

  .gtasktable {
    font-size: 0.875rem;
  }

  .gtaskname,
  .gresource,
  .gcaption {
    white-space: nowrap;
  }
}
</style>
