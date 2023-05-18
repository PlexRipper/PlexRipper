<template>
	<QCardDialog max-width="900px" :name="name" persistent cy="account-dialog-form" @opened="openDialog" @closed="closeDialog">
		<!-- Dialog Header -->
		<template #title>
			{{ getDisplayName }}
		</template>
		<template #default>
			<AccountForm ref="accountForm" :value="changedPlexAccount" @input="formChanged" @is-valid="isInputValid" />
			<Print>{{ changedPlexAccount }}</Print>
		</template>
		<!-- Dialog Actions	-->
		<template #actions="{ close }">
			<q-row justify="between" gutter="md">
				<!-- Delete account -->
				<q-col v-if="!isNewAccount">
					<DeleteButton class="mx-2" block cy="account-dialog-delete-button" @click="openConfirmationDialog" />
				</q-col>
				<!-- Cancel button -->
				<q-col>
					<CancelButton class="mx-2" block cy="account-dialog-cancel-button" @click="close" />
				</q-col>
				<!-- Reset Form -->
				<q-col>
					<ResetButton class="mx-2" block cy="account-dialog-reset-button" @click="reset" />
				</q-col>
				<!-- Validation button -->
				<q-col>
					<AccountValidationButton
						:color="validationStyle.color"
						:disabled="validateLoading"
						:icon="validationStyle.icon"
						:text-id="validationStyle.text"
						:loading="validateLoading"
						block
						cy="account-dialog-validate-button"
						class="q-mx-md"
						@click="validate" />
				</q-col>
				<!-- Save account -->
				<q-col>
					<SaveButton
						:disabled="!isAllowedToSave"
						:text-id="isNewAccount ? 'save' : 'update'"
						:cy="`account-dialog-${isNewAccount ? 'save' : 'update'}-button`"
						block
						:loading="savingLoading"
						class="q-mx-md"
						@click="saveAccount(close)" />
				</q-col>
			</q-row>
		</template>
	</QCardDialog>

	<!--	Account Verification Code Dialog	-->
	<AccountVerificationCodeDialog
		:name="verificationCodeDialogName"
		:account="changedPlexAccount"
		@close="closeVerificationDialog"
		@confirm="validateAfterVerificationCode" />
	<!--	Delete Confirmation Dialog	-->
	<confirmation-dialog
		:confirm-loading="true"
		:name="confirmationDialogName"
		class="q-mr-md"
		text-id="delete-account"
		@confirm="deleteAccount" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { cloneDeep } from 'lodash-es';
import { merge } from 'rxjs';
import { take } from 'rxjs/operators';
import { IError, PlexAccountDTO } from '@dto/mainApi';
import { validateAccount } from '@api/accountApi';
import { AccountService, LibraryService, ServerService } from '@service';
import { useI18n, useOpenControlDialog, useCloseControlDialog } from '#imports';
import type { AccountForm } from '#components';

const { t } = useI18n();

interface IPlexAccount extends PlexAccountDTO {
	isInputValid: boolean;
	hasValidationErrors: boolean;
	validationErrors: IError[];
}

const props = defineProps<{ name: string }>();

const isNewAccount = ref(false);

const accountForm = ref<InstanceType<typeof AccountForm> | null>(null);
const confirmationDialogName = 'confirmationAccountDialogName';
const verificationCodeDialogName = 'verificationCodeDialogName';
/**
 * The plexAccount as it is currently saved
 */
const originalPlexAccount = ref<PlexAccountDTO | null>(null);

/**
 * The plexAccount as it is currently changed in this dialog
 */
const changedPlexAccount = ref<IPlexAccount>(getDefaultAccount());

const validateLoading = ref(false);
const savingLoading = ref(false);

function getDefaultAccount(): IPlexAccount {
	return {
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
		email: '',
		plexServerAccess: [],
		// Dialog properties

		isValidated: false,
		hasValidationErrors: false,
		isInputValid: true,
		validationErrors: [],
	};
}

const isAllowedToSave = computed(() => {
	return !savingLoading.value && get(changedPlexAccount).isValidated;
});

const hasCredentialsChanged = computed(() => {
	if (!isNewAccount.value) {
		return (
			originalPlexAccount.value?.username !== get(changedPlexAccount).username ||
			originalPlexAccount.value?.password !== get(changedPlexAccount).password
		);
	}
	return false;
});

