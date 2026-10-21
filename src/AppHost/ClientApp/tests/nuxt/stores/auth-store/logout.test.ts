import { describe, afterEach, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { AuthenticationPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import { useAccountStore, useAuthenticationStore, useGlobalStore, useSignalrStore } from '@store';

describe('AuthenticationStore.logout()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.spyOn(useRouter(), 'push').mockResolvedValue(undefined);
	});
	afterEach(() => {
		vi.restoreAllMocks();
	});

	test('Should clear auth state and reset global store when logout succeeds', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		authenticationStore.isLoggedIn = true;
		const globalStore = useGlobalStore();
		const resetSpy = vi.spyOn(globalStore, 'resetSession');

		mock.onPost(AuthenticationPaths.appUserLogOutEndpoint()).reply(200, generateResultDTO('ok'));

		// Act
		const result = subscribeSpyTo(authenticationStore.logout());
		await result.onComplete();

		// Assert
		expect(authenticationStore.isLoggedIn).toEqual(false);
		expect(resetSpy).toHaveBeenCalledOnce();
	});
	test('Should wait for live connection teardown before logout completes', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		authenticationStore.isLoggedIn = true;
		const globalStore = useGlobalStore();
		let finishReset!: () => void;
		const resetting = new Promise<void>((resolve) => {
			finishReset = resolve;
		});
		vi.spyOn(globalStore, 'resetSession').mockReturnValue(resetting);
		mock.onPost(AuthenticationPaths.appUserLogOutEndpoint()).reply(200, generateResultDTO('ok'));

		// Act
		const result = subscribeSpyTo(authenticationStore.logout());
		await vi.waitFor(() => expect(globalStore.resetSession).toHaveBeenCalledOnce());

		// Assert
		expect(result.receivedComplete()).toBe(false);
		expect(authenticationStore.isLoggedIn).toBe(true);
		finishReset();
		await result.onComplete();
		expect(authenticationStore.isLoggedIn).toBe(false);
	});

	test('Should clear state and navigate when SignalR teardown fails', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		authenticationStore.isLoggedIn = true;
		const accountStore = useAccountStore();
		accountStore.accounts = [{ id: 1 } as (typeof accountStore.accounts)[number]];
		const signalrStore = useSignalrStore();
		vi.spyOn(signalrStore, 'resetSession').mockRejectedValue(new Error('Hub stop failed'));
		const pushSpy = vi.mocked(useRouter().push);
		mock.onPost(AuthenticationPaths.appUserLogOutEndpoint()).reply(200, generateResultDTO('ok'));

		// Act
		const result = subscribeSpyTo(authenticationStore.logout());
		await result.onComplete();

		// Assert
		expect(authenticationStore.isLoggedIn).toBe(false);
		expect(accountStore.accounts).toEqual([]);
		expect(pushSpy).toHaveBeenCalledWith('/login');
	});
});
