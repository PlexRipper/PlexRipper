import { mountSuspended } from '@nuxt/test-utils/runtime';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h, nextTick } from 'vue';
import { PlexAccessState } from '@dto';
import RefreshAccountAccessDialog from '../../../../src/components/Dialogs/RefreshAccountAccessDialog.vue';

const { libraryStoreMock } = vi.hoisted(() => ({
	libraryStoreMock: {
		getLibrary: vi.fn(),
	},
}));

vi.mock('vue-i18n', async () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

vi.mock('@store', () => ({
	useLibraryStore: vi.fn(() => libraryStoreMock),
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const QTabStub = defineComponent({
	name: 'q-tab',
	props: {
		label: {
			type: String,
			default: '',
		},
	},
	setup(props) {
		return () => h('div', props.label);
	},
});

const QTreeStub = defineComponent({
	name: 'q-tree',
	props: {
		nodes: {
			type: Array,
			default: () => [],
		},
	},
	setup(props) {
		function flatten(nodes: Array<{ name: string; children?: Array<{ name: string; children?: unknown[] }> }>): string[] {
			return nodes.flatMap((node) => [node.name, ...(node.children ? flatten(node.children as never) : [])]);
		}

		return () => h('div', flatten(props.nodes as never).join('|'));
	},
});

describe('RefreshAccountAccessDialog', () => {
	beforeEach(() => {
		libraryStoreMock.getLibrary.mockReset();
	});

	async function mountDialog() {
		const QCardDialogStub = defineComponent({
			name: 'QCardDialog',
			emits: ['opened', 'closed'],
			setup(_, { slots }) {
				return () => h('div', [
					slots.title?.(),
					slots['top-row']?.(),
					slots.default?.(),
					slots.actions?.({ close: vi.fn() }),
				]);
			},
		});

		const wrapper = await mountSuspended(RefreshAccountAccessDialog, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					QText: SlotStub,
					QRow: SlotStub,
					QCol: SlotStub,
					QIconTooltip: SlotStub,
					QStatus: SlotStub,
					QMediaTypeIcon: SlotStub,
					HideButton: SlotStub,
					'q-separator': SlotStub,
					'q-tabs': SlotStub,
					'q-tab': QTabStub,
					'q-tab-panels': SlotStub,
					'q-tab-panel': SlotStub,
					'q-tree': QTreeStub,
					'q-icon': SlotStub,
				},
			},
		});

		return { wrapper, QCardDialogStub };
	}

	test('Should render missing libraries without crashing', async () => {
		// Arrange
		libraryStoreMock.getLibrary.mockReturnValue(undefined);
		const { wrapper, QCardDialogStub } = await mountDialog();
		const rapport = [{
			plexAccountId: 1,
			plexAccountName: 'Main Account',
			access: [{
				plexServerId: 5,
				plexServerName: 'Primary Server',
				isServerOffline: false,
				state: PlexAccessState.Unknown,
				libraryAccess: [{
					plexLibraryId: 99,
					plexLibraryName: 'Missing Library',
					plexServerId: 5,
					state: PlexAccessState.Revoked,
				}],
			}],
		}];

		// Act
		expect(() => wrapper.findComponent(QCardDialogStub).vm.$emit('opened', rapport)).not.toThrow();
		await nextTick();

		// Assert
		expect(wrapper.text()).toContain('Missing Library');
		expect(wrapper.text()).toContain('Primary Server');
	});
});
