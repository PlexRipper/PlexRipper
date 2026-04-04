import { beforeEach, describe, expect, test, vi } from 'vitest';
import { mountSuspended } from '@nuxt/test-utils/runtime';
import { reactive } from 'vue';
import { of } from 'rxjs';
import AccountSelector from '@/components/AppBar/AccountSelector.vue';
import { generateFailedResultDTO, generateResultDTO } from '@mock';

const openRefreshPlexAccountAccessDialogSpy = vi.fn();
const accountStoreMock = reactive({
	accessSyncLoading: false,
	accounts: [],
	getAccountDisplayName: vi.fn(() => 'Account'),
	getAccountUserName: vi.fn(() => 'user'),
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
		useSettingsStore: () => ({ generalSettings: { activeAccountId: 0 } }),
		useAuthenticationStore: () => ({ logout: vi.fn(() => of(generateResultDTO({}))) }),
	};
});

vi.mock('#imports', async () => {
	const actual = await vi.importActual('#imports');
	return {
		...actual,
		useDialogStore: () => ({ openRefreshPlexAccountAccessDialog: openRefreshPlexAccountAccessDialogSpy }),
	};
});

describe('AccountSelector', () => {
	beforeEach(() => {
		accountStoreMock.accounts = [];
		accountStoreMock.accessSyncLoading = false;
		accountStoreMock.reSyncAccount = vi.fn(() => of(generateFailedResultDTO()));
		openRefreshPlexAccountAccessDialogSpy.mockReset();
	});

	test('Should not open the account access dialog when refresh fails', async () => {
		// Arrange
		const wrapper = await mountSuspended(AccountSelector, {
			global: {
				stubs: {
					QBtn: { template: '<button v-bind="$attrs" @click="$emit(\'click\')"><slot /></button>' },
					QMenu: { template: '<div><slot /></div>' },
					QList: { template: '<div><slot /></div>' },
					QItem: { template: '<button v-bind="$attrs" @click="$emit(\'click\')"><slot /></button>' },
					QItemSection: { template: '<div><slot /></div>' },
					QItemLabel: { template: '<div><slot /></div>' },
					QSeparator: true,
					QIcon: true,
				},
			},
		});

		// Act
		await wrapper.find('[data-cy="refresh-account-0-btn"]').trigger('click');

		// Assert
		expect(accountStoreMock.reSyncAccount).toHaveBeenCalledWith(0);
		expect(openRefreshPlexAccountAccessDialogSpy).not.toHaveBeenCalled();
	});
});
