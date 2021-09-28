<template>
	<v-dialog :value="dialog" persistent :max-width="isSettingUpAccount ? 1000 : 900">
		<!-- The account pop-up -->
		<v-card v-show="!isSettingUpAccount">
			<v-card-title class="headline">
				{{ getDisplayName }}
			</v-card-title>
			<v-divider></v-divider>
			<v-card-text class="mt-2">
				<v-form ref="accountForm" v-model="valid">
					<!-- Is account enabled -->
					<v-row no-gutters>
						<v-col :cols="labelCol">
							<help-icon help-id="help.account-form.is-enabled" />
						</v-col>
						<v-col>
							<v-checkbox v-model="isEnabled" color="red" class="ma-3 pt-0" hide-details></v-checkbox>
						</v-col>
					</v-row>
					<!-- Is main account -->
					<v-row no-gutters>
						<v-col :cols="labelCol">
							<help-icon help-id="help.account-form.is-main" />
						</v-col>
						<v-col>
							<v-checkbox v-model="isMain" color="red" class="ma-3 pt-0" hide-details></v-checkbox>
						</v-col>
					</v-row>
					<!-- Display Name -->
					<v-row no-gutters>
						<v-col :cols="labelCol">
							<help-icon help-id="help.account-form.display-name" />
						</v-col>
						<v-col>
							<v-text-field v-model="displayName" :rules="getDisplayNameRules" color="red" full-width outlined required />
						</v-col>
					</v-row>

					<!-- Username -->
					<v-row no-gutters>
						<v-col :cols="labelCol">
							<help-icon help-id="help.account-form.username" />
						</v-col>
						<v-col>
							<v-text-field
								v-model="username"
								:rules="getUsernameRules"
								color="red"
								full-width
								outlined
								required
								@input="inputChanged"
							/>
						</v-col>
					</v-row>

					<!-- Password -->
					<v-row no-gutters>
						<v-col :cols="labelCol">
							<help-icon help-id="help.account-form.password" />
						</v-col>
						<v-col>
							<v-text-field
								v-model="password"
								:rules="getPasswordRules"
								color="red"
								full-width
								outlined
								required
								:append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
								:type="showPassword ? 'text' : 'password'"
								@click:append="showPassword = !showPassword"
								@input="inputChanged"
							/>
						</v-col>
					</v-row>
				</v-form>
			</v-card-text>

			<!-- Dialog Actions	-->
			<v-card-actions>
				<!-- Delete account -->
				<p-btn
					v-if="!isNew"
					:width="130"
					:button-type="getDeleteButtonType"
					text-id="delete"
					@click="confirmationDialogState = true"
				/>
				<confirmation-dialog
					class="mr-4"
					text-id="delete-account"
					:dialog="confirmationDialogState"
					:confirm-loading="true"
					@cancel="confirmationDialogState = false"
					@confirm="deleteAccount"
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
					:disabled="!valid || validateLoading"
					:color="getValidationBtnColor"
					class="mr-4"
					:text-id="!valid ? 'validate' : ''"
					:width="130"
					@click="validate"
				/>

				<!-- Save account -->
				<p-btn
					:width="130"
					:disabled="!(isValidated === 'OK') || saving"
					:text-id="isNew ? 'create' : 'update'"
					:button-type="getSaveButtonType"
					@click="saveAccount"
				/>
			</v-card-actions>
		</v-card>
		<!-- The setup account progress -->
		<account-setup-progress v-show="isSettingUpAccount" :account="getAccount" @hide="closeDialog(true)" />
	</v-dialog>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import type { PlexAccountDTO } from '@dto/mainApi';
import { validateAccount } from '@api/accountApi';
import { AccountService } from '@service';
import { map } from 'rxjs/operators';
import { PlexServerDTO } from '@dto/mainApi';
import ButtonType from '@/types/enums/buttonType';

@Component
export default class AccountDialog extends Vue {
	@Prop({ required: false, type: Object as () => PlexAccountDTO })
	readonly account!: PlexAccountDTO | null;

	@Prop({ required: true, type: Boolean, default: false })
	dialog: boolean = false;

	@Ref('accountForm')
	readonly accountForm!: any;

