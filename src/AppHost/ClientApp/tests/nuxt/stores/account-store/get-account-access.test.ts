import { describe, beforeAll, beforeEach, test, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths } from '@api/api-paths';
import { generatePlexAccount, generateResultDTO } from '@mock';
import { useAccountStore } from '@store';

describe('AccountStore access getters', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should return whether an enabled account has library access', async () => {
		// Arrange
		const accessibleLibraryId = 201;
		const inaccessibleLibraryId = 202;
		const accountStore = useAccountStore();
		const accounts = [
			generatePlexAccount({
				id: 1,
				partialData: {
					isEnabled: true,
					plexServerAccess: [],
					plexLibraryAccess: [accessibleLibraryId],
				},
			}),
			generatePlexAccount({
				id: 2,
				partialData: {
					isEnabled: false,
					plexServerAccess: [],
					plexLibraryAccess: [inaccessibleLibraryId],
				},
			}),
		];
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO(accounts));

		// Act
		await subscribeSpyTo(accountStore.setup()).onComplete();

		// Assert
		expect(accountStore.getHasAccountLibraryAccess(accessibleLibraryId)).toBe(true);
		expect(accountStore.getHasAccountLibraryAccess(inaccessibleLibraryId)).toBe(false);
	});
});
