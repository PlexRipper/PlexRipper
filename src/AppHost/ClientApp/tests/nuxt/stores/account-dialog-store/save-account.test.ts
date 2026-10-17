import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { PlexAccountPaths, PlexLibraryPaths, PlexServerPaths } from '@api-urls';
import { generateFailedResultDTO, generatePlexAccount, generateResultDTO } from '@mock';
import { DialogType } from '@enums';
import { useAccountDialogStore, useDialogStore } from '@store';

describe('AccountDialogStore.saveAccount()', () => {
	let { mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should preserve a failed create draft and close only after a successful retry', async () => {
		// Arrange
		const account = generatePlexAccount({
			id: 99,
			partialData: {
				displayName: 'Retry account',
				isValidated: true,
			},
		});
		const accountDialogStore = useAccountDialogStore();
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		Object.assign(accountDialogStore, account, { isNewAccount: true });

		const failedResult = generateFailedResultDTO({ statusCode: 409 });
		const successfulResult = generateResultDTO(account);
		mock
			.onPost(PlexAccountPaths.createPlexAccountEndpoint())
			.replyOnce(200, failedResult)
			.onPost(PlexAccountPaths.createPlexAccountEndpoint())
			.reply(200, successfulResult);

		// Act
		const failedSave = subscribeSpyTo(accountDialogStore.saveAccount());
		await failedSave.onComplete();

		// Assert
		expect(failedSave.getFirstValue()).toEqual(failedResult);
		expect(accountDialogStore.displayName).toBe(account.displayName);
		expect(accountDialogStore.username).toBe(account.username);
		expect(accountDialogStore.savingLoading).toBe(false);
		expect(closeDialogSpy).not.toHaveBeenCalledWith(DialogType.AccountDialog);

		// Arrange successful retry refreshes
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([account]));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));

		// Act successful retry
		const successfulSave = subscribeSpyTo(accountDialogStore.saveAccount());
		await successfulSave.onComplete();

		// Assert successful retry
		expect(successfulSave.getFirstValue()).toEqual(successfulResult);
		expect(closeDialogSpy).toHaveBeenCalledWith(DialogType.AccountDialog);
		expect(accountDialogStore.savingLoading).toBe(false);
	});

	test('Should preserve a failed update draft and close only after a successful retry', async () => {
		// Arrange
		const account = generatePlexAccount({
			id: 99,
			partialData: {
				displayName: 'Updated account',
				isValidated: true,
			},
		});
		const accountDialogStore = useAccountDialogStore();
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		Object.assign(accountDialogStore, account, { isNewAccount: false });

		const failedResult = generateFailedResultDTO({ statusCode: 422 });
		const successfulResult = generateResultDTO(account);
		mock
			.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint())
			.replyOnce(200, failedResult)
			.onPut(PlexAccountPaths.updatePlexAccountByIdEndpoint())
			.reply(200, successfulResult);

		// Act
		const failedSave = subscribeSpyTo(accountDialogStore.saveAccount());
		await failedSave.onComplete();

		// Assert
		expect(failedSave.getFirstValue()).toEqual(failedResult);
		expect(accountDialogStore.displayName).toBe(account.displayName);
		expect(accountDialogStore.username).toBe(account.username);
		expect(accountDialogStore.savingLoading).toBe(false);
		expect(closeDialogSpy).not.toHaveBeenCalledWith(DialogType.AccountDialog);

		// Arrange successful retry refreshes
		mock.onGet(PlexAccountPaths.getAllPlexAccountsEndpoint()).reply(200, generateResultDTO([account]));
		mock.onGet(PlexServerPaths.getAllPlexServersEndpoint()).reply(200, generateResultDTO([]));
		mock.onGet(PlexLibraryPaths.getAllPlexLibrariesEndpoint()).reply(200, generateResultDTO([]));

		// Act successful retry
		const successfulSave = subscribeSpyTo(accountDialogStore.saveAccount());
		await successfulSave.onComplete();

		// Assert successful retry
		expect(successfulSave.getFirstValue()).toEqual(successfulResult);
		expect(closeDialogSpy).toHaveBeenCalledWith(DialogType.AccountDialog);
		expect(accountDialogStore.savingLoading).toBe(false);
	});
});