	isSettingUpAccount: boolean = false;

	validateLoading: boolean = false;

	valid: boolean = false;

	showPassword: boolean = false;

	displayName: string = '';

	username: string = '';

	password: string = '';

	isEnabled: boolean = true;

	isMain: boolean = true;

	saving: boolean = false;

	labelCol: number = 3;

	isValidated: string = '';

	plexServers: PlexServerDTO[] = [];

	confirmationDialogState: Boolean = false;

	get isNew(): boolean {
		return !this.account;
	}

	get getDeleteButtonType(): ButtonType {
		return ButtonType.Delete;
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

	get getAccount(): PlexAccountDTO {
		return {
			id: this.isNew ? 0 : this.account?.id ?? 0,
			isEnabled: this.isEnabled,
			isMain: this.isMain,
			displayName: this.displayName,
			username: this.username,
			password: this.password,
			plexServers: this.plexServers,
		} as PlexAccountDTO;
	}

	get getDisplayName(): string {
		const title = `${this.isNew ? 'Add' : 'Edit'} Plex account`;
		return this.displayName !== '' ? `${title}: ${this.displayName}` : title;
	}

	get getDisplayNameRules(): unknown {
		return [
			(v: string): boolean | string => !!v || 'Display name is required',
			(v: string): boolean | string => (v && v.length >= 4) || 'Display name must be at least 4 characters',
		];
	}

	get getUsernameRules(): unknown {
		return [(v: string): boolean | string => !!v || 'Username is required'];
	}

	get getPasswordRules(): unknown {
		return [
			(v: string): boolean | string => !!v || 'Password is required',
			(v: string): boolean | string => (v && v.length >= 8) || 'Password must be at least 8 characters',
		];
	}

	get getValidationBtnColor(): string {
		switch (this.isValidated) {
			case 'OK':
				return 'green';
			case 'ERROR':
				return 'red';
			default:
				return '';
		}
	}

	validate(): void {
		this.accountForm?.validate();

		if (this.valid) {
			this.validateLoading = true;
			validateAccount(this.getAccount).subscribe((data) => {
				// TODO show notification with errors if any
				if (data) {
					this.isValidated = 'OK';
				} else {
					this.isValidated = 'ERROR';
				}
				this.validateLoading = false;
			});
		}
	}

	inputChanged(): void {
		if (this.isValidated === 'OK') {
			this.isValidated = '';
		}
	}

	reset(): void {
		Log.debug('Reset form');
		this.isMain = true;
		this.displayName = '';
		this.username = '';
		this.password = '';
		this.isValidated = '';
		this.resetValidation();
	}

	resetValidation(): void {
		this.valid = false;
		this.accountForm?.resetValidation();
	}

	cancel(): void {
		this.closeDialog();
	}

	saveAccount(): void {
		this.saving = true;

		this.$subscribeTo(AccountService.createOrUpdateAccount(this.getAccount), (data) => {
			if (data.isSuccess) {
				this.plexServers = data.value?.plexServers ?? [];
				this.isSettingUpAccount = true;
			} else {
				Log.error('Result was invalid when saving account', data);
			}
		});
	}

	deleteAccount(): void {
		AccountService.deleteAccount(this.getAccount.id).subscribe(() => {
			this.closeDialog(true);
		});
	}

	onOpenDialog(dialogState: boolean): void {
		if (dialogState) {
			if (this.account) {
				this.isEnabled = this.account.isEnabled;
				this.isMain = this.account.isMain;
				this.displayName = this.account.displayName;
				this.username = this.account.username;
				this.password = this.account.password;
				this.isValidated = this.account.isValidated ? 'OK' : 'ERROR';
			}
		} else {
			this.reset();
		}
	}

	closeDialog(refreshAccounts: boolean = false): void {
		this.isSettingUpAccount = false;
		this.confirmationDialogState = false;
		this.saving = false;
		this.$emit('dialog-closed', refreshAccounts);
	}

	mounted(): void {
		this.$subscribeTo(this.$watchAsObservable('dialog').pipe(map((x) => x.newValue)), (dialogState) => {
			this.onOpenDialog(dialogState);
		});
	}
}
</script>
