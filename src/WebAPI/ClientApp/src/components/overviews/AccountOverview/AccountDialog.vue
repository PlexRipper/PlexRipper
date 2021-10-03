<template>
	<v-dialog :value="dialog" persistent :max-width="isSettingUpAccount ? 1000 : 900">
		<!-- The account pop-up -->
		<v-card v-show="!isSettingUpAccount">
			<v-card-title class="headline">
				{{ getDisplayName }}
			</v-card-title>
			<v-divider></v-divider>
			<v-card-text class="mt-2">
				<account-form ref="accountForm" :value="plexAccount" @input="formChanged" @isValid="isValid = $event" />
				{{ { ...plexAccount, plexServers: [] } }}
			</v-card-text>

			<!-- Dialog Actions	-->
			<v-card-actions>
				<!-- Delete account -->
				<p-btn
					v-if="!newAccount"
					:width="130"
					:button-type="getDeleteButtonType"
					text-id="delete"
					@click="confirmationDialogState = true"
				/>

				<!-- Reset Form -->
				<p-btn icon="mdi-restore" text-id="reset" :width="130" @click="reset" />
				<v-spacer />

				<!-- Cancel button -->
				<p-btn :width="130" :button-type="getCancelButtonType" @click="cancel" />

				<!-- Validation button -->
				<p-btn
					:icon="validationIcon"
					:loading="validateLoading"
					:disabled="!isValid || validateLoading"
					:color="isValid ? 'green' : 'red'"
					class="mr-4"
					:text-id="!isValid ? 'validate' : ''"
					:width="130"
					@click="validate"
				/>

				<!-- Save account -->
				<p-btn
					:width="130"
					:disabled="!isAllowedToSave"
					:text-id="newAccount ? 'create' : 'update'"
					:button-type="getSaveButtonType"
					@click="saveAccount"
				/>
			</v-card-actions>
		</v-card>
		<!--	The setup account progress -->
		<account-setup-progress v-show="isSettingUpAccount" :account="plexAccount" @hide="closeDialog(true)" />
		<!--	Account Verification Code Dialog	-->
		<account-verification-code-dialog
			:dialog="verificationCodeDialogState"
			:errors="validateErrors"
			@close="closeVerificationDialog"
			@submit="validateAfterVerificationCode"
		/>
		<!--	Delete Confirmation Dialog	-->
		<confirmation-dialog
			class="mr-4"
			text-id="delete-account"
			:dialog="confirmationDialogState"
			:confirm-loading="true"
			@cancel="confirmationDialogState = false"
			@confirm="deleteAccount"
		/>
	</v-dialog>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import type { Error, PlexAccountDTO } from '@dto/mainApi';
import { generateClientId, validateAccount } from '@api/accountApi';
import { AccountService } from '@service';
import { map } from 'rxjs/operators';
import AccountVerificationCodeDialog from '@overviews/AccountOverview/AccountVerificationCodeDialog.vue';
import AccountForm from '@overviews/AccountOverview/AccountForm.vue';
import ButtonType from '@/types/enums/buttonType';

@Component({
	components: { AccountForm, AccountVerificationCodeDialog },
})
export default class AccountDialog extends Vue {
	@Prop({ required: false, type: Object as () => PlexAccountDTO })
	readonly account!: PlexAccountDTO | null;

	@Prop({ required: true, type: Boolean, default: false })
	dialog: boolean = false;

	@Prop({ required: true, type: Boolean })
	readonly newAccount!: boolean;

	@Ref('accountForm')
	readonly accountForm!: AccountForm;

	plexAccount: PlexAccountDTO = this.getDefaultAccount;

	isSettingUpAccount: boolean = false;

	validateLoading: boolean = false;
	isValidated: string = '';
	isValid: boolean = true;

	saving: boolean = false;

	validateErrors: Error[] = [];

	confirmationDialogState: Boolean = false;
	verificationCodeDialogState: Boolean = false;
	inputHasChanged: Boolean = false;

