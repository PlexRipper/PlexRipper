<template>
	<q-dialog id="account-dialog" :max-width="900" :model-value="showDialog" persistent>
		<!-- The account pop-up -->
		<q-card data-cy="account-dialog-form" class="account-dialog-content">
			<!-- Dialog Header -->
			<q-card-title>
				{{ getDisplayName }}
			</q-card-title>
			<q-separator />
			<q-card-section>
				<AccountForm ref="accountForm" :value="changedPlexAccount" @input="formChanged" @is-valid="isValid = $event" />
			</q-card-section>
			<!-- Dialog Actions	-->
			<q-card-actions>
				<!-- Delete account -->
				<DeleteButton v-if="!isNewAccount" class="mx-2" :width="130" @click="confirmationDialogState = true" />

				<!-- Reset Form -->
				<ResetButton :width="130" class="mx-2" cy="account-dialog-reset-button" @click="reset" />
				<q-space />

				<!-- Cancel button -->
				<CancelButton :width="130" class="mx-2" cy="account-dialog-cancel-button" @click="cancel" />

				<!-- Validation button -->
				<AccountValidationButton
					:color="validationColor"
					:disabled="!isValid || validateLoading"
					:icon="validationIcon"
					:loading="validateLoading"
					:width="130"
					cy="account-dialog-validate-button"
					class="mx-2"
					@click="validate" />

				<!-- Save account -->
				<SaveButton
					:disabled="!isAllowedToSave"
					:text-id="isNewAccount ? 'save' : 'update'"
					:cy="`account-dialog-${isNewAccount ? 'save' : 'update'}-button`"
					:width="130"
					class="mx-2"
					@click="saveAccount" />
			</q-card-actions>
		</q-card>

		<!--	Account Verification Code Dialog	-->
		<AccountVerificationCodeDialog
			:dialog="verificationCodeDialogState"
			:errors="validateErrors"
			@close="closeVerificationDialog"
			@submit="validateAfterVerificationCode" />
		<!--	Delete Confirmation Dialog	-->
		<confirmation-dialog
			:confirm-loading="true"
			:dialog="confirmationDialogState"
			class="mr-4"
			text-id="delete-account"
			@cancel="confirmationDialogState = false"
			@confirm="deleteAccount" />
	</q-dialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { defineEmits, ref, computed } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import AccountForm from '@overviews/AccountOverview/AccountForm.vue';
import { IError, PlexAccountDTO } from '@dto/mainApi';
import { validateAccount } from '@api/accountApi';
import { AccountService } from '@service';
import { useI18n } from '#imports';

const showDialog = ref(false);
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
const confirmationDialogState = ref(false);
const verificationCodeDialogState = ref(false);
const inputHasChanged = ref(false);

const emit = defineEmits<{ (e: 'dialog-closed', refreshAccounts: boolean): void }>();

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
			return 'success';
		case 'ERROR':
			return 'error';
		default:
			return 'primary';
	}
});

const getDisplayName = computed(() => {
	const title = useI18n()
		.t(`components.account-dialog.${isNewAccount.value ? 'add-account-title' : 'edit-account-title'}`)
		.toString();
	return changedPlexAccount.value?.displayName !== '' ? `${title}: ${changedPlexAccount.value?.displayName}` : title;
});

const formChanged = ({ prop, value }: { prop: string; value: string | boolean }) => {
	inputHasChanged.value = true;
	changedPlexAccount.value[prop] = value;
};

const validate = () => {
	validateLoading.value = true;

	useSubscription(
		validateAccount(changedPlexAccount.value).subscribe((data) => {
			// Account has no 2FA and was valid
			if (data.isSuccess && data.value) {
				changedPlexAccount.value = data.value;

				isValidated.value = 'OK';
				isValid.value = true;
				validateErrors.value = [];
			} else if (data.isSuccess && !data.value) {
				// Account has no 2FA and was invalid
				isValidated.value = 'ERROR';
				isValid.value = false;
				validateErrors.value = [];
			} else if (!data.isSuccess && data.value) {
				// Account has 2FA
				isValidated.value = 'ERROR';
				isValid.value = false;
				validateErrors.value = data.errors ?? [];
				verificationCodeDialogState.value = true;
			} else {
				// Account has 2FA and was invalid
				isValidated.value = 'ERROR';
				isValid.value = false;
				validateErrors.value = [];
			}
			validateLoading.value = false;
		}),
	);
};

function closeVerificationDialog() {
	verificationCodeDialogState.value = false;
	validateLoading.value = false;
}

const validateAfterVerificationCode = (verificationCode: string) => {
	changedPlexAccount.value.verificationCode = verificationCode;
	useSubscription(
		validateAccount(changedPlexAccount.value).subscribe((data) => {
			if (data && data.isSuccess && data.value) {
				// Take over the authToken
				changedPlexAccount.value = data.value;

				validateLoading.value = false;
				verificationCodeDialogState.value = false;
				isValidated.value = 'OK';
			} else {
				validateErrors.value = data.errors ?? [];
				isValidated.value = 'ERROR';
				Log.error('Validate Error', data);
			}
		}),
	);
};

// region Button Commands

const reset = () => {
	changedPlexAccount.value = getDefaultAccount();
	accountForm.value?.reset();
};

const cancel = () => {
	closeDialog();
};

const saveAccount = () => {
	saving.value = true;

	if (isNewAccount.value) {
		useSubscription(
			AccountService.createPlexAccount(changedPlexAccount.value).subscribe((account) => {
				if (account) {
					changedPlexAccount.value = account;
					closeDialog();
				} else {
					Log.error('Result was invalid when saving a created account', account);
					saving.value = false;
				}
			}),
		);
	} else {
		useSubscription(
			AccountService.updatePlexAccount(changedPlexAccount.value, hasCredentialsChanged.value).subscribe((data) => {
				if (data) {
					changedPlexAccount.value = data;
					if (!hasCredentialsChanged.value) {
						closeDialog(true);
					}
				} else {
					Log.error('Result was invalid when saving an updated account', data);
					saving.value = false;
				}
			}),
		);
	}
};

const deleteAccount = () => {
	AccountService.deleteAccount(changedPlexAccount.value.id).subscribe(() => {
		closeDialog(true);
	});
};

const openDialog = (newAccount: boolean, account: PlexAccountDTO | null = null) => {
	isNewAccount.value = newAccount;
	// Setup values
	if (account) {
		changedPlexAccount.value = { ...account };
		isValidated.value = changedPlexAccount.value.isValidated ? 'OK' : 'ERROR';
	}

	showDialog.value = true;
};

const closeDialog = (refreshAccounts = false) => {
	showDialog.value = false;
	confirmationDialogState.value = false;
	saving.value = false;
	verificationCodeDialogState.value = false;
	inputHasChanged.value = false;
	isValidated.value = '';
	reset();
	emit('dialog-closed', refreshAccounts);
};

// endregion

defineExpose({ openDialog, closeDialog });
</script>

<style lang="scss">
.account-dialog-content {
	max-width: 60vw !important;
	min-width: 60vw !important;
}
</style>
