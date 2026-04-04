import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { describe, beforeEach, expect, test, vi } from 'vitest';
import { defineComponent, h } from 'vue';
import { of, throwError } from 'rxjs';
import { generateFailedResultDTO, generatePlexServers } from '@mock';
import type { PlexServerDTO } from '@dto';
import ServerCommandsTabContent from '@components/Dialogs/ServerDialog/Tabs/ServerCommandsTabContent.vue';

const { closeDialogSpy, openDialogSpy, syncSpy, inspectSpy } = vi.hoisted(() => ({
	closeDialogSpy: vi.fn(),
	openDialogSpy: vi.fn(),
	syncSpy: vi.fn(() => of(generateFailedResultDTO())),
	inspectSpy: vi.fn(() => of(generateFailedResultDTO())),
}));

vi.mock('@api', () => ({
	plexServerApi: {
		syncPlexServerMediaEndpoint: syncSpy,
		queueInspectPlexServerJobEndpoint: inspectSpy,
	},
}));

vi.mock('@store', () => ({
	useDialogStore: () => ({
		closeDialog: closeDialogSpy,
		openDialog: openDialogSpy,
	}),
}));

const plexServer: PlexServerDTO = generatePlexServers({ config: { plexServerCount: 1, seed: 263 } })[0]!;

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots }) {
		return () => h('div', slots.default?.());
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
		return () => h('button', {
			'data-loading': props.loading ? 'true' : 'false',
			onClick: () => emit('click'),
		}, props.label);
	},
});

describe('ServerCommandsTabContent', () => {
	beforeEach(() => {
		closeDialogSpy.mockReset();
		openDialogSpy.mockReset();
		syncSpy.mockReset();
		inspectSpy.mockReset();
		syncSpy.mockReturnValue(of(generateFailedResultDTO()));
		inspectSpy.mockReturnValue(of(generateFailedResultDTO()));
	});

	test('Should not open the sync dialog when syncing a server fails', async () => {
		// Arrange
		const wrapper = await mountSuspended(ServerCommandsTabContent, {
			props: {
				plexServer,
				isVisible: true,
			},
			global: {
				stubs: {
					HelpRow: SlotStub,
					BaseButton: BaseButtonStub,
				},
			},
		});

		// Act
		wrapper.findAllComponents(BaseButtonStub)[1]!.vm.$emit('click');
		await flushPromises();

		// Assert
		expect(syncSpy).toHaveBeenCalledWith(plexServer.id);
		expect(closeDialogSpy).not.toHaveBeenCalled();
		expect(openDialogSpy).not.toHaveBeenCalled();
	});

	test('Should stop inspect loading when inspecting a server errors', async () => {
		// Arrange
		inspectSpy.mockReturnValueOnce(throwError(() => new Error('Inspect failed')));
		const wrapper = await mountSuspended(ServerCommandsTabContent, {
			props: {
				plexServer,
				isVisible: true,
			},
			global: {
				stubs: {
					HelpRow: SlotStub,
					BaseButton: BaseButtonStub,
				},
			},
		});

		// Act
		wrapper.findAllComponents(BaseButtonStub)[0]!.vm.$emit('click');
		await flushPromises();

		// Assert
		expect(inspectSpy).toHaveBeenCalledWith(plexServer.id);
		expect(wrapper.findAllComponents(BaseButtonStub)[0]!.attributes('data-loading')).toBe('false');
	});
});
