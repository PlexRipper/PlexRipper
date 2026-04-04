import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h, reactive } from 'vue';
import { throwError } from 'rxjs';
import { DownloadActions, DownloadStatus, PlexMediaType, type DownloadProgressDTO, type PlexServerDTO } from '@dto';
import { generatePlexServers } from '@mock';
import DownloadsTable from '@components/Views/Downloads/DownloadsTable.vue';

interface DownloadsTableNode {
	actions?: { loading?: boolean }[];
}

const openDownloadTaskDetailsDialogSpy = vi.fn();
const openDialogSpy = vi.fn();
const closeDialogSpy = vi.fn();
const executeDownloadCommandSpy = vi.fn();

const plexServer: PlexServerDTO = generatePlexServers({ config: { plexServerCount: 1, seed: 263 } })[0]!;

const downloadRow: DownloadProgressDTO = {
	id: 'download-1',
	title: 'Download 1',
	status: DownloadStatus.Downloading,
	percentage: 10,
	dataReceived: 100,
	dataTotal: 1000,
	downloadSpeed: 20,
	timeRemaining: 30,
	children: [],
	mediaType: PlexMediaType.Movie,
};

const downloadStoreMock = reactive({
	executeDownloadCommand: executeDownloadCommandSpy,
	getDownloadsByServerId: vi.fn(() => [downloadRow]),
	getHeaderSelection: vi.fn(() => false),
	getSelectedDownloadTasks: vi.fn(() => []),
	getDownloadSelection: vi.fn(() => ({ maxSelectionCount: 99 })),
	setAllSelectedDownloadTasks: vi.fn(),
	updateSelectedDownloadTasks: vi.fn(),
});

vi.mock('#imports', async () => {
	const actual = await vi.importActual('#imports');
	return {
		...actual,
		useI18n: () => ({
			t: (key: string) => key,
		}),
	};
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

vi.mock('@store', () => ({
	useDownloadStore: () => downloadStoreMock,
	useServerConnectionStore: () => ({
		isServerConnected: vi.fn(() => true),
	}),
	useDialogStore: () => ({
		openDialog: openDialogSpy,
		closeDialog: closeDialogSpy,
		openDownloadTaskDetailsDialog: openDownloadTaskDetailsDialogSpy,
	}),
	useServerStore: () => ({
		getServerName: vi.fn(() => plexServer.name),
	}),
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const PrimeTreeTableStub = defineComponent({
	name: 'PrimeTreeTable',
	props: {
		nodes: {
			type: Array,
			default: () => [],
		},
	},
	emits: ['action', 'all-selected', 'selected'],
	setup(props, { emit }) {
		const nodes = props.nodes as DownloadsTableNode[];
		return () => h('div', [
			h('button', {
				'data-cy': 'trigger-action',
				onClick: () => emit('action', {
					action: DownloadActions.Delete,
					data: nodes[0],
				}),
			}, 'action'),
			h('div', {
				'data-cy': 'first-action-loading',
			}, String(nodes[0]?.actions?.[0]?.loading ?? false)),
		]);
	},
});

describe('DownloadsTable', () => {
	beforeEach(() => {
		openDownloadTaskDetailsDialogSpy.mockReset();
		openDialogSpy.mockReset();
		closeDialogSpy.mockReset();
		executeDownloadCommandSpy.mockReset();
		downloadStoreMock.getDownloadsByServerId = vi.fn(() => [downloadRow]);
	});

	test('Should clear action loading state when a table action errors', async () => {
		// Arrange
		executeDownloadCommandSpy.mockReturnValueOnce(throwError(() => new Error('Delete failed')));
		const wrapper = await mountSuspended(DownloadsTable, {
			props: {
				plexServer,
				downloadRows: [downloadRow],
			},
			global: {
				stubs: {
					QExpansionItem: SlotStub,
					QRow: SlotStub,
					QCol: SlotStub,
					QStatus: SlotStub,
					QBadge: SlotStub,
					IconButton: SlotStub,
					PrimeTreeTable: PrimeTreeTableStub,
					ConfirmationDialog: SlotStub,
				},
			},
		});

		// Act
		await wrapper.find('[data-cy="trigger-action"]').trigger('click');
		await flushPromises();

		// Assert
		expect(executeDownloadCommandSpy).toHaveBeenCalledWith(DownloadActions.Delete, ['download-1'], plexServer.id);
		expect(wrapper.find('[data-cy="first-action-loading"]').text()).toBe('false');
	});
});
