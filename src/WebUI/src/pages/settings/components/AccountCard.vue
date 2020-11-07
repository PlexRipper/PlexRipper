<template>
	<v-dialog v-model="dialog" persistent max-width="800">
		<template v-slot:activator="{ on }">
			<!-- Add new account -->
			<v-card v-if="isNew" hover max-height="130" v-on="on" @click="openDialog()">
				<v-card-text class="text-center">
					<v-icon style="font-size: 100px">mdi-plus-box-outline</v-icon>
				</v-card-text>
			</v-card>
			<!-- Edit Account -->
			<v-card v-else hover max-height="130" v-on="on" @click="openDialog()">
				<v-card-title class="headline">{{ account ? account.displayName : '' }}</v-card-title>
				<v-card-text>
					<template>
						<v-chip v-if="account.isValidated" class="ma-2" color="green" text-color="white"> Validated </v-chip>
						<v-chip v-else class="ma-2" color="red" text-color="white"> NotValidated </v-chip>
					</template>

					<v-chip v-if="account.isEnabled" class="ma-2" color="green" text-color="white"> Enabled </v-chip>
					<v-chip v-else class="ma-2" color="red" text-color="white"> Disabled </v-chip>
				</v-card-text>
			</v-card>
		</template>
		<!-- The account pop-up -->
		<v-card>
			<v-card-title class="headline">
				{{ getDisplayName }}
			</v-card-title>
			<v-divider></v-divider>
			<v-card-text class="mt-2">
				<v-form ref="form" v-model="valid">
					<!-- Is account enabled -->
					<v-row no-gutters>
						<v-col cols="3">
							<help-icon label="Is Enabled:" />
						</v-col>
						<v-col>
							<v-checkbox v-model="isEnabled" color="red" class="mt-0 pt-0" hide-details></v-checkbox>
						</v-col>
					</v-row>
					<!-- Is main account -->
					<v-row no-gutters>
						<v-col cols="3">
							<help-icon label="Is Main Account:" />
						</v-col>
						<v-col>
							<v-checkbox v-model="isMain" color="red" class="mt-0 pt-0" hide-details></v-checkbox>
						</v-col>
					</v-row>
					<!-- Display Name -->
					<v-row no-gutters>
						<v-col cols="3">
							<help-icon label="Display Name:" />
						</v-col>
						<v-col>
							<v-text-field v-model="displayName" :rules="getDisplayNameRules" color="red" full-width outlined required />
						</v-col>
					</v-row>

					<!-- Username -->
					<v-row no-gutters>
						<v-col cols="3">
							<help-icon label="Username:" />
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
						<v-col cols="3">
							<help-icon label="Password:" />
						</v-col>
						<v-col>
							<v-text-field
								v-model="password"
								:rules="getPasswordRules"
								color="red"
								full-width
								outlined
								required
								@input="inputChanged"
							/>
						</v-col>
					</v-row>
				</v-form>
			</v-card-text>
			<v-card-actions>
				<!-- Delete account -->
				<v-btn v-if="!isNew" color="error" class="mr-4" min-width="130" @click="deleteAccount">
					<v-icon>mdi-delete</v-icon>
					Delete
				</v-btn>
				<!-- Reset Form -->
				<v-btn class="mr-4" min-width="130" @click="reset">
					<v-icon>mdi-restore</v-icon>
					Reset
				</v-btn>
				<v-spacer />
				<!-- Validation button -->
				<v-btn
					:loading="validateLoading"
					:disabled="!valid || validateLoading"
					:color="getValidationBtnColor"
					class="mr-4"
					min-width="130"
					@click="validate"
				>
					<v-icon v-if="isValidated === 'OK'">mdi-check-bold</v-icon>
					<v-icon v-else-if="isValidated == 'ERROR'">mdi-alert-circle-outline</v-icon>
					<span v-else>
						<v-icon>mdi-text-box-search-outline</v-icon>
						Validate
					</span>
				</v-btn>
				<!-- Cancel button -->
				<v-btn class="mr-4" min-width="130" @click="cancel">
					<v-icon>mdi-cancel</v-icon>
					Cancel
				</v-btn>
				<!-- Save account -->
				<v-btn :disabled="!(isValidated === 'OK')" color="success" min-width="130" class="mr-4" @click="saveAccount">
					<v-icon>mdi-content-save</v-icon>
					{{ isNew ? 'Create' : 'Update' }}
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
	<!-- Plex Accounts -->
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import Log from 'consola';
import type { PlexAccountDTO } from '@dto/mainApi';
import { validateAccount, createAccount, deleteAccount, updateAccount } from '@api/accountApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import HelpIcon from '@components/Help/HelpIcon.vue';

@Component({
	components: {
		LoadingSpinner,
		HelpIcon,
	},
})
export default class AccountCard extends Vue {
	@Prop({ type: Object as () => PlexAccountDTO })
	readonly account!: PlexAccountDTO;

	validateLoading: boolean = false;

	dialog: boolean = false;

	valid: boolean = false;

	displayName: string = '';

	username: string = '';

	password: string = '';

	isEnabled: boolean = true;

	isMain: boolean = true;

	isValidated: string = '';

	get isNew(): boolean {
		return !this.account;
	}

	get getAccount(): PlexAccountDTO {
		return {
			id: this.isNew ? 0 : this.account.id,
			isEnabled: this.isEnabled,
			isMain: this.isMain,
			displayName: this.displayName,
			username: this.username,
			password: this.password,
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
				return 'success';
			case 'ERROR':
				return 'error';
			default:
				return '';
		}
	}

	get getForm(): Vue & { validate: () => boolean; reset: () => void; resetValidation: () => void } {
		return this.$refs.form as Vue & { validate: () => boolean; reset: () => void; resetValidation: () => void };
	}

	validate(): void {
		this.getForm.validate();

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
		this.isMain = false;
		this.displayName = '';
		this.username = '';
		this.password = '';
		this.isValidated = '';
		this.resetValidation();
	}

	resetValidation(): void {
		this.valid = false;
		this.getForm.resetValidation();
	}

	cancel(): void {
		this.closeDialog();
	}

	saveAccount(): void {
		if (this.isNew) {
			createAccount(this.getAccount).subscribe(() => {
				this.closeDialog();
			});
		} else {
			updateAccount(this.getAccount).subscribe(() => {
				this.closeDialog();
			});
		}
	}

	deleteAccount(): void {
		deleteAccount(this.account.id).subscribe(() => {
			this.closeDialog();
		});
	}

	openDialog(): void {
		if (this.account) {
			this.isEnabled = this.account.isEnabled;
			this.isMain = this.account.isMain;
			this.displayName = this.account.displayName;
			this.username = this.account.username;
			this.password = this.account.password;
			this.isValidated = this.account.isValidated ? 'OK' : 'ERROR';
		}
		this.dialog = true;
	}

	closeDialog(): void {
		this.$emit('dialog-closed');
		this.dialog = false;
	}
}
</script>
