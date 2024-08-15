<template>
	<QCardDialog
		max-width="900px"
		:name="name"
		persistent
		cy="account-dialog-form"
		@opened="openDialog"
		@closed="closeDialog">
		<!-- Dialog Header -->
		<template #title>
			<QRow>
				<QCol>
					<QText
						size="h5"
						bold="bold">
						{{ getDisplayName }}
					</QText>
				</QCol>
				<!-- Auth Token Mode -->
				<QCol cols="auto">
					<IconButton
						icon="mdi-cloud-key-outline"
						cy="account-dialog-auth-token-mode-button"
						@click="changedPlexAccount.isAuthTokenMode = !changedPlexAccount.isAuthTokenMode" />
				</QCol>
			</QRow>
		</template>
		<template #default>
			<AccountForm
				ref="accountForm"
				:value="changedPlexAccount"
				@input="formChanged"
				@is-valid="isInputValid" />
			<Print>{{ changedPlexAccount }}</Print>
		</template>
		<!-- Dialog Actions	-->
		<template #actions="{ close }">
			<QRow
				justify="between"
				gutter="md">
				<!-- Delete account -->
				<QCol v-if="!isNewAccount">
					<DeleteButton
						class="mx-2"
						block
						cy="account-dialog-delete-button"
						@click="openConfirmationDialog" />
				</QCol>
				<!-- Cancel button -->
				<QCol>
					<CancelButton
						class="mx-2"
						block
						cy="account-dialog-cancel-button"
						@click="close" />
				</QCol>
				<!-- Reset Form -->
				<QCol>
					<ResetButton
						class="mx-2"
						block
						cy="account-dialog-reset-button"
						@click="reset" />
				</QCol>
				<!-- Validation button -->
				<QCol>
					<AccountValidationButton
						:color="validationStyle.color"
						:disabled="validateLoading"
						:icon="validationStyle.icon"
						:label="validationStyle.text"
						:loading="validateLoading"
						block
						cy="account-dialog-validate-button"
						class="q-mx-md"
						@click="validate" />
				</QCol>
				<!-- Save account -->
				<QCol>
					<SaveButton
						:disabled="!isAllowedToSave"
						:label="isNewAccount ? $t('general.commands.save') : $t('general.commands.update')"
						:cy="`account-dialog-${isNewAccount ? 'save' : 'update'}-button`"
						block
						:loading="savingLoading"
						class="q-mx-md"
						@click="saveAccount(close)" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>

	<!--	Account Verification Code Dialog	-->
	<AccountVerificationCodeDialog
		:name="verificationCodeDialogName"
		:account="changedPlexAccount"
		@close="closeVerificationDialog"
		@confirm="validateAfterVerificationCode" />

	<AccountTokenValidateDialog
		:name="accountTokenValidateDialogName"
		:account="changedPlexAccount" />

	<!--	Delete Confirmation Dialog	-->
	<ConfirmationDialog
		class="q-mr-md"
		:confirm-loading="deleteLoading"
		:name="confirmationDialogName"
		:title="$t('confirmation.delete-account.title')"
		:text="$t('confirmation.delete-account.text')"
		:warning="$t('confirmation.delete-account.warning')"
		@confirm="deleteAccount" />
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { PlexAccountDTO } from '@dto';
import type { IPlexAccount } from '@interfaces';
import { plexAccountApi } from '@api';
import type AccountForm from '@components/Dialogs/AccountDialog/AccountForm.vue';

import type { AxiosResponse } from 'axios';
import { useI18n, useOpenControlDialog, useCloseControlDialog, useAccountStore } from '#imports';

const { t } = useI18n();
const accountStore = useAccountStore();

const props = defineProps<{ name: string }>();

const isNewAccount = ref(false);

const accountForm = ref<InstanceType<typeof AccountForm> | null>(null);
const confirmationDialogName = 'confirmationAccountDialogName';
const verificationCodeDialogName = 'verificationCodeDialogName';
const accountTokenValidateDialogName = 'accountTokenValidateDialogName';
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
const deleteLoading = ref(false);

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
		plexLibraryAccess: [],
		isAuthTokenMode: false,
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
			originalPlexAccount.value?.username !== get(changedPlexAccount).username
			|| originalPlexAccount.value?.password !== get(changedPlexAccount).password
		);
	}
	return false;
});