	get isFormValid(): boolean {
		return this.isValid && this.isValidated === 'OK';
	}

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
			joinedAt: '0001-01-01T00:00:00Z',
			is2Fa: false,
			title: '',
			plexId: 0,
			isValidated: false,
			authenticationToken: '',
			email: '',
			plexServers: [],
		};
	}

	get isAllowedToSave(): boolean {
		return !this.saving && this.isValidated === 'OK' && this.isValid;
	}

	get getDeleteButtonType(): ButtonType {
		return ButtonType.Delete;
	}

	get getVerificationCodeButtonType(): ButtonType {
		return ButtonType.Info;
	}

	get getCancelButtonType(): ButtonType {
		return ButtonType.Cancel;
	}

	get getSaveButtonType(): ButtonType {
		return ButtonType.Save;
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

	get getDisplayName(): string {
		const title = this.$t(`components.account-dialog.${this.newAccount ? 'add-account-title' : 'edit-account-title'}`).toString();
		return this.plexAccount?.displayName !== '' ? `${title}: ${this.plexAccount?.displayName}` : title;
	}

	formChanged({ prop, value }: { prop: string; value: string | boolean }) {
		this.inputHasChanged = true;
		this.plexAccount[prop] = value;
	}

	validate(): void {
		this.validateLoading = true;
		this.$subscribeTo(validateAccount(this.plexAccount), (data) => {
			// Account has no 2FA and was valid
			if (data.isSuccess && data.value) {
				this.plexAccount = data.value;

				Log.info('PlexAccount', this.plexAccount);
				if (this.plexAccount.is2Fa) {
					this.verificationCodeDialogState = true;
				} else {
					this.validateErrors = data.errors ?? [];
					this.isValidated = 'ERROR';
					return;
				}
				this.isValidated = 'OK';
			} else {
				Log.error('Validating account failed:', data);
			}
		});
	}

	closeVerificationDialog() {
		this.verificationCodeDialogState = false;
		this.validateLoading = false;
	}

	validateAfterVerificationCode(verificationCode: string) {
		this.plexAccount.verificationCode = verificationCode;
		this.$subscribeTo(validateAccount(this.plexAccount), (data) => {
			if (data && data.isSuccess && data.value) {
				// Take over the authToken
				this.plexAccount = data.value;

				this.validateLoading = false;
				this.verificationCodeDialogState = false;
				this.isValidated = 'OK';
			} else {
				this.validateErrors = data.errors ?? [];
				this.isValidated = 'ERROR';
				Log.error('Validate Error', data);
			}
		});
	}

	// region Button Commands

	reset(): void {
		this.plexAccount = this.getDefaultAccount;
		this.accountForm?.reset();
	}

	cancel(): void {
		this.closeDialog();
	}

	saveAccount(): void {
		this.saving = true;

		if (this.newAccount) {
			this.$subscribeTo(AccountService.createPlexAccount(this.plexAccount), (data) => {
				if (data.isSuccess) {
					this.plexAccount.plexServers = data.value?.plexServers ?? [];
					this.isSettingUpAccount = true;
				} else {
					Log.error('Result was invalid when saving a created account', data);
					this.saving = false;
				}
			});
		} else {
			this.$subscribeTo(AccountService.updatePlexAccount(this.plexAccount), (data) => {
				if (data.isSuccess) {
					this.plexAccount.plexServers = data.value?.plexServers ?? [];
					this.isSettingUpAccount = true;
				} else {
					Log.error('Result was invalid when saving an updated account', data);
					this.saving = false;
				}
			});
		}
	}

	deleteAccount(): void {
		AccountService.deleteAccount(this.plexAccount.id).subscribe(() => {
			this.closeDialog(true);
		});
	}

	// endregion

	closeDialog(refreshAccounts: boolean = false): void {
		this.isSettingUpAccount = false;
		this.confirmationDialogState = false;
		this.saving = false;
		this.verificationCodeDialogState = false;
		this.inputHasChanged = false;
		this.reset();
		this.$emit('dialog-closed', refreshAccounts);
	}

	mounted(): void {
		this.$subscribeTo(this.$watchAsObservable('dialog').pipe(map((x) => x.newValue)), (dialogState) => {
			if (dialogState) {
				// Setup values
				if (this.account) {
					this.plexAccount = { ...this.account };
					this.isValidated = this.account.isValidated ? 'OK' : 'ERROR';
				}

				if (this.newAccount) {
					// This is a new account, generate a clientId for it
					this.$subscribeTo(generateClientId(), (value) => {
						if (value.isSuccess) {
							this.plexAccount.clientId = value.value ?? '';
						}
					});
				}
			} else {
				// Reset values
				this.closeDialog();
			}
		});
	}
}
</script>
