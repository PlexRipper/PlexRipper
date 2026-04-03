import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { throwError } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { useAccountDialogStore, useAccountStore, useDialogStore } from '@store';

describe('AccountDialogStore.saveAccount()', () => {
	let { mock: _mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		_mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should stop saving loading and keep dialog open when create account fails', async () => {
		// Arrange
		const accountStore = useAccountStore();
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		vi
			.spyOn(accountStore, 'createPlexAccount')
			.mockReturnValue(throwError(() => new Error('Create account failed')) as never);

		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.isNewAccount = true;
		accountDialogStore.displayName = 'Test Account';
		accountDialogStore.isValidated = true;

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount(), { expectErrors: true });
		await result.onError();

		// Assert
		expect(result.receivedError()).toEqual(true);
		expect(accountDialogStore.savingLoading).toEqual(false);
		expect(closeDialogSpy).not.toHaveBeenCalledWith('account-dialog');
	});

	test('Should stop saving loading and keep dialog open when update account fails', async () => {
		// Arrange
		const accountStore = useAccountStore();
		const dialogStore = useDialogStore();
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');
		vi
			.spyOn(accountStore, 'updatePlexAccount')
			.mockReturnValue(throwError(() => new Error('Update account failed')) as never);

		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.id = 42;
		accountDialogStore.isNewAccount = false;

		// Act
		const result = subscribeSpyTo(accountDialogStore.saveAccount(), { expectErrors: true });
		await result.onError();

		// Assert
		expect(result.receivedError()).toEqual(true);
		expect(accountDialogStore.savingLoading).toEqual(false);
		expect(closeDialogSpy).not.toHaveBeenCalledWith('account-dialog');
	});
});
