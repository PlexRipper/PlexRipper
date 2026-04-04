import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h, reactive } from 'vue';
import { of } from 'rxjs';
import { PlexMediaType, FolderType, type DownloadMediaDTO, type FolderPathDTO } from '@dto';
import DownloadConfirmation from '@components/Dialogs/DownloadConfirmation.vue';

interface PreviewNode {
	key: string;
	title: string;
}

const previewDownloadSpy = vi.fn();

const folderPath: FolderPathDTO = {
	id: 1,
	displayName: 'Movies',
	directory: '/movies',
	mediaType: PlexMediaType.Movie,
	folderType: FolderType.MovieFolder,
	isValid: true,
	isDefault: true,
};

const downloadStoreMock = reactive({
	previewDownload: previewDownloadSpy,
});

const folderPathStoreMock = reactive({
	getFolderPaths: () => [folderPath],
});

const dialogStoreMock = reactive({
	openDirectoryBrowserDialog: vi.fn(),
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

vi.mock('@store', () => ({
	useDownloadStore: () => downloadStoreMock,
	useFolderPathStore: () => folderPathStoreMock,
	useDialogStore: () => dialogStoreMock,
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const QTextStub = defineComponent({
	name: 'QText',
	props: {
		value: {
			type: String,
			default: '',
		},
		cy: {
			type: String,
			default: '',
		},
	},
	setup(props, { slots }) {
		return () => h('div', { 'data-cy': props.cy }, slots.default ? slots.default() : props.value);
	},
});

const QFileSizeStub = defineComponent({
	name: 'QFileSize',
	props: {
		size: {
			type: Number,
			default: 0,
		},
	},
	setup(props) {
		return () => h('div', { 'data-cy': 'total-size' }, String(props.size));
	},
});

const TreeTableStub = defineComponent({
	name: 'TreeTable',
	props: {
		value: {
			type: Array,
			default: () => [],
		},
	},
	setup(props) {
		return () => h('div', (props.value as PreviewNode[]).map((node) => h('div', { 'data-cy': `preview-${node.key}` }, node.title)));
	},
});

const QCardDialogStub = defineComponent({
	name: 'QCardDialog',
	emits: ['opened'],
	setup(_, { slots }) {
		return () => h('div', [
			slots['top-row']?.(),
			slots.default?.(),
			slots.actions?.({ close: vi.fn() }),
		]);
	},
});

describe('DownloadConfirmation', () => {
	beforeEach(() => {
		previewDownloadSpy.mockReset();
		dialogStoreMock.openDirectoryBrowserDialog = vi.fn();
	});

	async function mountDialog() {
		return mountSuspended(DownloadConfirmation, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					QRow: SlotStub,
					QCol: SlotStub,
					QText: QTextStub,
					QFileSize: QFileSizeStub,
					TreeTable: TreeTableStub,
					Column: SlotStub,
					QMediaTypeIcon: SlotStub,
					MediaQuality: SlotStub,
					CancelButton: SlotStub,
					QSection: SlotStub,
					'q-btn-dropdown': SlotStub,
					'q-list': SlotStub,
					'q-item': SlotStub,
					'q-item-section': SlotStub,
					'q-radio': SlotStub,
					'q-item-label': SlotStub,
					DirectoryBrowser: SlotStub,
				},
			},
		});
	}

	test('Should clear stale preview rows when reopening after a failed preview request', async () => {
		// Arrange
		previewDownloadSpy
			.mockReturnValueOnce(of({
				previews: [{ key: 'a', title: 'Old Preview', type: PlexMediaType.Movie, qualities: [], size: 12, children: [] }],
				totalSize: 12,
				expanded: {},
			}))
			.mockReturnValueOnce(of(null));
		const wrapper = await mountDialog();
		const payload = [{ mediaIds: [1], keepCompletedInDownloadFolder: false, plexLibraryId: 1, plexServerId: 1, qualities: [], type: PlexMediaType.Movie }] as DownloadMediaDTO[];
		const dialog = wrapper.findComponent(QCardDialogStub);

		// Act
		dialog.vm.$emit('opened', payload);
		await flushPromises();
		expect(wrapper.text()).toContain('Old Preview');

		dialog.vm.$emit('opened', payload);
		await flushPromises();

		// Assert
		expect(wrapper.text()).not.toContain('Old Preview');
	});

	test('Should clear stale total size when reopening after a failed preview request', async () => {
		// Arrange
		previewDownloadSpy
			.mockReturnValueOnce(of({
				previews: [{ key: 'a', title: 'Old Preview', type: PlexMediaType.Movie, qualities: [], size: 12, children: [] }],
				totalSize: 12,
				expanded: {},
			}))
			.mockReturnValueOnce(of(null));
		const wrapper = await mountDialog();
		const payload = [{ mediaIds: [1], keepCompletedInDownloadFolder: false, plexLibraryId: 1, plexServerId: 1, qualities: [], type: PlexMediaType.Movie }] as DownloadMediaDTO[];
		const dialog = wrapper.findComponent(QCardDialogStub);

		// Act
		dialog.vm.$emit('opened', payload);
		await flushPromises();
		expect(wrapper.find('[data-cy="total-size"]').text()).toContain('12');

		dialog.vm.$emit('opened', payload);
		await flushPromises();

		// Assert
		expect(wrapper.find('[data-cy="total-size"]').text()).toContain('0');
	});
});
