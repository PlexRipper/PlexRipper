import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths } from '@api-urls';
import { generateResultDTO } from '@mock';
import { useAccountDialogStore, useDialogStore } from '@store';

describe('AccountDialogStore.validatePlexAccount()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should keep account validated and not open verification dialog when account is validated with 2FA', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const openDialogSpy = vi.spyOn(dialogStore, 'openDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'valid-user';
		accountDialogStore.password = 'valid-pass';

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: true,
			authenticationToken: 'token',
			clientId: 'client-id',
			email: 'mail@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			password: 'valid-pass',
			plexId: 42,
			title: 'Title',
			username: 'valid-user',
			uuid: 'uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(true);
		expect(accountDialogStore.validateLoading).toEqual(false);
		expect(openDialogSpy).not.toHaveBeenCalledWith('account-verification-code-dialog');
	});

	test('Should open verification code dialog when account has 2FA and is not validated', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const openDialogSpy = vi.spyOn(dialogStore, 'openDialog');
		const accountDialogStore = useAccountDialogStore();

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: true,
			authenticationToken: 'token',
			clientId: 'client-id',
			email: 'mail@test.dev',
			isUnAuthorized: false,
			isValidated: false,
			password: 'valid-pass',
			plexId: 42,
			title: 'Title',
			username: 'valid-user',
			uuid: 'uuid',
			validatedAt: null,
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.validateLoading).toEqual(false);
		expect(openDialogSpy).toHaveBeenCalledWith('account-verification-code-dialog');
	});

	test('Should surface validation feedback when account credentials are unauthorized without 2FA', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const openDialogSpy = vi.spyOn(dialogStore, 'openDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'wrong-user';
		accountDialogStore.password = 'wrong-pass';

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: false,
			authenticationToken: '',
			clientId: 'client-id',
			email: 'mail@test.dev',
			isUnAuthorized: true,
			isValidated: false,
			password: 'wrong-pass',
			plexId: 42,
			title: 'Title',
			username: 'wrong-user',
			uuid: 'uuid',
			validatedAt: null,
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(false);
		expect(accountDialogStore.hasValidationErrors).toEqual(true);
		expect(accountDialogStore.validateLoading).toEqual(false);
		expect(openDialogSpy).toHaveBeenCalledWith('account-token-validate-dialog');
	});

	test('Should clear previous validation errors when account credentials validate successfully without 2FA', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const openDialogSpy = vi.spyOn(dialogStore, 'openDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.hasValidationErrors = true;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: false,
			authenticationToken: 'token',
			clientId: 'client-id',
			email: 'mail@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			password: 'valid-pass',
			plexId: 42,
			title: 'Title',
			username: 'valid-user',
			uuid: 'uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(true);
		expect(accountDialogStore.hasValidationErrors).toEqual(false);
		expect(accountDialogStore.validateLoading).toEqual(false);
		expect(openDialogSpy).not.toHaveBeenCalledWith('account-token-validate-dialog');
	});
});
