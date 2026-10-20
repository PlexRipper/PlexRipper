import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia, type Pinia } from 'pinia';
import { defineComponent, h } from 'vue';
import { flushPromises, type VueWrapper } from '@vue/test-utils';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { createI18n } from 'vue-i18n';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateFailedResultDTO, generatePlexServers, generateResultDTO } from '@mock';
import { PlexServerPaths } from '@api-urls';
import { useDialogStore, useServerStore } from '@store';
import { DialogType } from '@enums';
import type { PlexServerDTO } from '@dto';
import ServerConfigTabContent from '@/components/Dialogs/ServerDialog/Tabs/ServerConfigTabContent.vue';
import messages from '@/lang/en-US.json';

const Confirmation = defineComponent({
	props: { confirmLoading: Boolean },
	emits: ['confirm'],
	setup(props, { emit }) {
		return () => h('button', {
			'data-cy': 'confirm-disable', disabled: props.confirmLoading, onClick: () => emit('confirm'),
		}, props.confirmLoading ? 'Disabling' : 'Confirm');
	},
});

describe('ServerConfigTabContent - disable recovery', () => {
	let { mock } = baseVars();
	let pinia: Pinia;
	let wrapper: VueWrapper | undefined;

	beforeAll(() => baseSetup());
	beforeEach(() => {
		mock = getAxiosMock();
		pinia = createPinia();
		setActivePinia(pinia);
	});
	afterEach(() => {
		wrapper?.unmount();
		wrapper = undefined;
		mock.restore();
		vi.restoreAllMocks();
	});

	async function render(server: PlexServerDTO) {
		wrapper = await mountSuspended(ServerConfigTabContent, {
			props: { plexServer: server },
			global: {
				plugins: [pinia, createI18n({ legacy: false, locale: 'en-US', messages: { 'en-US': messages } })],
				stubs: {
					HelpGroup: { template: '<section><slot /></section>' },
					HelpRow: { template: '<div><slot /></div>' },
					QToggle: true, DownloadLimitInput: true, ConfirmationDialog: Confirmation,
				},
			},
		});
		return wrapper;
	}

	test('Should retain both dialogs after failed results and transport errors, then close only after retry succeeds', async () => {
		// Arrange
		const server = generatePlexServers({ config: { seed: 55001, plexServerCount: 1 } })[0]!;
		server.isEnabled = true;
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([server]));
		const store = useServerStore();
		await subscribeSpyTo(store.setup()).onComplete();
		const closeDialog = vi.spyOn(useDialogStore(), 'closeDialog');
		const endpoint = PlexServerPaths.setServerEnabledEndpoint(server.id);
		const success = generateResultDTO({ ...server, isEnabled: false });
		const retry = Promise.withResolvers<[number, typeof success]>();
		mock.onPut(endpoint).replyOnce(200, generateFailedResultDTO())
			.onPut(endpoint).networkErrorOnce()
			.onPut(endpoint).reply(() => retry.promise);
		const rendered = await render(server);
		const confirm = () => rendered.get('[data-cy="confirm-disable"]');

		// Act: a normal failure emission must not close either dialog.
		await confirm().trigger('click');
		await flushPromises();

		// Assert
		expect(closeDialog).not.toHaveBeenCalled();
		expect(confirm().attributes('disabled')).toBeUndefined();
		expect(store.getServer(server.id)?.isEnabled).toBe(true);

		// Act: API transport errors are also converted into failed emissions.
		await confirm().trigger('click');
		await flushPromises();

		// Assert
		expect(closeDialog).not.toHaveBeenCalled();
		expect(confirm().attributes('disabled')).toBeUndefined();
		expect(store.getServer(server.id)?.isEnabled).toBe(true);

		// Act: leave the retry pending before delivering success.
		await confirm().trigger('click');

		// Assert
		expect(confirm().attributes('disabled')).toBeDefined();
		expect(closeDialog).not.toHaveBeenCalled();
		retry.resolve([200, success]);
		await flushPromises();
		expect(confirm().attributes('disabled')).toBeUndefined();
		expect(store.getServer(server.id)?.isEnabled).toBe(false);
		expect(closeDialog.mock.calls).toEqual([
			[DialogType.ServerHideConfirmationDialog], [DialogType.ServerSettingsDialog],
		]);
		expect(mock.history.put.map((request) => [request.url, JSON.parse(request.data)])).toEqual([
			[endpoint, { isEnabled: false }], [endpoint, { isEnabled: false }], [endpoint, { isEnabled: false }],
		]);
	});
});
