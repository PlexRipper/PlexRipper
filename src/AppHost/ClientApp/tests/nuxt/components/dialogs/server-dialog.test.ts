import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h } from 'vue';
import { of } from 'rxjs';
import { generateFailedResultDTO, generatePlexServers } from '@mock';
import type { PlexServerDTO } from '@dto';
import ServerDialog from '@components/Dialogs/ServerDialog/ServerDialog.vue';

const closeDialogSpy = vi.fn();
const setServerHiddenSpy = vi.fn(() => of(generateFailedResultDTO()));

const plexServer: PlexServerDTO = generatePlexServers({ config: { plexServerCount: 1, seed: 263 } })[0]!;

vi.mock('@store', async () => {
	const actual = await vi.importActual<typeof import('@store')>('@store');
	return {
		...actual,
		useServerStore: () => ({
			getServer: (id: number) => (id === plexServer.id ? plexServer : null),
			getServerName: (id: number) => (id === plexServer.id ? plexServer.name : null),
			setServerAlias: vi.fn(() => of(generateFailedResultDTO())),
			setServerHidden: setServerHiddenSpy,
		}),
		useLibraryStore: () => ({
			getLibrariesByServerId: vi.fn(() => []),
		}),
		useDialogStore: () => ({
			closeDialog: closeDialogSpy,
			openDialog: vi.fn(),
		}),
	};
});

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const QCardDialogStub = defineComponent({
	name: 'QCardDialog',
	emits: ['opened'],
	setup(_, { slots }) {
		return () => h('div', [
			slots.default?.(),
			slots.actions?.(),
		]);
	},
});

const ConfirmationDialogStub = defineComponent({
	name: 'ConfirmationDialog',
	emits: ['confirm'],
	setup(_, { emit }) {
		return () => h('button', { 'data-cy': 'confirm-hide', onClick: () => emit('confirm') }, 'confirm');
	},
});

describe('ServerDialog', () => {
	beforeEach(() => {
		closeDialogSpy.mockReset();
		setServerHiddenSpy.mockReset();
		setServerHiddenSpy.mockReturnValue(of(generateFailedResultDTO()));
	});

	test('Should keep the dialog open when hiding a server fails', async () => {
		// Arrange
		const wrapper = await mountSuspended(ServerDialog, {
			shallow: true,
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					QTabs: SlotStub,
					QTab: SlotStub,
					QTabPanels: SlotStub,
					QTabPanel: SlotStub,
					ServerDataTabContent: SlotStub,
					ServerConnectionsTabContent: SlotStub,
					ServerCommandsTabContent: SlotStub,
					ConfirmationDialog: ConfirmationDialogStub,
				},
			},
		});
		wrapper.findComponent(QCardDialogStub).vm.$emit('opened', plexServer.id);
		await flushPromises();

		// Act
		await wrapper.find('[data-cy="confirm-hide"]').trigger('click');
		await flushPromises();

		// Assert
		expect(setServerHiddenSpy).toHaveBeenCalledWith(plexServer.id, true);
		expect(closeDialogSpy).not.toHaveBeenCalled();
	});
});
