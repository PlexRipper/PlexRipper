import { beforeEach, describe, expect, test, vi } from 'vitest';
import { defineComponent, h } from 'vue';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { of, throwError } from 'rxjs';
import { DownloadStatus, DownloadTaskType, NotificationLevel } from '@dto';
import { generateDownloadTask } from '@mock/factories/download-task-factory';
import { generateFailedResultDTO } from '@mock';
import DownloadDetailsDialogLogsTabContent from '~/components/Dialogs/DownloadDetailsDialog/Tabs/DownloadDetailsDialogLogsTabContent.vue';

const { deleteLogsSpy, openDialogSpy, closeDialogSpy, showSuccessNotificationSpy } = vi.hoisted(() => ({
	deleteLogsSpy: vi.fn(),
	openDialogSpy: vi.fn(),
	closeDialogSpy: vi.fn(),
	showSuccessNotificationSpy: vi.fn(),
}));

vi.mock('@api', () => ({
	downloadApi: {
		deleteAllDownloadTaskLogsByDownloadTaskIdEndpoint: deleteLogsSpy,
		getDownloadTaskLogsByDownloadTaskIdEndpoint: vi.fn(() => of(generateFailedResultDTO())),
	},
}));

vi.mock('@composables', async () => {
	const actual = await vi.importActual('@composables');
	return {
		...actual,
		translateDownloadStatus: vi.fn(() => 'Downloading'),
		showSuccessNotification: showSuccessNotificationSpy,
	};
});

