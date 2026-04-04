import { beforeEach, describe, expect, test, vi } from 'vitest';
import { defineComponent, h } from 'vue';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { of } from 'rxjs';
import { generateDownloadTask } from '@mock/factories/download-task-factory';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { DownloadStatus, DownloadTaskType, NotificationLevel } from '@dto';
import DownloadDetailsDialog from '~/components/Dialogs/DownloadDetailsDialog/DownloadDetailsDialog.vue';

const { getDownloadTaskByGuidEndpointSpy, getDownloadTaskLogsByDownloadTaskIdEndpointSpy } = vi.hoisted(() => ({
	getDownloadTaskByGuidEndpointSpy: vi.fn(),
	getDownloadTaskLogsByDownloadTaskIdEndpointSpy: vi.fn(),
}));

vi.mock('@api', () => ({
	downloadApi: {
		getDownloadTaskByGuidEndpoint: getDownloadTaskByGuidEndpointSpy,
		getDownloadTaskLogsByDownloadTaskIdEndpoint: getDownloadTaskLogsByDownloadTaskIdEndpointSpy,
	},
}));

const QCardDialogStub = defineComponent({
	name: 'QCardDialogStub',
	emits: ['opened', 'closed'],
	setup(_props, { emit, slots }) {
		return () => h('div', { 'data-cy': 'download-details-dialog-stub' }, [
			h('button', {
				'data-cy': 'open-download-details-dialog',
				onClick: () => emit('opened', 'task-guid'),
			}),
			h('button', {
				'data-cy': 'close-download-details-dialog',
				onClick: () => emit('closed'),
			}),
			slots.title?.(),
			slots.default?.(),
		]);
	},
});

const QTabPanelsStub = defineComponent({
	name: 'QTabPanelsStub',
	setup(_props, { slots }) {
		return () => h('div', { 'data-cy': 'q-tab-panels-stub' }, slots.default?.());
	},
});

const QTabPanelStub = defineComponent({
	name: 'QTabPanelStub',
	setup(_props, { slots }) {
		return () => h('div', { 'data-cy': 'q-tab-panel-stub' }, slots.default?.());
	},
});

const OverviewTabContentStub = defineComponent({
	name: 'OverviewTabContentStub',
	props: {
		downloadTask: {
			type: Object,
			default: undefined,
		},
		errors: {
			type: Array,
			default: () => [],
		},
	},
	setup(props) {
		return () => h('div', { 'data-cy': 'overview-tab-content' }, [
			h('div', { 'data-cy': 'overview-download-task-title' }, props.downloadTask?.fullTitle ?? ''),
			h('div', { 'data-cy': 'overview-error-count' }, String(props.errors.length)),
		]);
	},
});

const LogsTabContentStub = defineComponent({
	name: 'LogsTabContentStub',
	props: {
		initialLogs: {
			type: Array,
			default: () => [],
		},
	},
	setup(props) {
		return () => h('div', { 'data-cy': 'logs-tab-content' }, [
			h('div', { 'data-cy': 'download-details-log-count' }, String(props.initialLogs.length)),
		]);
	},
});

function createDownloadTaskLog(id: number, message: string) {
	return {
		id,
		message,
		createdAt: new Date(2024, 0, id).toISOString(),
		logLevel: NotificationLevel.Information,
		status: DownloadStatus.Downloading,
	};
}

describe('DownloadDetailsDialog', () => {
	beforeEach(() => {
		vi.clearAllMocks();
	});

	test('Should clear stale title and logs when reopening after download fetch fails', async () => {
		// Arrange
		const downloadTask = generateDownloadTask({
			id: 'task-guid',
			plexLibraryId: 12,
			plexServerId: 34,
			type: DownloadTaskType.Movie,
			config: { seed: 263 },
			partial: { fullTitle: 'Old Download Title' },
		});
		const logs = [createDownloadTaskLog(1, 'Old log')];

		getDownloadTaskByGuidEndpointSpy
			.mockReturnValueOnce(of(generateResultDTO(downloadTask)))
			.mockReturnValueOnce(of(generateFailedResultDTO()));
		getDownloadTaskLogsByDownloadTaskIdEndpointSpy.mockReturnValue(of(generateResultDTO(logs)));

		const wrapper = await mountSuspended(DownloadDetailsDialog, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					OverviewTabContent: OverviewTabContentStub,
					DownloadDetailsDialogLogsTabContent: LogsTabContentStub,
					QText: true,
					QMediaTypeIcon: true,
					QTab: true,
					QTabs: true,
					QTabPanel: QTabPanelStub,
					QTabPanels: QTabPanelsStub,
				},
				mocks: {
					$t: (key: string) => key,
				},
			},
		});

		// Act
		await wrapper.get('[data-cy="open-download-details-dialog"]').trigger('click');
		await wrapper.vm.$nextTick();
		expect(wrapper.get('[data-cy="overview-download-task-title"]').text()).toContain('Old Download Title');
		expect(wrapper.get('[data-cy="download-details-log-count"]').text()).toBe('1');

		await wrapper.get('[data-cy="open-download-details-dialog"]').trigger('click');
		await wrapper.vm.$nextTick();

		// Assert
		expect(wrapper.get('[data-cy="overview-download-task-title"]').text()).toBe('');
		expect(wrapper.get('[data-cy="download-details-log-count"]').text()).toBe('0');
	});

	test('Should clear stale errors after a successful reopen', async () => {
		// Arrange
		const failedResult = generateFailedResultDTO({
			errors: [{ message: 'Task failed', metadata: {}, reasons: [] }],
		});
		const downloadTask = generateDownloadTask({
			id: 'task-guid',
			plexLibraryId: 12,
			plexServerId: 34,
			type: DownloadTaskType.Movie,
			config: { seed: 264 },
			partial: { fullTitle: 'Recovered Download Title' },
		});

		getDownloadTaskByGuidEndpointSpy
			.mockReturnValueOnce(of(failedResult))
			.mockReturnValueOnce(of(generateResultDTO(downloadTask)));
		getDownloadTaskLogsByDownloadTaskIdEndpointSpy.mockReturnValue(of(generateResultDTO([])));

		const wrapper = await mountSuspended(DownloadDetailsDialog, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					OverviewTabContent: OverviewTabContentStub,
					DownloadDetailsDialogLogsTabContent: LogsTabContentStub,
					QText: true,
					QMediaTypeIcon: true,
					QTab: true,
					QTabs: true,
					QTabPanel: QTabPanelStub,
					QTabPanels: QTabPanelsStub,
				},
				mocks: {
					$t: (key: string) => key,
				},
			},
		});

		// Act
		await wrapper.get('[data-cy="open-download-details-dialog"]').trigger('click');
		await wrapper.vm.$nextTick();
		expect(wrapper.get('[data-cy="overview-error-count"]').text()).toBe('1');

		await wrapper.get('[data-cy="open-download-details-dialog"]').trigger('click');
		await wrapper.vm.$nextTick();

		// Assert
		expect(wrapper.get('[data-cy="overview-error-count"]').text()).toBe('0');
		expect(wrapper.get('[data-cy="overview-download-task-title"]').text()).toContain('Recovered Download Title');
	});
});
