import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths } from '@api-urls';
import { generateFailedResultDTO, generateResultDTO } from '@mock';
import { useAccountDialogStore, useDialogStore } from '@store';

describe('AccountDialogStore.validateVerificationCode()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should close verification dialog, update state, and reset validation errors on success', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'user';
		accountDialogStore.password = 'pass';
		accountDialogStore.clientId = 'client-id';
		accountDialogStore.verificationCode = '123456';
		accountDialogStore.hasValidationErrors = true;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: true,
			authenticationToken: 'valid-token',
			clientId: 'client-id',
			email: 'user@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			password: 'pass',
			plexId: 99,
			title: 'User Title',
			username: 'user',
			uuid: 'account-uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(true);
		expect(accountDialogStore.hasValidationErrors).toEqual(false);
		expect(accountDialogStore.validateLoading).toEqual(false);
		expect(accountDialogStore.email).toEqual('user@test.dev');
		expect(accountDialogStore.authenticationToken).toEqual('valid-token');
		expect(closeDialogSpy).toHaveBeenCalledWith('account-verification-code-dialog');
	});

	test('Should keep verification dialog open and not update state when verification code is rejected', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'user';
		accountDialogStore.password = 'pass';
		accountDialogStore.clientId = 'client-id';
		accountDialogStore.verificationCode = 'wrong-code';
		accountDialogStore.hasValidationErrors = true;
		accountDialogStore.isValidated = false;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: true,
			authenticationToken: '',
			clientId: 'client-id',
			email: '',
			isUnAuthorized: true,
			isValidated: false,
			password: 'pass',
			plexId: 0,
			title: '',
			username: 'user',
			uuid: '',
			validatedAt: null,
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(false);
		expect(accountDialogStore.hasValidationErrors).toEqual(true);
		expect(closeDialogSpy).not.toHaveBeenCalled();
	});

	test('Should keep verification dialog open and log error when API returns unsuccessful result', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'user';
		accountDialogStore.password = 'pass';
		accountDialogStore.clientId = 'client-id';
		accountDialogStore.verificationCode = '123456';
		accountDialogStore.hasValidationErrors = true;
		accountDialogStore.isValidated = false;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateFailedResultDTO());

		// Act
		const result = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(false);
		expect(accountDialogStore.hasValidationErrors).toEqual(true);
		expect(closeDialogSpy).not.toHaveBeenCalled();
	});

	test('Should preserve account credentials in state after successful verification code submission', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'preserved-user';
		accountDialogStore.password = 'preserved-pass';
		accountDialogStore.clientId = 'preserved-client-id';
		accountDialogStore.verificationCode = '654321';
		accountDialogStore.hasValidationErrors = true;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: false,
			authenticationToken: 'new-token',
			clientId: 'preserved-client-id',
			email: 'preserved@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			password: 'preserved-pass',
			plexId: 77,
			title: 'Preserved',
			username: 'preserved-user',
			uuid: 'preserved-uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toEqual(true);
		expect(accountDialogStore.hasValidationErrors).toEqual(false);
		expect(accountDialogStore.username).toEqual('preserved-user');
		expect(accountDialogStore.password).toEqual('preserved-pass');
		expect(accountDialogStore.plexId).toEqual(77);
		expect(accountDialogStore.uuid).toEqual('preserved-uuid');
		expect(accountDialogStore.title).toEqual('Preserved');
		expect(closeDialogSpy).toHaveBeenCalledWith('account-verification-code-dialog');
	});

	test('Should clear verification code after successful submission', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'user';
		accountDialogStore.password = 'pass';
		accountDialogStore.clientId = 'client-id';
		accountDialogStore.verificationCode = '123456';
		accountDialogStore.hasValidationErrors = true;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: false,
			authenticationToken: 'token',
			clientId: 'client-id',
			email: 'user@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			password: 'pass',
			plexId: 1,
			title: 'Title',
			username: 'user',
			uuid: 'uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.verificationCode).toEqual('');
		expect(closeDialogSpy).toHaveBeenCalledWith('account-verification-code-dialog');
	});

	test('Should clear verification code after rejected submission', async () => {
		// Arrange
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.username = 'user';
		accountDialogStore.password = 'pass';
		accountDialogStore.clientId = 'client-id';
		accountDialogStore.verificationCode = 'wrong-code';
		accountDialogStore.hasValidationErrors = true;

		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, generateResultDTO({
			is2Fa: true,
			authenticationToken: '',
			clientId: 'client-id',
			email: '',
			isUnAuthorized: true,
			isValidated: false,
			password: 'pass',
			plexId: 0,
			title: '',
			username: 'user',
			uuid: '',
			validatedAt: null,
		}));

		// Act
		const result = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.verificationCode).toEqual('');
		expect(accountDialogStore.isValidated).toEqual(false);
		expect(closeDialogSpy).not.toHaveBeenCalled();
	});
});
