import { mountSuspended } from '@nuxt/test-utils/runtime';
import { describe, beforeAll, beforeEach, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { defineComponent, h, nextTick } from 'vue';
import { flushPromises } from '@vue/test-utils';
import { baseSetup, baseVars, getAxiosMock } from '@services-test-base';
import { generateFailedResultDTO, generatePlexServers } from '@mock';
import { generatePlexServerConnection } from '@factories';
import { useServerConnectionStore, useServerStore } from '@store';
import { PlexServerConnectionPaths } from '@api-urls';
import AddConnectionDialog from '../../../../src/components/Dialogs/AddConnectionDialog.vue';

vi.mock('vue-i18n', async () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
	},
});

const QInputStub = defineComponent({
	name: 'q-input',
	props: {
		modelValue: {
			type: [Number, String],
			default: '',
		},
	},
	emits: ['update:modelValue'],
	setup() {
		return () => h('input');
	},
});

const ValidationButtonStub = defineComponent({
	name: 'ValidationButton',
	props: {
		loading: {
			type: Boolean,
			default: false,
		},
		isValidated: {
			type: Boolean,
			default: false,
		},
		disabled: {
			type: Boolean,
			default: false,
		},
	},
	emits: ['click'],
	setup(props, { emit }) {
		return () => h('button', { disabled: props.disabled, onClick: () => emit('click') }, 'validate');
	},
});

const BaseButtonStub = defineComponent({
	name: 'BaseButton',
	props: {
		loading: {
			type: Boolean,
			default: false,
		},
		disabled: {
			type: Boolean,
			default: false,
		},
		label: {
			type: String,
			default: '',
		},
	},
	emits: ['click'],
	setup(props, { emit }) {
		return () => h('button', { disabled: props.disabled, onClick: () => emit('click') }, props.label);
	},
});

const DeleteButtonStub = defineComponent({
	name: 'DeleteButton',
	emits: ['click'],
	setup(_, { emit }) {
		return () => h('button', { onClick: () => emit('click') }, 'delete');
	},
});

describe('AddConnectionDialog', () => {
	let { mock, config } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		config = { plexServerCount: 1, seed: 263 };
		setActivePinia(createPinia());

		const serverStore = useServerStore();
		serverStore.servers = generatePlexServers({ config });
	});

	async function mountDialog(plexServerConnectionId = 0) {
		const closeSpy = vi.fn();
		const serverStore = useServerStore();
		const serverId = serverStore.servers[0]!.id;
		const QCardDialogStub = defineComponent({
			name: 'QCardDialog',
			emits: ['opened', 'closed'],
			setup(_, { slots }) {
				return () => h('div', [
					slots.title?.(),
					slots.default?.(),
					slots.actions?.({ close: closeSpy }),
				]);
			},
		});

		const wrapper = await mountSuspended(AddConnectionDialog, {
			global: {
				stubs: {
					QCardDialog: QCardDialogStub,
					QRow: SlotStub,
					QCol: SlotStub,
					QSpace: SlotStub,
					QConnectionIcon: SlotStub,
					ValidIcon: SlotStub,
					ValidationButton: ValidationButtonStub,
					DeleteButton: DeleteButtonStub,
					BaseButton: BaseButtonStub,
					'q-input': QInputStub,
				},
			},
		});

		wrapper.findComponent(QCardDialogStub).vm.$emit('opened', {
			plexServerId: serverId,
			plexServerConnectionId,
		});
		await nextTick();

		if (plexServerConnectionId === 0) {
			wrapper.findAllComponents(QInputStub)[0]!.vm.$emit('update:modelValue', 'http://example.com');
			await nextTick();
		}

		return { wrapper, closeSpy, serverId };
	}

	test('Should keep the dialog open and stop loading when creating a connection fails', async () => {
		// Arrange
		mock.onPost(PlexServerConnectionPaths.createPlexServerConnectionEndpoint()).reply(200, generateFailedResultDTO());
		const { wrapper, closeSpy } = await mountDialog();

		// Act
		wrapper.findComponent(BaseButtonStub).vm.$emit('click');
		await flushPromises();
		await nextTick();

		// Assert
		expect(closeSpy).not.toHaveBeenCalled();
		expect(wrapper.findComponent(BaseButtonStub).props('loading')).toBe(false);
	});

	test('Should keep the dialog open and stop loading when updating a connection fails', async () => {
		// Arrange
		const serverStore = useServerStore();
		const serverId = serverStore.servers[0]!.id;
		const connectionStore = useServerConnectionStore();
		const existingConnection = generatePlexServerConnection({ id: 9, plexServerId: serverId });
		connectionStore.serverConnections = [existingConnection];
		mock.onPatch(PlexServerConnectionPaths.updatePlexServerConnectionEndpoint()).reply(200, generateFailedResultDTO());
		const { wrapper, closeSpy } = await mountDialog(existingConnection.id);

		// Act
		wrapper.findComponent(BaseButtonStub).vm.$emit('click');
		await flushPromises();
		await nextTick();

		// Assert
		expect(closeSpy).not.toHaveBeenCalled();
		expect(wrapper.findComponent(BaseButtonStub).props('loading')).toBe(false);
	});

	test('Should keep the dialog open when deleting a connection fails', async () => {
		// Arrange
		const serverStore = useServerStore();
		const serverId = serverStore.servers[0]!.id;
		const connectionStore = useServerConnectionStore();
		const existingConnection = generatePlexServerConnection({ id: 10, plexServerId: serverId });
		connectionStore.serverConnections = [existingConnection];
		mock.onDelete(PlexServerConnectionPaths.deletePlexServerConnectionById(existingConnection.id)).reply(200, generateFailedResultDTO());
		const { wrapper, closeSpy } = await mountDialog(existingConnection.id);

		// Act
		wrapper.findComponent(DeleteButtonStub).vm.$emit('click');
		await flushPromises();
		await nextTick();

		// Assert
		expect(closeSpy).not.toHaveBeenCalled();
	});
});
