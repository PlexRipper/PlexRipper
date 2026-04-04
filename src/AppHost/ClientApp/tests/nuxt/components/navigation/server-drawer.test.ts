import { beforeEach, describe, expect, test, vi } from 'vitest';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { reactive } from 'vue';
import { of } from 'rxjs';
import ServerDrawer from '@/components/Navigation/ServerDrawer.vue';
import { generateFailedResultDTO, generatePlexServers } from '@mock';

const openRefreshPlexAccountAccessDialogSpy = vi.fn();
const server = generatePlexServers({ config: { plexServerCount: 1, seed: 263 } })[0]!;
const accountStoreMock = reactive({
	accessSyncLoading: false,
	reSyncAccount: vi.fn(() => of(generateFailedResultDTO())),
});

vi.mock('vue-i18n', () => ({
	useI18n: () => ({ t: (key: string) => key }),
}));

vi.mock('@store', async () => {
	const actual = await vi.importActual('@store');
	return {
		...actual,
		useAccountStore: () => accountStoreMock,
		useLibraryStore: () => ({ getLibrariesByServerId: vi.fn(() => []), getIsLibrarySyncing: vi.fn(() => false), getLibraryName: vi.fn(() => 'Library') }),
		useDialogStore: () => ({ openRefreshPlexAccountAccessDialog: openRefreshPlexAccountAccessDialogSpy, openServerSettingsDialog: vi.fn() }),
		useServerStore: () => ({ getVisibleServers: [server], getServerName: vi.fn(() => server.name), getServer: vi.fn(() => server) }),
		useSettingsStore: () => ({ generalSettings: { activeAccountId: 0 } }),
	};
});

vi.mock('#imports', async () => {
	const actual = await vi.importActual('#imports');
	return {
		...actual,
		useRoute: () => ({ path: '/' }),
		useRouter: () => ({ push: vi.fn() }),
	};
});

describe('ServerDrawer', () => {
	beforeEach(() => {
		accountStoreMock.accessSyncLoading = false;
		accountStoreMock.reSyncAccount = vi.fn(() => of(generateFailedResultDTO()));
		openRefreshPlexAccountAccessDialogSpy.mockReset();
	});

	test('Should not open the account access dialog when drawer refresh fails', async () => {
		// Arrange
		const wrapper = await mountSuspended(ServerDrawer, {
			shallow: true,
			global: {
				stubs: {
					QList: { template: '<div><slot /></div>' },
					QItem: { template: '<button v-bind="$attrs" @click="$emit(\'click\')"><slot /></button>' },
					QItemSection: { template: '<div><slot /></div>' },
					QExpansionItem: { template: '<div><slot /><slot name="header" /></div>' },
					QRow: { template: '<div><slot /></div>' },
					QCol: { template: '<div><slot /></div>' },
					QSpinnerDots: true,
					ServerDialog: true,
					QMediaTypeIcon: true,
				},
			},
		});

		// Act
		await wrapper.find(`[data-cy="server-drawer-item-${server.id}-no-libraries"]`).trigger('click');

		// Assert
		expect(accountStoreMock.reSyncAccount).toHaveBeenCalledWith(0);
		expect(openRefreshPlexAccountAccessDialogSpy).not.toHaveBeenCalled();
	});
});
