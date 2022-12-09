<template>
	<v-dialog id="account-dialog" :max-width="900" :value="showDialog" persistent>
		<!-- The account pop-up -->
		<v-card data-cy="account-dialog-form">
			<v-card-title class="headline">
				{{ getDisplayName }}
			</v-card-title>
			<v-divider></v-divider>
			<v-card-text class="mt-2">
				<account-form ref="accountForm" :value="changedPlexAccount" @input="formChanged" @isValid="isValid = $event" />
			</v-card-text>

			<!-- Dialog Actions	-->
			<v-card-actions>
				<!-- Delete account -->
				<DeleteButton v-if="!isNewAccount" class="mx-2" :width="130" @click="confirmationDialogState = true" />

				<!-- Reset Form -->
				<ResetButton :width="130" class="mx-2" cy="account-dialog-reset-button" @click="reset" />
				<v-spacer />

				<!-- Cancel button -->
				<CancelButton :width="130" class="mx-2" cy="account-dialog-cancel-button" @click="cancel" />

				<!-- Validation button -->
				<AccountValidationButton
					:color="validationColor"
					:disabled="!isValid || validateLoading"
					:icon="validationIcon"
					:loading="validateLoading"
					:text-id="!isValid ? 'validate' : ''"
					:width="130"
					cy="account-dialog-validate-button"
					class="mx-2"
					@click="validate"
				/>

				<!-- Save account -->
				<SaveButton
					:disabled="!isAllowedToSave"
					:text-id="isNewAccount ? 'save' : 'update'"
					:cy="`account-dialog-${isNewAccount ? 'save' : 'update'}-button`"
					:width="130"
					class="mx-2"
					@click="saveAccount"
				/>
			</v-card-actions>
		</v-card>

		<!--	Account Verification Code Dialog	-->
		<account-verification-code-dialog
			:dialog="verificationCodeDialogState"
			:errors="validateErrors"
			@close="closeVerificationDialog"
			@submit="validateAfterVerificationCode"
		/>
		<!--	Delete Confirmation Dialog	-->
		<confirmation-dialog
			:confirm-loading="true"
			:dialog="confirmationDialogState"
			class="mr-4"
			text-id="delete-account"
			@cancel="confirmationDialogState = false"
			@confirm="deleteAccount"
		/>
	</v-dialog>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Ref, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { Error, PlexAccountDTO } from '@dto/mainApi';
import { validateAccount } from '@api/accountApi';
import { AccountService } from '@service';
import AccountVerificationCodeDialog from '@overviews/AccountOverview/AccountVerificationCodeDialog.vue';
import AccountForm from '@overviews/AccountOverview/AccountForm.vue';

@Component({
	components: { AccountForm, AccountVerificationCodeDialog },
})
export default class AccountDialog extends Vue {
	private showDialog: boolean = false;

	@Ref('accountForm')
	readonly accountForm!: AccountForm;

	private isNewAccount: boolean = false;
	/**
	 * The plexAccount as it is currently saved
	 */
	private originalPlexAccount!: PlexAccountDTO | null;
	/**
	 * The plexAccount as it is currently changed in this dialog
	 */
	private changedPlexAccount: PlexAccountDTO = this.getDefaultAccount;

	private validateLoading: boolean = false;
	private isValidated: string = '';
	private isValid: boolean = true;

	private saving: boolean = false;

	private validateErrors: Error[] = [];

	private confirmationDialogState: Boolean = false;
	private verificationCodeDialogState: Boolean = false;
	private inputHasChanged: Boolean = false;