const validationStyle = computed((): { color: 'default' | 'positive' | 'warning' | 'negative'; icon: string; text: string } => {
	if (get(changedPlexAccount).hasValidationErrors) {
		return {
			color: 'negative',
			icon: 'mdi-alert-circle-outline',
			text: t('general.commands.validate'),
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
		text: t('general.commands.validate'),
	};
});

const getDisplayName = computed(() => {
	const displayName = get(changedPlexAccount).displayName;
	let title = '';
	if (get(isNewAccount)) {
		title = t('components.account-dialog.add-account-title', {
			name: get(changedPlexAccount).displayName,
		});
	} else {
		title = t('components.account-dialog.edit-account-title', {
			name: get(changedPlexAccount).displayName,
		});
	}
	// Remove the colon if the display name is empty
	return displayName ? title : title.replace(':', '');
});

function isInputValid(value: boolean) {
	get(changedPlexAccount).isInputValid = value;
}

function formChanged<K extends keyof IPlexAccount>({ prop, value }: { prop: K; value: IPlexAccount[K] }) {
	get(changedPlexAccount)[prop] = value;
}

function openConfirmationDialog() {
	useOpenControlDialog(confirmationDialogName);
}

function validate() {
	set(validateLoading, true);

	useSubscription(
		plexAccountApi
			.validatePlexAccountEndpoint(get(changedPlexAccount))
			.subscribe({
				next: (data) => {
					const account = data.isSuccess ? data?.value : null;

					get(changedPlexAccount).hasValidationErrors = false;

					if (account?.isValidated && account?.isAuthTokenMode) {
						Log.info('Account is validated and was added by token');
						set(changedPlexAccount, { ...get(changedPlexAccount), ...account });
						useOpenControlDialog(accountTokenValidateDialogName);
						return;
					}

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
					if (!account?.isValidated && account?.is2Fa) {
						Log.info('Account has 2FA enabled');
						set(changedPlexAccount, { ...get(changedPlexAccount), ...account });
						useOpenControlDialog(verificationCodeDialogName);
						return;
					}

					if (!account?.isValidated && account?.is2Fa) {
						Log.info('Account was valid and has 2FA enabled, this makes no sense and sounds like a bug');
					}
				},
				error(err: AxiosResponse) {
					Log.error('Error validating account', err);
					get(changedPlexAccount).hasValidationErrors = true;
					useOpenControlDialog(accountTokenValidateDialogName);

					set(validateLoading, false);
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

function saveAccount(close: () => void) {
	set(savingLoading, true);

	const accountData: PlexAccountDTO = omit(get(changedPlexAccount), ['isInputValid', 'hasValidationErrors', 'validationErrors']);

	if (get(isNewAccount)) {
		useSubscription(
			accountStore.createPlexAccount(accountData).subscribe(() => {
				set(savingLoading, false);
				close();
			}),
		);
	} else {
		useSubscription(
			accountStore.updatePlexAccount(accountData).subscribe((account) => {
				if (account) {
					set(changedPlexAccount, {
						...get(changedPlexAccount),
						...account,
					});
					if (!get(hasCredentialsChanged)) {
						close();
					}
				} else {
					Log.error('Result was invalid when saving an updated account', account);
					set(savingLoading, false);
				}
			}),
		);
	}

	close();
}

function deleteAccount() {
	set(deleteLoading, true);
	useSubscription(
		accountStore.deleteAccount(get(changedPlexAccount).id).subscribe(() => {
			closeDialog();
		}),
	);
}

function openDialog(event: unknown) {
	const { isNewAccountValue, account } = event as { isNewAccountValue: boolean; account: PlexAccountDTO | null };
	set(isNewAccount, isNewAccountValue);
	// Setup values
	if (account) {
		set(changedPlexAccount, { ...get(changedPlexAccount), ...cloneDeep(account) });
	}
}

function closeDialog() {
	set(savingLoading, false);
	set(deleteLoading, false);
	closeVerificationDialog();
	useCloseControlDialog(confirmationDialogName);
	useCloseControlDialog(props.name);

	reset();
}
// endregion
</script>
