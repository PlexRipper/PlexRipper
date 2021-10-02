<template>
	<v-dialog :value="dialog" persistent :max-width="isSettingUpAccount ? 1000 : 900">
		<!-- The account pop-up -->
		<v-card v-show="!isSettingUpAccount">
			<v-card-title class="headline">
				{{ getDisplayName }}
			</v-card-title>
			<v-divider></v-divider>
			<v-card-text class="mt-2">
				<account-form ref="accountForm" v-model="plexAccount" @isValid="isValid = $event" />
				{{ plexAccount }}
				{{ isValid }}
				{{ isValidated }}
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

				<!--	Account Verification Code Dialog	-->
				<p-btn :width="130" :button-type="getVerificationCodeButtonType" @click="verificationCodeDialogState = true" />
				<!-- Reset Form -->
				<p-btn icon="mdi-restore" text-id="reset" :width="130" @click="reset" />
				<v-spacer />

				<!-- Cancel button -->
				<p-btn :width="130" :button-type="getCancelButtonType" @click="cancel" />

				<!-- Validation button -->
				{{ plexAccount.verificationCode }}
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
					:disabled="!(isValidated === 'OK') || saving"
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
			@close="verificationCodeDialogState = false"
			@submit="sendVerificationCode"
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
import { iif, of } from 'rxjs';
import type { PlexAccountDTO } from '@dto/mainApi';
import { generateClientId, validateAccount } from '@api/accountApi';
import { AccountService } from '@service';
import { map, switchMap, tap } from 'rxjs/operators';
import { PlexServerDTO } from '@dto/mainApi';
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

	plexAccount: PlexAccountDTO = {} as PlexAccountDTO;

	isSettingUpAccount: boolean = false;

	validateLoading: boolean = false;
	isValidated: string = '';
	isValid: boolean = true;

	saving: boolean = false;

	plexServers: PlexServerDTO[] = [];

	confirmationDialogState: Boolean = false;
	verificationCodeDialogState: Boolean = false;

	get isFormValid(): boolean {
		return this.isValid && this.isValidated === 'OK';
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

	validate(): void {
		this.validateLoading = true;

		this.$subscribeTo(
			validateAccount(this.plexAccount).pipe(
				tap((data) => {
					if (data.isSuccess && data.value) {
						this.plexAccount = { ...this.plexAccount, ...data.value, verificationCode: '' };

						Log.info('PlexAccount', this.plexAccount);
						if (this.plexAccount.is2Fa) {
							this.verificationCodeDialogState = true;
						}
					}
				}),
				switchMap(() => iif(() => this.plexAccount.is2Fa, validateAccount(this.plexAccount), of('validated'))),
			),
			(data) => {
				Log.info('Authpin', data);
				// TODO show notification with errors if any
				if (data === 'validated') {
					this.verificationCodeDialogState = false;
				}

				this.validateLoading = false;
			},
		);
	}

	// region Button Commands

	reset(): void {
		this.accountForm?.reset();
	}

	cancel(): void {
		this.closeDialog();
	}

	saveAccount(): void {
		this.saving = true;

		this.$subscribeTo(AccountService.createOrUpdateAccount(this.plexAccount), (data) => {
			if (data.isSuccess) {
				this.plexServers = data.value?.plexServers ?? [];
				this.isSettingUpAccount = true;
			} else {
				Log.error('Result was invalid when saving account', data);
			}
		});
	}

	deleteAccount(): void {
		AccountService.deleteAccount(this.plexAccount.id).subscribe(() => {
			this.closeDialog(true);
		});
	}

	// endregion

	closeDialog(refreshAccounts: boolean = false): void {
		this.$emit('dialog-closed', refreshAccounts);
	}

	sendVerificationCode(verificationCode: string) {
		this.plexAccount.verificationCode = verificationCode;
		this.validate();
	}

	mounted(): void {
		this.$subscribeTo(this.$watchAsObservable('dialog').pipe(map((x) => x.newValue)), (dialogState) => {
			if (dialogState) {
				// Setup values
				if (this.account) {
					this.plexAccount = this.account;
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
				this.isSettingUpAccount = false;
				this.confirmationDialogState = false;
				this.saving = false;

				this.reset();
				this.closeDialog();
			}
		});
	}
}
</script>
