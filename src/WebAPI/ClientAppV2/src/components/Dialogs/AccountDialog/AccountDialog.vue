<template>
	<q-card-dialog max-width="900px" :name="name" persistent cy="account-dialog-form" @opened="openDialog" @closed="closeDialog">
		<!-- Dialog Header -->
		<template #title>
			{{ getDisplayName }}
		</template>
		<template #default>
			<AccountForm ref="accountForm" :value="changedPlexAccount" @input="formChanged" @is-valid="isValid = $event" />
		</template>
		<!-- Dialog Actions	-->
		<template #actions="{ close }">
			<q-row justify="between" gutter="md">
				<!-- Delete account -->
				<q-col v-if="!isNewAccount">
					<DeleteButton class="mx-2" block :width="130" @click="openConfirmationDialog" />
				</q-col>
				<!-- Cancel button -->
				<q-col>
					<CancelButton :width="130" class="mx-2" block cy="account-dialog-cancel-button" @click="close" />
				</q-col>
				<!-- Reset Form -->
				<q-col>
					<ResetButton :width="130" class="mx-2" block cy="account-dialog-reset-button" @click="reset" />
				</q-col>
				<!-- Validation button -->
				<q-col>
					<AccountValidationButton
						:color="validationColor"
						:disabled="!isValid || validateLoading"
						:icon="validationIcon"
						:loading="validateLoading"
						:width="130"
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
						:width="130"
						block
						class="q-mx-md"
						@click="saveAccount(close)" />
				</q-col>
			</q-row>
		</template>
	</q-card-dialog>

	<!--	Account Verification Code Dialog	-->
	<AccountVerificationCodeDialog
		:dialog="verificationCodeDialogState"
		:errors="validateErrors"
		@close="closeVerificationDialog"
		@submit="validateAfterVerificationCode" />
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
import { ref, computed, defineProps } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { cloneDeep } from 'lodash-es';
import { merge } from 'rxjs';
import { IError, PlexAccountDTO } from '@dto/mainApi';
import { validateAccount } from '@api/accountApi';
import { AccountService, LibraryService, ServerService } from '@service';
import { useI18n, useOpenControlDialog } from '#imports';
import type { AccountForm } from '#components';

const { t } = useI18n();

defineProps<{ name: string }>();

const confirmationDialogName = 'confirmationAccountDialogName';
const isNewAccount = ref(false);

const accountForm = ref<InstanceType<typeof AccountForm> | null>(null);

/**
 * The plexAccount as it is currently saved
 */
const originalPlexAccount = ref<PlexAccountDTO | null>(null);

/**
 * The plexAccount as it is currently changed in this dialog
 */
const changedPlexAccount = ref<PlexAccountDTO>(getDefaultAccount());

const validateLoading = ref(false);
const isValidated = ref('');
const isValid = ref(true);

const saving = ref(false);
const validateErrors = ref<IError[]>([]);
const verificationCodeDialogState = ref(false);
const inputHasChanged = ref(false);

function getDefaultAccount(): PlexAccountDTO {
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
		isValidated: false,
		authenticationToken: '',
		email: '',
		plexServerAccess: [],
	};
}

const isAllowedToSave = computed(() => {
	return !saving.value && isValidated.value === 'OK' && isValid.value;
});

const hasCredentialsChanged = computed(() => {
	if (!isNewAccount.value) {
		return (
			originalPlexAccount.value?.username !== changedPlexAccount.value.username ||
			originalPlexAccount.value?.password !== changedPlexAccount.value.password
		);
	}
	return false;
});

const validationIcon = computed(() => {
	if (isValidated.value === 'OK') {
		return 'mdi-check-bold';
	} else if (isValidated.value === 'ERROR') {
		return 'mdi-alert-circle-outline';
	} else {
		return 'mdi-text-box-search-outline';
	}
});

const validationColor = computed(() => {
	switch (isValidated.value) {
		case 'OK':
			return 'positive';
		case 'ERROR':
			return 'negative';
		default:
			return 'default';
	}
});

const getDisplayName = computed(() => {
	const title = t(`components.account-dialog.${isNewAccount.value ? 'add-account-title' : 'edit-account-title'}`).toString();
	return changedPlexAccount.value?.displayName !== '' ? `${title}: ${changedPlexAccount.value?.displayName}` : title;
});

const formChanged = ({ prop, value }: { prop: string; value: string | boolean }) => {
	set(inputHasChanged, true);
	changedPlexAccount.value[prop] = value;
};

function openConfirmationDialog() {
	useOpenControlDialog(confirmationDialogName);
}

const validate = () => {
	set(validateLoading, true);

	useSubscription(
		validateAccount(changedPlexAccount.value).subscribe((data) => {
			// Account has no 2FA and was valid
			if (data.isSuccess && data.value) {
				set(changedPlexAccount, data.value);

				set(isValidated, 'OK');
				set(isValid, true);
				set(validateErrors, []);
			} else if (data.isSuccess && !data.value) {
				// Account has no 2FA and was invalid
				set(isValidated, 'ERROR');
				set(isValid, false);
				set(validateErrors, []);
			} else if (!data.isSuccess && data.value) {
				// Account has 2FA
				set(isValidated, 'ERROR');
				set(isValid, false);
				set(validateErrors, data.errors ?? []);
				set(verificationCodeDialogState, true);
			} else {
				// Account has 2FA and was invalid
				set(isValidated, 'ERROR');
				set(isValid, false);
				set(validateErrors, []);
			}
			set(validateLoading, false);
		}),
	);
};

function closeVerificationDialog() {
	set(verificationCodeDialogState, false);
	set(validateLoading, false);
}

const validateAfterVerificationCode = (verificationCode: string) => {
	changedPlexAccount.value.verificationCode = verificationCode;
	useSubscription(
		validateAccount(changedPlexAccount.value).subscribe((data) => {
			if (data && data.isSuccess && data.value) {
				// Take over the authToken
				set(changedPlexAccount, data.value);

				set(validateLoading, false);
				set(verificationCodeDialogState, false);
				set(isValidated, 'OK');
			} else {
				set(validateErrors, data.errors ?? []);
				set(isValidated, 'ERROR');
				Log.error('Validate Error', data);
			}
		}),
	);
};

// region Button Commands

const reset = () => {
	set(changedPlexAccount, getDefaultAccount());
	accountForm.value?.onReset();
};

function saveAccount(close: any) {
	set(saving, true);

	if (get(isNewAccount)) {
		useSubscription(
			AccountService.createPlexAccount(changedPlexAccount.value).subscribe((account) => {
				if (account) {
					set(changedPlexAccount, account);
					close();
				} else {
					Log.error('Result was invalid when saving a created account', account);
					set(saving, false);
				}
			}),
		);
		return;
	}
	useSubscription(
		AccountService.updatePlexAccount(changedPlexAccount.value, hasCredentialsChanged.value).subscribe((data) => {
			if (data) {
				set(changedPlexAccount, data);
				if (!hasCredentialsChanged.value) {
					close();
					refreshAccounts();
				}
			} else {
				Log.error('Result was invalid when saving an updated account', data);
				set(saving, false);
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
		set(changedPlexAccount, cloneDeep(account));
		set(isValidated, get(changedPlexAccount).isValidated ? 'OK' : 'ERROR');
	}
}

function closeDialog() {
	set(saving, false);
	set(verificationCodeDialogState, false);
	set(inputHasChanged, false);
	set(isValidated, '');
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
