import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths, PlexLibraryPaths, PlexServerPaths } from '@api-urls';
import { generateFailedResultDTO, generatePlexAccount, generateResultDTO } from '@mock';
import type {
	PlexAccountDTO,
	ValidatePlexCredentialsDTO,
	ValidatePlexTokenEndpointResponse,
} from '@dto';
import { useAccountDialogStore, useAccountStore, useDialogStore } from '@store';
import { DialogType } from '@enums';

describe('AccountDialogStore credential freshness', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	function makeAccount(id: number, partialData: Partial<PlexAccountDTO> = {}): PlexAccountDTO {
		return generatePlexAccount({
			id,
			config: { seed: id + 100 },
			partialData: {
				customAuthenticationToken: '',
				authenticationToken: `generated-token-${id}`,
				clientId: `client-${id}`,
				displayName: `Account ${id}`,
				email: `account-${id}@test.dev`,
				hasPassword: true,
				is2Fa: false,
				isEnabled: true,
				isMain: false,
				isValidated: true,
				password: `password-${id}`,
				plexId: id * 10,
				title: `Title ${id}`,
				username: `user-${id}`,
				uuid: `uuid-${id}`,
				validatedAt: '2026-04-03T00:00:00Z',
				verificationCode: '',
				...partialData,
			},
		});
	}

	function makeCredentialsResult(overrides: Partial<ValidatePlexCredentialsDTO> = {}) {
		return generateResultDTO({
			is2Fa: false,
			authenticationToken: 'validated-authentication-token',
			clientId: 'validated-client-id',
			email: 'validated@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			password: 'validated-password',
			plexId: 42,
			title: 'Validated title',
			username: 'validated-user',
			uuid: 'validated-uuid',
			validatedAt: '2026-04-03T00:00:00Z',
			...overrides,
		});
	}

	function makeTokenResult(overrides: Partial<ValidatePlexTokenEndpointResponse> = {}) {
		return generateResultDTO({
			is2Fa: false,
			clientId: 'validated-token-client-id',
			customAuthenticationToken: 'validated-custom-token',
			email: 'token-user@test.dev',
			isUnAuthorized: false,
			isValidated: true,
			plexId: 84,
			title: 'Token title',
			username: 'token-user',
			uuid: 'token-uuid',
			validatedAt: '2026-04-04T00:00:00Z',
			...overrides,
		});
	}

	function openSavedAccount(account: PlexAccountDTO) {
		const accountStore = useAccountStore();
		accountStore.accounts = [account];
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: account.id });
		return accountDialogStore;
	}

	function mockRefreshes(account: PlexAccountDTO) {
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([account]));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));
	}

	function readRequestBody<T>(requestData: unknown): T {
		return typeof requestData === 'string' ? JSON.parse(requestData) as T : requestData as T;
	}

	test('Should retain validated authentication fields for a metadata-only edit of a loaded account', async () => {
		// Arrange
		const account = makeAccount(42);
		const accountDialogStore = openSavedAccount(account);
		accountDialogStore.displayName = 'Renamed account';
		accountDialogStore.isEnabled = false;
		accountDialogStore.isMain = true;
		const savedAccount = { ...account, displayName: 'Renamed account', isEnabled: false, isMain: true };
		mock.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(accountDialogStore.isValidated).toBe(true);
		expect(readRequestBody<PlexAccountDTO>(mock.history.put[0]?.data)).toEqual(savedAccount);
	});

	test('Should keep an unvalidated loaded account usable for a metadata-only edit', async () => {
		// Arrange
		const account = makeAccount(43, { isValidated: false, validatedAt: null });
		const accountDialogStore = openSavedAccount(account);
		accountDialogStore.displayName = 'Unvalidated metadata edit';
		const savedAccount = { ...account, displayName: 'Unvalidated metadata edit' };
		mock.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(accountDialogStore.isValidated).toBe(false);
		expect(readRequestBody<PlexAccountDTO>(mock.history.put[0]?.data)).toEqual(savedAccount);
	});

	test.each(['username', 'password'] as const)('Should invalidate credential proof and block saving when a loaded %s changes', async (field) => {
		// Arrange
		const accountDialogStore = openSavedAccount(makeAccount(44));
		const changedInput = `changed-${field}`;
		accountDialogStore[field] = changedInput;

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toBe(false);
		expect(accountDialogStore.validatedAt).toBeNull();
		expect(accountDialogStore.authenticationToken).toBe('');
		expect(result.getFirstValue()).toEqual({
			isSuccess: false,
			statusCode: 400,
			errors: [{ message: expect.any(String), reasons: [], metadata: {} }],
			successes: [],
		});
		expect(result.getFirstValue()?.value).toBeUndefined();
		expect(mock.history.put).toHaveLength(0);
		expect(accountDialogStore[field]).toBe(changedInput);
	});

	test('Should restore saving after current credential validation of changed loaded inputs', async () => {
		// Arrange
		const originalAccount = makeAccount(451);
		const accountDialogStore = openSavedAccount(originalAccount);
		accountDialogStore.username = 'changed-loaded-user';
		accountDialogStore.password = 'changed-loaded-password';
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, makeCredentialsResult({
			authenticationToken: 'revalidated-authentication-token',
			clientId: 'revalidated-client-id',
			email: 'revalidated@test.dev',
			password: 'changed-loaded-password',
			plexId: 4510,
			title: 'Revalidated title',
			username: 'canonical-loaded-user',
			uuid: 'revalidated-uuid',
			validatedAt: '2026-04-05T00:00:00Z',
		}));
		const savedAccount = {
			...originalAccount,
			authenticationToken: 'revalidated-authentication-token',
			clientId: 'revalidated-client-id',
			email: 'revalidated@test.dev',
			isValidated: true,
			password: 'changed-loaded-password',
			plexId: 4510,
			title: 'Revalidated title',
			username: 'canonical-loaded-user',
			uuid: 'revalidated-uuid',
			validatedAt: '2026-04-05T00:00:00Z',
		};

		// Act
		const validation = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		await validation.onComplete();
		mock.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toBe(true);
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(readRequestBody<PlexAccountDTO>(mock.history.put[0]?.data)).toEqual(savedAccount);
	});

	test('Should invalidate token proof and block saving when a loaded custom token changes', async () => {
		// Arrange
		const accountDialogStore = openSavedAccount(makeAccount(46, {
			authenticationToken: '',
			customAuthenticationToken: 'old-custom-token',
		}));
		accountDialogStore.customAuthenticationToken = 'changed-custom-token';

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toBe(false);
		expect(accountDialogStore.validatedAt).toBeNull();
		expect(result.getFirstValue()).toEqual({
			isSuccess: false,
			statusCode: 400,
			errors: [{ message: expect.any(String), reasons: [], metadata: {} }],
			successes: [],
		});
		expect(result.getFirstValue()?.value).toBeUndefined();
		expect(mock.history.put).toHaveLength(0);
		expect(accountDialogStore.customAuthenticationToken).toBe('changed-custom-token');
	});

	test('Should save fresh credential validation using provider metadata and the generated token', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.displayName = 'Fresh credential account';
		accountDialogStore.username = 'entered-user';
		accountDialogStore.password = 'entered-password';
		const validationResponse = makeCredentialsResult({
			authenticationToken: 'fresh-generated-token',
			clientId: 'fresh-client-id',
			email: 'fresh@test.dev',
			password: 'entered-password',
			plexId: 101,
			title: 'Fresh title',
			username: 'canonical-user',
			uuid: 'fresh-uuid',
		});
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, validationResponse);
		const savedAccount = makeAccount(47, {
			authenticationToken: 'fresh-generated-token',
			clientId: 'fresh-client-id',
			displayName: 'Fresh credential account',
			email: 'fresh@test.dev',
			isValidated: true,
			password: 'entered-password',
			plexId: 101,
			title: 'Fresh title',
			username: 'canonical-user',
			uuid: 'fresh-uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		});

		// Act
		const validation = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		await validation.onComplete();
		mock.onPost(PlexAccountPaths.createPlexAccountEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toBe(true);
		expect(accountDialogStore.username).toBe('canonical-user');
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(readRequestBody<PlexAccountDTO>(mock.history.post[1]?.data)).toEqual({
			isValidated: true,
			password: 'entered-password',
			username: 'canonical-user',
			uuid: 'fresh-uuid',
			validatedAt: '2026-04-03T00:00:00Z',
			customAuthenticationToken: '',
			authenticationToken: 'fresh-generated-token',
			clientId: 'fresh-client-id',
			displayName: 'Fresh credential account',
			email: 'fresh@test.dev',
			is2Fa: false,
			isEnabled: true,
			isMain: true,
			plexId: 101,
			title: 'Fresh title',
		});
	});

	test('Should accept a successful token validation when Plex returns the same token and save token-mode metadata', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.switchInputMode(true);
		accountDialogStore.displayName = 'Fresh token account';
		accountDialogStore.customAuthenticationToken = 'same-token';
		const validationResponse = makeTokenResult({
			clientId: 'same-token-client-id',
			customAuthenticationToken: 'same-token',
			email: 'same-token@test.dev',
			plexId: 102,
			title: 'Same token title',
			username: 'token-profile-user',
			uuid: 'same-token-uuid',
		});
		mock.onPost(PlexAccountPaths.validatePlexTokenEndpoint()).reply(200, validationResponse);
		const savedAccount = makeAccount(48, {
			authenticationToken: '',
			clientId: 'same-token-client-id',
			customAuthenticationToken: 'same-token',
			displayName: 'Fresh token account',
			email: 'same-token@test.dev',
			isValidated: true,
			password: '',
			plexId: 102,
			title: 'Same token title',
			username: 'token-profile-user',
			uuid: 'same-token-uuid',
			validatedAt: '2026-04-04T00:00:00Z',
		});

		// Act
		const validation = subscribeSpyTo(accountDialogStore.validatePlexToken());
		await validation.onComplete();
		mock.onPost(PlexAccountPaths.createPlexAccountEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		expect(accountDialogStore.isValidated).toBe(true);
		expect(accountDialogStore.customAuthenticationToken).toBe('same-token');
		expect(accountDialogStore.username).toBe('token-profile-user');
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(readRequestBody<PlexAccountDTO>(mock.history.post[1]?.data)).toEqual({
			isValidated: true,
			password: '',
			username: 'token-profile-user',
			uuid: 'same-token-uuid',
			validatedAt: '2026-04-04T00:00:00Z',
			customAuthenticationToken: 'same-token',
			authenticationToken: '',
			clientId: 'same-token-client-id',
			displayName: 'Fresh token account',
			email: 'same-token@test.dev',
			is2Fa: false,
			isEnabled: true,
			isMain: true,
			plexId: 102,
			title: 'Same token title',
		});
	});

	test('Should clear credential validation synchronously when a new account switches authentication mode', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.username = 'switch-user';
		accountDialogStore.password = 'switch-password';
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, makeCredentialsResult({
			authenticationToken: 'switch-generated-token',
			password: 'switch-password',
			username: 'switch-user',
		}));
		await subscribeSpyTo(accountDialogStore.validatePlexAccount()).onComplete();

		// Act
		accountDialogStore.switchInputMode(true);

		// Assert
		expect(accountDialogStore.isAuthTokenMode).toBe(true);
		expect(accountDialogStore.isValidated).toBe(false);
		expect(accountDialogStore.validatedAt).toBeNull();
		expect(accountDialogStore.authenticationToken).toBe('');
	});

	test('Should exclude inactive password and generated-token state when saving a token-mode account', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.username = 'temporary-user';
		accountDialogStore.password = 'temporary-password';
		accountDialogStore.customAuthenticationToken = 'temporary-token';
		accountDialogStore.switchInputMode(true);
		accountDialogStore.displayName = 'Token mode account';
		accountDialogStore.customAuthenticationToken = 'active-token';
		const savedAccount = makeAccount(49, {
			authenticationToken: '',
			clientId: 'token-mode-client-id',
			customAuthenticationToken: 'active-token',
			displayName: 'Token mode account',
			email: 'token-mode@test.dev',
			isValidated: true,
			password: '',
			plexId: 103,
			title: 'Token mode title',
			username: 'token-mode-user',
			uuid: 'token-mode-uuid',
			validatedAt: '2026-04-04T00:00:00Z',
		});
		mock.onPost(PlexAccountPaths.validatePlexTokenEndpoint()).reply(200, makeTokenResult({
			clientId: 'token-mode-client-id',
			customAuthenticationToken: 'active-token',
			email: 'token-mode@test.dev',
			plexId: 103,
			title: 'Token mode title',
			username: 'token-mode-user',
			uuid: 'token-mode-uuid',
		}));
		await subscribeSpyTo(accountDialogStore.validatePlexToken()).onComplete();
		accountDialogStore.password = 'inactive-password';
		accountDialogStore.authenticationToken = 'inactive-generated-token';
		mock.onPost(PlexAccountPaths.createPlexAccountEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		const body = readRequestBody<PlexAccountDTO>(mock.history.post[1]?.data);
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(body.password).toBe('');
		expect(body.authenticationToken).toBe('');
		expect(body.customAuthenticationToken).toBe('active-token');
		expect(body.username).toBe('token-mode-user');
	});

	test('Should omit inactive custom-token state after switching a new account back to credential mode', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.switchInputMode(true);
		accountDialogStore.customAuthenticationToken = 'inactive-token';
		accountDialogStore.switchInputMode(false);
		accountDialogStore.displayName = 'Credential mode account';
		accountDialogStore.username = 'credential-mode-user';
		accountDialogStore.password = 'credential-mode-password';
		const savedAccount = makeAccount(50, {
			clientId: 'credential-mode-client-id',
			displayName: 'Credential mode account',
			email: 'credential-mode@test.dev',
			isValidated: true,
			password: 'credential-mode-password',
			plexId: 104,
			title: 'Credential mode title',
			username: 'credential-mode-user',
			uuid: 'credential-mode-uuid',
			validatedAt: '2026-04-03T00:00:00Z',
		});
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint()).reply(200, makeCredentialsResult({
			authenticationToken: 'credential-mode-generated-token',
			clientId: 'credential-mode-client-id',
			email: 'credential-mode@test.dev',
			password: 'credential-mode-password',
			plexId: 104,
			title: 'Credential mode title',
			username: 'credential-mode-user',
			uuid: 'credential-mode-uuid',
		}));
		await subscribeSpyTo(accountDialogStore.validatePlexAccount()).onComplete();
		accountDialogStore.customAuthenticationToken = 'inactive-token';
		mock.onPost(PlexAccountPaths.createPlexAccountEndpoint()).reply(200, generateResultDTO(savedAccount));
		mockRefreshes(savedAccount);

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount());
		await result.onComplete();

		// Assert
		const body = readRequestBody<PlexAccountDTO>(mock.history.post[1]?.data);
		expect(result.getFirstValue()).toEqual(generateResultDTO(savedAccount));
		expect(body.customAuthenticationToken).toBe('');
		expect(body.password).toBe('credential-mode-password');
		expect(body.authenticationToken).toBe('credential-mode-generated-token');
	});

	test('Should ignore stale credential success after edited inputs start a newer validation', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.displayName = 'Credential race';
		accountDialogStore.username = 'user-a';
		accountDialogStore.password = 'password-a';
		let resolveFirst: (response: [number, unknown]) => void = () => undefined;
		let resolveSecond: (response: [number, unknown]) => void = () => undefined;
		const firstResponse = new Promise<[number, unknown]>((resolve) => {
			resolveFirst = resolve;
		});
		const secondResponse = new Promise<[number, unknown]>((resolve) => {
			resolveSecond = resolve;
		});
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.replyOnce(() => firstResponse)
			.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.reply(() => secondResponse);

		// Act
		const firstValidation = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		accountDialogStore.username = 'user-b';
		accountDialogStore.password = 'password-b';
		const secondValidation = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		resolveFirst([200, makeCredentialsResult({ username: 'stale-canonical-user', password: 'password-a' })]);
		await firstValidation.onComplete();

		// Assert stale result and finalizer do not touch current input or loading
		expect(accountDialogStore.username).toBe('user-b');
		expect(accountDialogStore.password).toBe('password-b');
		expect(accountDialogStore.isValidated).toBe(false);
		expect(accountDialogStore.validatedAt).toBeNull();
		expect(accountDialogStore.validateLoading).toBe(true);
		expect(mock.history.post).toHaveLength(2);
		expect(readRequestBody<ValidatePlexCredentialsDTO>(mock.history.post[1]?.data).username).toBe('user-b');

		resolveSecond([200, makeCredentialsResult({ username: 'current-canonical-user', password: 'password-b' })]);
		await secondValidation.onComplete();
		await firstValidation.onComplete();
		expect(firstValidation.getValues()).toEqual([]);
		expect(accountDialogStore.username).toBe('current-canonical-user');
		expect(accountDialogStore.isValidated).toBe(true);
		expect(accountDialogStore.validateLoading).toBe(false);
	});

	test('Should ignore stale credential failure after closing and reopening a different account', async () => {
		// Arrange
		const firstAccount = makeAccount(51);
		const secondAccount = makeAccount(52, { isValidated: false, validatedAt: null });
		const accountStore = useAccountStore();
		accountStore.accounts = [firstAccount, secondAccount];
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: firstAccount.id });
		const dialogStore = useDialogStore();
		const openDialogSpy = vi.spyOn(dialogStore, 'openDialog');
		let resolveFirst: (response: [number, unknown]) => void = () => undefined;
		let resolveSecond: (response: [number, unknown]) => void = () => undefined;
		const firstResponse = new Promise<[number, unknown]>((resolve) => {
			resolveFirst = resolve;
		});
		const secondResponse = new Promise<[number, unknown]>((resolve) => {
			resolveSecond = resolve;
		});
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.replyOnce(() => firstResponse)
			.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.reply(() => secondResponse);

		// Act
		const firstValidation = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		accountDialogStore.closeDialog();
		accountDialogStore.openDialog({ accountId: secondAccount.id });
		const secondValidation = subscribeSpyTo(accountDialogStore.validatePlexAccount());
		resolveFirst([200, generateFailedResultDTO({ statusCode: 401 })]);
		await firstValidation.onComplete();

		// Assert stale failure cannot open a dialog, overwrite account B, or clear B loading
		expect(accountDialogStore.id).toBe(secondAccount.id);
		expect(accountDialogStore.username).toBe(secondAccount.username);
		expect(accountDialogStore.hasValidationErrors).toBe(false);
		expect(accountDialogStore.validateLoading).toBe(true);
		expect(openDialogSpy).not.toHaveBeenCalledWith(DialogType.AccountTokenValidateDialog);

		resolveSecond([200, makeCredentialsResult({ username: 'second-account-user', password: secondAccount.password })]);
		await secondValidation.onComplete();
		await firstValidation.onComplete();
		expect(firstValidation.getValues()).toEqual([]);
		expect(accountDialogStore.username).toBe('second-account-user');
		expect(accountDialogStore.validateLoading).toBe(false);
	});

	test('Should ignore stale token errors after reset and keep the current token validation pending', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.switchInputMode(true);
		accountDialogStore.displayName = 'Token A';
		accountDialogStore.customAuthenticationToken = 'token-a';
		const dialogStore = useDialogStore();
		const openDialogSpy = vi.spyOn(dialogStore, 'openDialog');
		let rejectFirst: (error: unknown) => void = () => undefined;
		let resolveSecond: (response: [number, unknown]) => void = () => undefined;
		const firstResponse = new Promise<[number, unknown]>((_resolve, reject) => {
			rejectFirst = reject;
		});
		const secondResponse = new Promise<[number, unknown]>((resolve) => {
			resolveSecond = resolve;
		});
		mock.onPost(PlexAccountPaths.validatePlexTokenEndpoint())
			.replyOnce(() => firstResponse)
			.onPost(PlexAccountPaths.validatePlexTokenEndpoint())
			.reply(() => secondResponse);

		// Act
		const firstValidation = subscribeSpyTo(accountDialogStore.validatePlexToken());
		accountDialogStore.closeDialog();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.switchInputMode(true);
		accountDialogStore.displayName = 'Token B';
		accountDialogStore.customAuthenticationToken = 'token-b';
		const secondValidation = subscribeSpyTo(accountDialogStore.validatePlexToken());
		rejectFirst(new Error('stale network error'));
		await firstValidation.onComplete();

		// Assert stale error cannot overwrite the reopened draft or clear B loading
		expect(accountDialogStore.displayName).toBe('Token B');
		expect(accountDialogStore.customAuthenticationToken).toBe('token-b');
		expect(accountDialogStore.hasValidationErrors).toBe(false);
		expect(accountDialogStore.validateLoading).toBe(true);
		expect(openDialogSpy).not.toHaveBeenCalledWith(DialogType.AccountTokenValidateDialog);

		resolveSecond([200, generateFailedResultDTO({ statusCode: 401 })]);
		await secondValidation.onComplete();
		await firstValidation.onComplete();
		expect(firstValidation.getValues()).toEqual([]);
		expect(accountDialogStore.hasValidationErrors).toBe(true);
		expect(accountDialogStore.validateLoading).toBe(false);
	});

	test('Should ignore stale 2FA validation success after reopening the dialog for another draft', async () => {
		// Arrange
		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.username = 'two-fa-a';
		accountDialogStore.password = 'password-a';
		accountDialogStore.clientId = 'client-a';
		accountDialogStore.verificationCode = '111111';
		let resolveFirst: (response: [number, unknown]) => void = () => undefined;
		let resolveSecond: (response: [number, unknown]) => void = () => undefined;
		const firstResponse = new Promise<[number, unknown]>((resolve) => {
			resolveFirst = resolve;
		});
		const secondResponse = new Promise<[number, unknown]>((resolve) => {
			resolveSecond = resolve;
		});
		mock.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.replyOnce(() => firstResponse)
			.onPost(PlexAccountPaths.validatePlexCredentialsEndpoint())
			.reply(() => secondResponse);
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');

		// Act
		const firstValidation = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		accountDialogStore.closeDialog();
		accountDialogStore.openDialog({ accountId: 0 });
		accountDialogStore.username = 'two-fa-b';
		accountDialogStore.password = 'password-b';
		accountDialogStore.clientId = 'client-b';
		accountDialogStore.verificationCode = '222222';
		closeDialogSpy.mockClear();
		const secondValidation = subscribeSpyTo(accountDialogStore.validateVerificationCode());
		resolveFirst([200, makeCredentialsResult({
			is2Fa: true,
			password: 'password-a',
			username: 'two-fa-a',
		})]);
		await firstValidation.onComplete();

		// Assert stale 2FA success cannot close or overwrite the current verification draft
		expect(accountDialogStore.username).toBe('two-fa-b');
		expect(accountDialogStore.password).toBe('password-b');
		expect(accountDialogStore.verificationCode).toBe('222222');
		expect(accountDialogStore.isValidated).toBe(false);
		expect(accountDialogStore.validatedAt).toBeNull();
		expect(accountDialogStore.validateLoading).toBe(true);
		expect(closeDialogSpy).not.toHaveBeenCalledWith(DialogType.AccountVerificationCodeDialog);

		resolveSecond([200, makeCredentialsResult({ is2Fa: true, username: 'two-fa-b-canonical', password: 'password-b' })]);
		await secondValidation.onComplete();
		await firstValidation.onComplete();
		expect(firstValidation.getValues()).toEqual([]);
		expect(accountDialogStore.username).toBe('two-fa-b-canonical');
		expect(accountDialogStore.isValidated).toBe(true);
		expect(accountDialogStore.validateLoading).toBe(false);
		expect(closeDialogSpy).toHaveBeenCalledWith(DialogType.AccountVerificationCodeDialog);
	});
});
