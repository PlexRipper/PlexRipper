import { beforeAll, beforeEach, describe, expect, test, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { AuthenticationPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { StoreNames, type ISetupResult } from '@interfaces';
import { useAuthenticationStore } from '@store';
import { throwError } from 'rxjs';
import { authenticationApi } from '@api';

describe('AuthenticationStore.setup()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
		vi.restoreAllMocks();
	});

	test('Should report success when status returns an unauthenticated user', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		const setupResult: ISetupResult = {
			isSuccess: true,
			name: StoreNames.AuthenticationStore,
		};
		mock.onGet(AuthenticationPaths.authenticationStatusEndpoint()).reply(200, generateResultDTO({
			claims: [],
			isLoggedIn: false,
			userName: 'guest',
		}));

		// Act
		const result = subscribeSpyTo(authenticationStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(authenticationStore.isLoggedIn).toEqual(false);
	});

	test('Should report failure when status request fails', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.AuthenticationStore,
		};
		mock.onGet(AuthenticationPaths.authenticationStatusEndpoint()).reply(200, generateFailedResultDTO({
			statusCode: 500,
		}));

		// Act
		const result = subscribeSpyTo(authenticationStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(authenticationStore.isLoggedIn).toEqual(false);
	});

	test('Should report failure when status request throws an error', async () => {
		// Arrange
		const authenticationStore = useAuthenticationStore();
		const setupResult: ISetupResult = {
			isSuccess: false,
			name: StoreNames.AuthenticationStore,
		};
		vi.spyOn(authenticationApi, 'authenticationStatusEndpoint').mockReturnValue(
			throwError(() => new Error('Status failed')) as never,
		);

		// Act
		const result = subscribeSpyTo(authenticationStore.setup());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(setupResult);
		expect(authenticationStore.isLoggedIn).toEqual(false);
	});
});