	get getDefaultAccount(): PlexAccountDTO {
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

	get isAllowedToSave(): boolean {
		return !this.saving && this.isValidated === 'OK' && this.isValid;
	}

	get hasCredentialsChanged(): boolean {
		if (!this.isNewAccount) {
			return (
				this.originalPlexAccount?.username !== this.changedPlexAccount.username ||
				this.originalPlexAccount?.password !== this.changedPlexAccount.password
			);
		}
		return false;
	}

	get validationIcon(): string {
		if (this.isValidated === 'OK') {
			return 'mdi-check-bold';
		} else if (this.isValidated === 'ERROR') {
			return 'mdi-alert-circle-outline';
		} else {
			return 'mdi-text-box-search-outline';
		}
	}

	get validationColor(): string {
		switch (this.isValidated) {
			case 'ERROR':
				return 'red';
			case 'OK':
				return 'green';
			default:
				return 'white';
		}
	}

	get getDisplayName(): string {
		const title = this.$t(
			`components.account-dialog.${this.isNewAccount ? 'add-account-title' : 'edit-account-title'}`,
		).toString();
		return this.changedPlexAccount?.displayName !== '' ? `${title}: ${this.changedPlexAccount?.displayName}` : title;
	}

	formChanged({ prop, value }: { prop: string; value: string | boolean }) {
		this.inputHasChanged = true;
		this.changedPlexAccount[prop] = value;
	}

	validate(): void {
		this.validateLoading = true;

		useSubscription(
			validateAccount(this.changedPlexAccount).subscribe((data) => {
				// Account has no 2FA and was valid
				if (data.isSuccess && data.value) {
					this.changedPlexAccount = data.value;

					Log.info('PlexAccount', this.changedPlexAccount);
					if (this.changedPlexAccount.is2Fa) {
						this.verificationCodeDialogState = true;
						return;
					}
					this.isValidated = 'OK';
				} else {
					Log.error('Validating account failed:', data);
				}
				this.validateLoading = false;
			}),
		);
	}

	closeVerificationDialog() {
		this.verificationCodeDialogState = false;
		this.validateLoading = false;
	}

	validateAfterVerificationCode(verificationCode: string) {
		this.changedPlexAccount.verificationCode = verificationCode;
		useSubscription(
			validateAccount(this.changedPlexAccount).subscribe((data) => {
				if (data && data.isSuccess && data.value) {
					// Take over the authToken
					this.changedPlexAccount = data.value;

					this.validateLoading = false;
					this.verificationCodeDialogState = false;
					this.isValidated = 'OK';
				} else {
					this.validateErrors = data.errors ?? [];
					this.isValidated = 'ERROR';
					Log.error('Validate Error', data);
				}
			}),
		);
	}

	// region Button Commands

	reset(): void {
		this.changedPlexAccount = this.getDefaultAccount;
		this.accountForm?.reset();
	}

	cancel(): void {
		this.closeDialog();
	}

	saveAccount(): void {
		this.saving = true;

		if (this.isNewAccount) {
			useSubscription(
				AccountService.createPlexAccount(this.changedPlexAccount).subscribe((account) => {
					if (account) {
						this.changedPlexAccount = account;
						this.closeDialog();
					} else {
						Log.error('Result was invalid when saving a created account', account);
						this.saving = false;
					}
				}),
			);
		} else {
			useSubscription(
				AccountService.updatePlexAccount(this.changedPlexAccount, this.hasCredentialsChanged).subscribe((data) => {
					if (data) {
						this.changedPlexAccount = data;
						if (!this.hasCredentialsChanged) {
							this.closeDialog(true);
						}
					} else {
						Log.error('Result was invalid when saving an updated account', data);
						this.saving = false;
					}
				}),
			);
		}
	}

	deleteAccount(): void {
		AccountService.deleteAccount(this.changedPlexAccount.id).subscribe(() => {
			this.closeDialog(true);
		});
	}

	// endregion

	openDialog(newAccount: boolean, account: PlexAccountDTO | null = null): void {
		this.isNewAccount = newAccount;
		// Setup values
		if (account) {
			this.changedPlexAccount = { ...account };
			this.isValidated = this.changedPlexAccount.isValidated ? 'OK' : 'ERROR';
		}

		this.showDialog = true;
	}

	closeDialog(refreshAccounts: boolean = false): void {
		this.showDialog = false;
		this.confirmationDialogState = false;
		this.saving = false;
		this.verificationCodeDialogState = false;
		this.inputHasChanged = false;
		this.isValidated = '';
		this.reset();
		this.$emit('dialog-closed', refreshAccounts);
	}
}
</script>
