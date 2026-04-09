import { mountSuspended } from '@nuxt/test-utils/runtime';
import { flushPromises } from '@vue/test-utils';
import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { defineComponent, h } from 'vue';
import { createPinia, setActivePinia } from 'pinia';
import { generateFailedResultDTO, generateResultDTO, generateSettingsModel } from '@mock';
import { useSettingsStore } from '@store';
import { baseSetup, getAxiosMock } from '@services-test-base';

const routerPushSpy = vi.fn();

let settingsStore: ReturnType<typeof useSettingsStore>;
let mock: ReturnType<typeof getAxiosMock>;

vi.mock('#imports', async () => {
	const actual = await vi.importActual('#imports');

	return {
		...actual,
		useRouter: () => ({
			push: routerPushSpy,
		}),
		useI18n: () => ({
			t: (key: string) => key,
		}),
		useSubscription: (subscription: { unsubscribe?: () => void }) => subscription,
	};
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({
		t: (key: string) => key,
	}),
}));

const SlotStub = defineComponent({
	name: 'SlotStub',
	setup(_, { slots, attrs }) {
		return () => h('div', attrs, slots.default?.());
	},
});

const SetupFooterStub = defineComponent({
	name: 'SetupFooter',
	emits: ['finish'],
	setup(_, { emit }) {
		return () => h('button', { 'data-cy': 'finish-setup', onClick: () => emit('finish') }, 'finish');
	},
});

describe('Setup page finish flow', () => {
	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		vi.restoreAllMocks();
		routerPushSpy.mockReset();
		setActivePinia(createPinia());
		mock = getAxiosMock();
		mock.onGet('/api/PlexAccount').reply(200, generateResultDTO([]));
		mock.onGet('/api/Download').reply(200, generateResultDTO([]));
		mock.onGet('/api/FolderPath').reply(200, generateResultDTO([]));
		mock.onGet('/api/PlexLibrary').reply(200, generateResultDTO([]));
		mock.onGet('/api/Notification').reply(200, generateResultDTO([]));
		mock.onGet('/api/PlexServerConnection').reply(200, generateResultDTO([]));
		mock.onGet('/api/PlexServer').reply(200, generateResultDTO([]));
		mock.onGet('/api/Settings').reply(200, generateResultDTO(generateSettingsModel({ config: {} })));

		settingsStore = useSettingsStore();
		settingsStore.$reset();
		settingsStore.$patch({
			generalSettings: {
				firstTimeSetup: true,
			},
		});
	});

	test('Should not boot or redirect when saving setup settings fails', async () => {
		mock.onPut('/api/Settings').reply(500, generateFailedResultDTO({
			statusCode: 500,
			errors: [{ message: 'Save failed', metadata: {}, reasons: [] }],
		}));
		const wrapper = await mountSetupPage();

		await wrapper.find('[data-cy="finish-setup"]').trigger('click');
		await flushPromises();

		expect(mock.history.get.some((request) => request.url === '/api/PlexAccount')).toBe(false);
		expect(routerPushSpy).not.toHaveBeenCalled();
	});

	test('Should boot and redirect when saving setup settings succeeds', async () => {
		mock.onPut('/api/Settings').reply(200, generateResultDTO(generateSettingsModel({ config: {} })));
		const wrapper = await mountSetupPage();

		await wrapper.find('[data-cy="finish-setup"]').trigger('click');
		await flushPromises();

		expect(mock.history.get.some((request) => request.url === '/api/PlexAccount')).toBe(true);
	});
});

async function mountSetupPage() {
	const { default: SetupPage } = await import('@/pages/setup/index.vue');

	return mountSuspended(SetupPage, {
		global: {
			stubs: {
				QPage: SlotStub,
				QRow: SlotStub,
				QCol: SlotStub,
				Logo: SlotStub,
				SetupTabs: SlotStub,
				DisclaimerSetupPanel: SlotStub,
				IntroductionSetupPanel: SlotStub,
				AuthorizationSetupPanel: SlotStub,
				FolderOverviewSetupPanel: SlotStub,
				PlexAccountsSetupPanel: SlotStub,
				FinishSetupPanel: SlotStub,
				'q-tab-panels': SlotStub,
				SetupFooter: SetupFooterStub,
			},
		},
	});
}
