import { describe, beforeAll, beforeEach, test, expect, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { of } from 'rxjs';
import { baseSetup, baseVars, getAxiosMock, subscribeSpyTo } from '@services-test-base';
import { generateResultDTO } from '@mock';
import type { PlexAccountDTO } from '@dto';
import type { ResultDTO } from '@interfaces';
import { useAccountDialogStore, useAccountStore, useDialogStore } from '@store';

describe('AccountDialogStore.deleteAccount()', () => {
	let { mock: _mock } = baseVars();

	beforeAll(() => {
		baseSetup();
	});

	beforeEach(() => {
		_mock = getAxiosMock();
		setActivePinia(createPinia());
	});

	test('Should stop delete loading after delete request completes', async () => {
		// Arrange
		const accountStore = useAccountStore();
		const dialogStore = useDialogStore();
		const deleteSpy = vi
			.spyOn(accountStore, 'deleteAccount')
			.mockReturnValue(of(generateResultDTO([] as PlexAccountDTO[])));
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');

		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.id = 9;

		// Act
		const result = subscribeSpyTo(accountDialogStore.deleteAccount());
		await result.onComplete();

		// Assert
		expect(deleteSpy).toHaveBeenCalledWith(9);
		expect(closeDialogSpy).toHaveBeenCalledWith('account-dialog');
		expect(accountDialogStore.deleteLoading).toEqual(false);
	});

	test('Should not close dialog when delete request fails', async () => {
		// Arrange
		const accountStore = useAccountStore();
		const dialogStore = useDialogStore();
		const failedDeleteResult: ResultDTO<PlexAccountDTO[]> = {
			isSuccess: false,
			value: [] as PlexAccountDTO[],
			statusCode: 500,
			errors: [],
			successes: [],
		};
		vi.spyOn(accountStore, 'deleteAccount').mockReturnValue(of(failedDeleteResult));
		const closeDialogSpy = vi.spyOn(dialogStore, 'closeDialog');

		const accountDialogStore = useAccountDialogStore();
		accountDialogStore.id = 10;

		// Act
		const result = subscribeSpyTo(accountDialogStore.deleteAccount());
		await result.onComplete();

		// Assert
		expect(closeDialogSpy).not.toHaveBeenCalledWith('account-dialog');
		expect(accountDialogStore.deleteLoading).toEqual(false);
	});
});