const validationStyle = computed((): { color: 'default' | 'positive' | 'warning' | 'negative'; icon: string; text: string } => {
	if (get(changedPlexAccount).hasValidationErrors) {
		return {
			color: 'negative',
			icon: 'mdi-alert-circle-outline',
			text: 'validate',
		};
	}
	if (get(changedPlexAccount).isValidated && !get(changedPlexAccount).hasValidationErrors) {
		return {
			color: 'positive',
			icon: 'mdi-check-bold',
			text: '',
		};
	}
	return {
		color: 'default',
		icon: 'mdi-text-box-search-outline',
		text: 'validate',
	};
});

const getDisplayName = computed(() => {
	const title = t(`components.account-dialog.${isNewAccount.value ? 'add-account-title' : 'edit-account-title'}`).toString();
	return get(changedPlexAccount)?.displayName !== '' ? `${title}: ${get(changedPlexAccount)?.displayName}` : title;
});

function isInputValid(value: boolean) {
	get(changedPlexAccount).isInputValid = value;
}

function formChanged({ prop, value }: { prop: string; value: string | number | null }) {
	get(changedPlexAccount)[prop] = value;
}

function openConfirmationDialog() {
	useOpenControlDialog(confirmationDialogName);
}

function validate() {
	set(validateLoading, true);

	useSubscription(
		validateAccount(get(changedPlexAccount))
			.pipe(take(1))
			.subscribe({
				next: (data) => {
					const account = data.isSuccess ? data?.value : null;
					// Account has no 2FA and was valid
					if (account?.isValidated && !account?.is2Fa) {
						Log.info('Account has no 2FA and was valid');
						set(changedPlexAccount, { ...get(changedPlexAccount), ...account });

						return;
					}

					// Account has no 2FA and was invalid
					if (!account?.isValidated && !account?.is2Fa) {
						Log.info('Account has no 2FA and was invalid');
						set(changedPlexAccount, { ...get(changedPlexAccount), ...account });
						return;
					}

					// Account has 2FA
					if (account?.isValidated && account?.is2Fa) {
						Log.info('Account has 2FA enabled');
						set(changedPlexAccount, { ...get(changedPlexAccount), ...account });
						useOpenControlDialog(verificationCodeDialogName);
						return;
					}

					if (!account?.isValidated && account?.is2Fa) {
						// Account has 2FA and was invalid
						Log.info('Account has 2FA and was invalid');
						set(changedPlexAccount, { ...get(changedPlexAccount), ...account });
					}
				},
				complete: () => {
					set(validateLoading, false);
				},
			}),
	);
}

function closeVerificationDialog() {
	useCloseControlDialog(verificationCodeDialogName);
	set(validateLoading, false);
}

function validateAfterVerificationCode(account: PlexAccountDTO) {
	if (account) {
		set(changedPlexAccount, { ...get(changedPlexAccount), ...account });
	}
}

// region Button Commands

const reset = () => {
	set(changedPlexAccount, getDefaultAccount());
	accountForm.value?.onReset();
};

function saveAccount(close: any) {
	set(savingLoading, true);

	if (get(isNewAccount)) {
		useSubscription(
			AccountService.createPlexAccount(get(changedPlexAccount)).subscribe((account) => {
				if (account) {
					set(changedPlexAccount, {
						...get(changedPlexAccount),
						...account,
					});
					close();
				} else {
					Log.error('Result was invalid when saving a created account', account);
					set(savingLoading, false);
				}
			}),
		);
		return;
	}
	useSubscription(
		AccountService.updatePlexAccount(get(changedPlexAccount), get(hasCredentialsChanged)).subscribe((account) => {
			if (account) {
				set(changedPlexAccount, {
					...get(changedPlexAccount),
					...account,
				});
				if (!hasCredentialsChanged.value) {
					close();
					refreshAccounts();
				}
			} else {
				Log.error('Result was invalid when saving an updated account', account);
				set(savingLoading, false);
			}
		}),
	);
}

function deleteAccount() {
	AccountService.deleteAccount(get(changedPlexAccount).id).subscribe(() => {
		closeDialog();
		refreshAccounts();
	});
}

function openDialog({ isNewAccountValue, account = null }: { isNewAccountValue: boolean; account: PlexAccountDTO | null }) {
	set(isNewAccount, isNewAccountValue);
	// Setup values
	if (account) {
		set(changedPlexAccount, { ...get(changedPlexAccount), ...cloneDeep(account) });
	}
}

function closeDialog() {
	set(savingLoading, false);
	closeVerificationDialog();
	useCloseControlDialog(confirmationDialogName);
	useCloseControlDialog(props.name);

	reset();
}

function refreshAccounts(): void {
	useSubscription(
		merge([
			AccountService.refreshAccounts(),
			ServerService.refreshPlexServers(),
			LibraryService.refreshLibraries(),
		]).subscribe(),
	);
}

// endregion
</script>
