import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { AuthenticationPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import { useAuthenticationStore, useGlobalStore } from '@store';

describe('AuthenticationStore.logout()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should clear auth state and reset global store when logout succeeds', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		authenticationStore.isLoggedIn = true;
		const globalStore = useGlobalStore();
		const resetSpy = vi.spyOn(globalStore, '$reset');

		mock.onPost(AuthenticationPaths.appUserLogOutEndpoint()).reply(200, generateResultDTO('ok'));

		// Act
		const result = subscribeSpyTo(authenticationStore.logout());
		await result.onComplete();

		// Assert
		expect(authenticationStore.isLoggedIn).toEqual(false);
		expect(resetSpy).toHaveBeenCalledOnce();
	});
});
