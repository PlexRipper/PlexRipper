import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { h } from 'vue';
import { of } from 'rxjs';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createPinia, setActivePinia, type Pinia } from 'pinia';
import { createI18n } from 'vue-i18n';
import { baseSetup } from '@services-test-base';
import { generatePlexMedia } from '@mock';
import type { QTableProps } from 'quasar';
import type { DownloadMediaDTO } from '@dto';
import { PlexMediaType } from '@dto';
import MediaOverviewTable from '@components/MediaOverview/MediaTable/MediaOverviewTable.vue';
import MediaQTable from '@components/MediaOverview/MediaTable/MediaQTable.vue';
import messages from '@/lang/en-US.json';

type PhotoRow = {
	id: number;
	title: string;
	owner: string;
	bytes: number;
};

const photoRows: PhotoRow[] = [
	{ id: 11, title: 'Summer', owner: 'Camera', bytes: 512 },
	{ id: 12, title: 'Winter', owner: 'Phone', bytes: 1024 },
];

const photoColumns: NonNullable<QTableProps['columns']> = [
	{ name: 'title', label: 'Title', field: 'title', align: 'left', required: true },
	{ name: 'owner', label: 'Owner', field: 'owner', align: 'left' },
	{
		name: 'bytes',
		label: 'Size',
		field: 'bytes',
		align: 'right',
		format: (value: unknown) => `${value} bytes`,
	},
];

describe('MediaOverviewTable and MediaQTable', () => {
	let pinia: Pinia;
	const mounted: Array<{ unmount: () => void }> = [];

	beforeAll(() => baseSetup());
	beforeEach(() => {
		pinia = createPinia();
		setActivePinia(pinia);
	});
	afterEach(() => {
		mounted.forEach((wrapper) => wrapper.unmount());
		mounted.length = 0;
		vi.restoreAllMocks();
	});

	function plugins() {
		return [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })];
	}

	test('Should render photo-shaped rows and emit the selected row objects', async () => {
		// Arrange
		const wrapper = await mountSuspended(MediaOverviewTable, {
			props: { rows: photoRows, columns: photoColumns, selected: [] },
			global: { plugins: plugins() },
		});
		mounted.push(wrapper);

		// Act
		await wrapper.get('[aria-label="Selection Summer"]').trigger('click');

		// Assert
		expect(wrapper.get('tbody').text()).toContain('Summer');
		expect(wrapper.get('tbody').text()).toContain('512 bytes');
		expect(wrapper.get('[aria-label="Selection Summer"]').attributes('aria-label')).toBe('Selection Summer');
		expect(wrapper.emitted('selection')).toEqual([[[photoRows[0]]]]);
		expect(wrapper.emitted('row-click')).toBeUndefined();
	});

	test('Should open the requested row once through its keyboard-accessible title control', async () => {
		// Arrange
		const wrapper = await mountSuspended(MediaOverviewTable, {
			props: { rows: photoRows, columns: photoColumns, selected: [] },
			global: { plugins: plugins() },
		});
		mounted.push(wrapper);

		// Act
		await wrapper.get('tbody tr:nth-child(2) button').trigger('click');

		// Assert
		expect(wrapper.emitted('row-click')).toEqual([[{ row: photoRows[1], event: expect.any(MouseEvent) }]]);
	});

	test('Should preserve MediaQTable selection and download factory actions', async () => {
		// Arrange
		const mediaRow = generatePlexMedia({
			config: { seed: 901 },
			partialData: {
				id: 31,
				type: PlexMediaType.Movie,
				plexLibraryId: 4,
				plexServerId: 2,
				title: 'Existing movie',
				children: [],
			},
		});
		const downloadMediaFactory = vi.fn((): DownloadMediaDTO[] => []);
		const wrapper = await mountSuspended(MediaQTable, {
			props: { rows: [mediaRow], selection: null, downloadMediaFactory },
			global: { plugins: plugins() },
		});
		mounted.push(wrapper);
		const sharedTable = wrapper.findComponent({ name: 'MediaOverviewTable' });

		// Act
		sharedTable.vm.$emit('selection', [mediaRow]);
		await wrapper.get('tbody tr td:last-child button').trigger('click');

		// Assert
		expect(wrapper.emitted('selection')).toEqual([[
			{ keys: [mediaRow.id], allSelected: true, indexKey: 0 },
		]]);
		expect(downloadMediaFactory).toHaveBeenCalledExactlyOnceWith(mediaRow);
	});

	test('Should virtualize a logical photo count and delegate complete selection to the caller', async () => {
		// Arrange
		const requestRange = vi.fn(() => of(undefined));
		vi.spyOn(HTMLElement.prototype, 'offsetWidth', 'get').mockReturnValue(960);
		vi.spyOn(HTMLElement.prototype, 'offsetHeight', 'get').mockReturnValue(600);
		const wrapper = await mountSuspended(MediaOverviewTable, {
			props: {
				itemCount: 10_000,
				itemAt: (index: number) => ({ id: index, title: `Photo ${index}` }),
				selectedCount: 1,
				isRowSelected: (row: { id: number }) => row.id === 1,
				columns: photoColumns,
				rowTitle: (row: { title: string }) => row.title,
				requestRange,
			},
			global: { plugins: plugins() },
		});
		mounted.push(wrapper);
		expect(wrapper.get('[data-cy="photo-select-all"]').attributes('aria-checked')).toBe('mixed');

		// Act
		await wrapper.get('[data-cy="photo-select-all"]').trigger('click');

		// Assert
		expect(wrapper.get('[aria-rowcount="10000"]')).toBeTruthy();
		expect(wrapper.emitted('select-all')).toEqual([[true]]);
		expect(wrapper.findAll('.media-overview-table__virtual-row').length).toBeLessThan(100);
	});

	test('Should focus the rendered title control after restoring a distant controlled row', async () => {
		// Arrange
		vi.spyOn(HTMLElement.prototype, 'offsetWidth', 'get').mockReturnValue(960);
		vi.spyOn(HTMLElement.prototype, 'offsetHeight', 'get').mockReturnValue(600);
		const wrapper = await mountSuspended(MediaOverviewTable, {
			attachTo: document.body,
			props: {
				itemCount: 3,
				itemAt: (index: number) => ({ id: index, title: `Photo ${index}` }),
				columns: photoColumns,
				rowTitle: (row: { title: string }) => row.title,
			},
			slots: {
				'controlled-row': ({ row }: { row: { title: string } }) => h('td', [
					h('button', { 'data-photo-overview-focus': 'title', type: 'button' }, row.title),
				]),
			},
			global: { plugins: plugins() },
		});
		mounted.push(wrapper);

		// Act
		const exposed = wrapper.vm as unknown as { focusItem: (index: number) => Promise<boolean> }; // defineExpose is not reflected by Vue Test Utils' component instance type.
		const focused = await exposed.focusItem(1);

		// Assert
		expect(focused).toBe(true);
		expect(document.activeElement?.getAttribute('data-photo-overview-focus')).toBe('title');
	});
});