vi.mock('@store', async () => {
	const actual = await vi.importActual('@store');
	return {
		...actual,
		useDialogStore: () => ({
			openDialog: openDialogSpy,
			closeDialog: closeDialogSpy,
		}),
	};
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

const ConfirmationDialogStub = defineComponent({
	name: 'ConfirmationDialogStub',
	emits: ['confirm'],
	setup(_props, { emit }) {
		return () => h('button', {
			'data-cy': 'confirm-delete-logs',
			onClick: () => emit('confirm'),
		});
	},
});

const QRowStub = defineComponent({
	name: 'QRowStub',
	setup(_props, { slots, attrs }) {
		return () => h('div', { ...attrs, 'data-cy': attrs['data-cy'] ?? 'q-row-stub' }, slots.default?.());
	},
});

const QSpinnerDotsStub = defineComponent({
	name: 'QSpinnerDotsStub',
	setup() {
		return () => h('div', { 'data-cy': 'logs-loading' });
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

describe('DownloadDetailsDialogLogsTabContent', () => {
	beforeEach(() => {
		vi.clearAllMocks();
		vi.useRealTimers();
	});

	test('Should keep existing logs and avoid success handling when deleting logs fails', async () => {
		// Arrange
		const downloadTask = generateDownloadTask({
			id: 'task-guid',
			plexLibraryId: 12,
			plexServerId: 34,
			type: DownloadTaskType.Movie,
			config: { seed: 263 },
		});
		const initialLogs = [createDownloadTaskLog(1, 'Old log')];

		deleteLogsSpy.mockReturnValue(of(generateFailedResultDTO()));

		const wrapper = await mountSuspended(DownloadDetailsDialogLogsTabContent, {
			props: {
				downloadTaskId: 'task-guid',
				downloadTask,
				initialLogs,
			},
			global: {
				stubs: {
					ConfirmationDialog: ConfirmationDialogStub,
					QAlert: true,
					QBtn: true,
					QCheckbox: true,
					QCol: true,
					QDateTime: true,
					QIcon: true,
					QItem: true,
					QItemSection: true,
					QList: true,
					QMenu: true,
					QRow: QRowStub,
					QSpinnerDots: QSpinnerDotsStub,
					QText: true,
					QTimeline: true,
					QTimelineEntry: true,
					QToolbar: true,
					QToolbarTitle: true,
					QTooltip: true,
				},
			},
		});

		// Act
		await wrapper.get('[data-cy="confirm-delete-logs"]').trigger('click');
		await wrapper.vm.$nextTick();

		// Assert
		expect(deleteLogsSpy).toHaveBeenCalledWith('task-guid', {
			type: downloadTask.downloadTaskType,
			plexLibraryId: downloadTask.plexLibraryId,
			plexServerId: downloadTask.plexServerId,
		});
		expect(wrapper.emitted('logs-deleted')).toBeFalsy();
		expect(showSuccessNotificationSpy).not.toHaveBeenCalled();
		expect(wrapper.text()).not.toContain('components.download-details-dialog.logs.no-logs');
	});

	test('Should stop loading when deleting logs throws an error', async () => {
		// Arrange
		const downloadTask = generateDownloadTask({
			id: 'task-guid',
			plexLibraryId: 12,
			plexServerId: 34,
			type: DownloadTaskType.Movie,
			config: { seed: 263 },
		});

		deleteLogsSpy.mockReturnValue(throwError(() => new Error('Delete failed')));

		const wrapper = await mountSuspended(DownloadDetailsDialogLogsTabContent, {
			props: {
				downloadTaskId: 'task-guid',
				downloadTask,
				initialLogs: [createDownloadTaskLog(1, 'Old log')],
			},
			global: {
				stubs: {
					ConfirmationDialog: ConfirmationDialogStub,
					QAlert: true,
					QBtn: true,
					QCheckbox: true,
					QCol: true,
					QDateTime: true,
					QIcon: true,
					QItem: true,
					QItemSection: true,
					QList: true,
					QMenu: true,
					QRow: QRowStub,
					QSpinnerDots: QSpinnerDotsStub,
					QText: true,
					QTimeline: true,
					QTimelineEntry: true,
					QToolbar: true,
					QToolbarTitle: true,
					QTooltip: true,
				},
			},
		});

		// Act
		await wrapper.get('[data-cy="confirm-delete-logs"]').trigger('click');
		await wrapper.vm.$nextTick();

		// Assert
		expect(wrapper.find('[data-cy="logs-loading"]').exists()).toBe(false);
	});

	test('Should stop initial loading when refreshing logs throws an error', async () => {
		// Arrange
		vi.useFakeTimers();
		const downloadTask = generateDownloadTask({
			id: 'task-guid',
			plexLibraryId: 12,
			plexServerId: 34,
			type: DownloadTaskType.Movie,
			config: { seed: 263 },
		});

		deleteLogsSpy.mockReturnValue(of(generateFailedResultDTO()));
		const getLogsSpy = vi.fn(() => throwError(() => new Error('Refresh failed')));
		const api = await import('@api');
		vi.mocked(api.downloadApi.getDownloadTaskLogsByDownloadTaskIdEndpoint).mockImplementation(getLogsSpy);

		const wrapper = await mountSuspended(DownloadDetailsDialogLogsTabContent, {
			props: {
				downloadTaskId: 'task-guid',
				downloadTask,
				initialLogs: [],
			},
			global: {
				stubs: {
					ConfirmationDialog: ConfirmationDialogStub,
					QAlert: true,
					QBtn: true,
					QCheckbox: true,
					QCol: true,
					QDateTime: true,
					QIcon: true,
					QItem: true,
					QItemSection: true,
					QList: true,
					QMenu: true,
					QRow: QRowStub,
					QSpinnerDots: QSpinnerDotsStub,
					QText: true,
					QTimeline: true,
					QTimelineEntry: true,
					QToolbar: true,
					QToolbarTitle: true,
					QTooltip: true,
				},
			},
		});

		// Act
		await vi.advanceTimersByTimeAsync(1000);
		await wrapper.vm.$nextTick();

		// Assert
		expect(getLogsSpy).toHaveBeenCalled();
		expect(wrapper.find('[data-cy="logs-loading"]').exists()).toBe(false);
	});
});
