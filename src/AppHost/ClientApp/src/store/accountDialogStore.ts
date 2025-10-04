import Log from 'consola';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { get } from '@vueuse/core';
import { tap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { DialogType } from '@enums';
import { plexAccountApi } from '@api';
import type { IError, PlexAccountDTO } from '@dto';
import type { IAccountDialog } from '@interfaces';
import { useAccountStore, useDialogStore } from '@store';
import { cloneDeep } from 'lodash-es';

interface IAccountDialogStore extends PlexAccountDTO {
	isAuthTokenMode: boolean;
	isNewAccount: boolean;
	isInputValid: boolean;
	showPassword: boolean;
	showAuthToken: boolean;
	deleteLoading: boolean;
	validateLoading: boolean;
	savingLoading: boolean;
	hasValidationErrors: boolean;
	validationErrors: IError[];
}

export const useAccountDialogStore = defineStore('AccountDialogStore', () => {
	const defaultState: IAccountDialogStore = {
		id: 0,
		isEnabled: true,
		isMain: true,
		username: '',
		password: '',
		displayName: '',
		clientId: '',
		verificationCode: '',
		uuid: '',
		hasPassword: false,
		validatedAt: '0001-01-01T00:00:00Z',
		is2Fa: false,
		title: '',
		plexId: 0,
		authenticationToken: '',
		apiAuthenticationToken: '',
		email: '',
		plexServerAccess: [],
		plexLibraryAccess: [],
		// Dialog properties
		isAuthTokenMode: false,
		isNewAccount: false,
		isValidated: false,
		showAuthToken: false,
		showPassword: false,
		hasValidationErrors: false,
		deleteLoading: false,
		validateLoading: false,
		savingLoading: false,
		isInputValid: true,
		validationErrors: [],
	};

	const state = reactive<IAccountDialogStore>(cloneDeep(defaultState));
	const dialogStore = useDialogStore();
	const accountStore = useAccountStore();

	// Helper function to update state with validated account data
	const updateStateWithAccountData = (accountData: {
		clientId: string;
		username: string;
		email: string;
		title: string;
		plexId: number;
		uuid: string;
		authenticationToken: string;
		isValidated: boolean;
		validatedAt?: string | null;
		is2Fa: boolean;
	}) => {
		Object.assign(state, {
			clientId: accountData.clientId,
			username: accountData.username,
			email: accountData.email,
			title: accountData.title,
			plexId: accountData.plexId,
			uuid: accountData.uuid,
			authenticationToken: accountData.authenticationToken,
			isValidated: accountData.isValidated,
			validatedAt: accountData.validatedAt,
			is2Fa: accountData.is2Fa,
		});
	};

	const actions = {
		openDialog({ accountId }: IAccountDialog): void {
			state.isNewAccount = accountId === 0;
			if (!state.isNewAccount) {
				const account = accountStore.getAccount(accountId);
				if (account) {
					Object.assign(state, account);
				}
			}
		},
		closeDialog(): void {
			dialogStore.closeDialog(DialogType.AccountVerificationCodeDialog);
			dialogStore.closeDialog(DialogType.AccountConfirmationDialog);
			dialogStore.closeDialog(DialogType.AccountDialog);
			actions.$reset();
		},
		validatePlexToken() {
			state.validateLoading = true;

			return plexAccountApi.validatePlexTokenEndpoint({
				displayName: state.displayName,
				manualAuthenticationToken: state.authenticationToken,
			}).pipe(
				tap(({ value, isSuccess, errors }) => {
					// Always reset loading state
					state.validateLoading = false;

					if (!isSuccess || !value) {
						Log.error('Token validation failed', errors);
						return;
					}

					if (!isSuccess || value?.isUnAuthorized) {
						state.isValidated = false;
						state.hasValidationErrors = true;
						dialogStore.openDialog(DialogType.AccountTokenValidateDialog);
						return;
					}

					if (value.isValidated) {
						Log.info('Account is validated and was added by token');
						dialogStore.openDialog(DialogType.AccountTokenValidateDialog);
						return;
					}
					state.hasValidationErrors = false;

					if (!value) {
						state.isValidated = false;
						state.hasValidationErrors = true;
						state.validationErrors = [];
						return;
					}

					// Update state with validated token data
					updateStateWithAccountData(value);

					// Account was validated successfully
					if (value.isValidated) {
						Log.info('Token validation successful');
						return;
					}
				}),
				catchError((error) => {
					// Reset loading state on error
					state.validateLoading = false;
					state.isValidated = false;
					state.hasValidationErrors = true;
					Log.error('Token validation failed', error);
					return of({ value: null, isSuccess: false });
				}),
			);
		},
		validatePlexAccount() {
			state.validateLoading = true;

			return plexAccountApi.validatePlexCredentialsEndpoint(get(getters.getAccountData)).pipe(
				tap(({ value, isSuccess }) => {
					// Always reset loading state
					state.validateLoading = false;

					if (!isSuccess || !value) {
						state.isValidated = false;
						state.hasValidationErrors = true;
						dialogStore.openDialog(DialogType.AccountTokenValidateDialog);
						return;
					}

					// Update state with validated credentials data
					updateStateWithAccountData(value);

					// Account has no 2FA and was valid
					if (value.isValidated && !value.is2Fa) {
						Log.info('Account has no 2FA and was valid');
						return;
					}

					// Account has no 2FA and was invalid
					if (!value.isValidated && !value.is2Fa) {
						Log.info('Account has no 2FA and was invalid');
						return;
					}

					// Account has 2FA
					if (!value.isValidated && value.is2Fa) {
						Log.info('Account has 2FA enabled');
						dialogStore.openDialog(DialogType.AccountVerificationCodeDialog);
						return;
					}

					if (!value.isValidated && value.is2Fa) {
						Log.info('Account was valid and has 2FA enabled, this makes no sense and sounds like a bug');
					}
				}),
				catchError((error) => {
					// Reset loading state on error
					state.validateLoading = false;
					state.isValidated = false;
					state.hasValidationErrors = true;
					Log.error('Credentials validation failed', error);
					return of({ value: null, isSuccess: false });
				}),
			);
		},
		validateVerificationCode() {
			return plexAccountApi.validatePlexCredentialsEndpoint(get(getters.getAccountData)).pipe(
				tap(({ value, isSuccess }) => {
					if (isSuccess && value) {
						dialogStore.closeDialog(DialogType.AccountVerificationCodeDialog);
						// Update state with validated credentials data
						updateStateWithAccountData(value);
					} else {
						Log.error('Validate Error', value);
					}
				}),
			);
		},
		generateToken(verificationCode: string = '') {
			return plexAccountApi.generatePlexTokenEndpoint(state.id, { verificationCode });
		},
		saveAccount() {
			state.savingLoading = true;
			if (state.isNewAccount) {
				return accountStore.createPlexAccount(get(getters.getAccountData)).pipe(
					tap(() => {
						state.savingLoading = false;
						dialogStore.closeDialog(DialogType.AccountDialog);
					}),
				);
			}
			return accountStore.updatePlexAccount(get(getters.getAccountData)).pipe(
				tap(() => {
					state.savingLoading = false;
					dialogStore.closeDialog(DialogType.AccountDialog);
				}),
			);
		},
		switchInputMode(isAuthTokenMode: boolean) {
			state.isAuthTokenMode = isAuthTokenMode;
			// Clear input fields
			state.username = '';
			state.password = '';
			state.authenticationToken = '';
			state.isValidated = false;
		},
		deleteAccount() {
			state.deleteLoading = true;
			return accountStore.deleteAccount(state.id).pipe(tap(() => dialogStore.closeDialog(DialogType.AccountDialog)));
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};
	const getters = {
		hasCredentialsChanged: computed(() => {
			if (!state.isNewAccount) {
				const originalPlexAccount = accountStore.getAccount(state.id);
				if (!originalPlexAccount) {
					return false;
				}

				return originalPlexAccount.username !== state.username || originalPlexAccount.password !== state.password;
			}
			return false;
		}),
		getAccountData: computed((): PlexAccountDTO => {
			return {
				id: state.id,
				isValidated: state.isValidated,
				password: state.password,
				username: state.username,
				uuid: state.uuid,
				validatedAt: state.validatedAt,
				verificationCode: state.verificationCode,
				apiAuthenticationToken: state.apiAuthenticationToken,
				authenticationToken: state.authenticationToken,
				clientId: state.clientId,
				displayName: state.displayName,
				email: state.email,
				hasPassword: state.hasPassword,
				is2Fa: state.is2Fa,
				isEnabled: state.isEnabled,
				isMain: state.isMain,
				plexId: state.plexId,
				plexLibraryAccess: state.plexLibraryAccess,
				plexServerAccess: state.plexServerAccess,
				title: state.title,
			};
		}),
	};
	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useAccountDialogStore, import.meta.hot));
}
