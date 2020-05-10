<template>
	<v-dialog v-model="dialog" persistent max-width="800">
		<template v-slot:activator="{ on }">
			<!-- Add new account -->
			<v-card v-if="isNew" hover v-on="on" @click="openDialog()">
				<v-card-text class="text-center">
					<v-icon style="font-size: 100px; height: 100px">mdi-plus-box-outline</v-icon>
				</v-card-text>
			</v-card>
			<!-- Edit Account -->
			<v-card v-else hover v-on="on" @click="openDialog()">
				<v-card-title class="headline">{{ account ? account.displayName : '' }} - {{ account ? account.id : -1 }}</v-card-title>
				<v-card-text>
					<template>
						<v-chip v-if="account.isValidated" class="ma-2" color="green" text-color="white">
							Validated
						</v-chip>
						<v-chip v-else class="ma-2" color="red" text-color="white">
							NotValidated
						</v-chip>
					</template>

					<v-chip class="ma-2" color="green" text-color="white">
						Green Chip
					</v-chip>
				</v-card-text>
			</v-card>
		</template>
		<!-- The account pop-up -->
		<v-card>
			<v-card-title class="headline">{{ getDisplayName }}</v-card-title>
			<v-card-text>
				<v-form ref="form" v-model="valid">
					<!-- Is account enabled -->
					<v-switch v-model="isEnabled" :dark="$vuetify.theme.dark" label="Is Enabled:"></v-switch>
					<!-- Display Name -->
					<v-text-field
						v-model="displayName"
						:rules="getDisplayNameRules"
						label="Display Name"
						full-width
						single-line
						outlined
						:dark="$vuetify.theme.dark"
						required
					/>
					<!-- Username -->
					<v-text-field
						v-model="username"
						:rules="getUsernameRules"
						label="Username"
						full-width
						single-line
						outlined
						:dark="$vuetify.theme.dark"
						required
						@input="inputChanged"
					/>

					<!-- Password -->
					<v-text-field
						v-model="password"
						:rules="getPasswordRules"
						label="Password"
						full-width
						single-line
						outlined
						:dark="$vuetify.theme.dark"
						required
						@input="inputChanged"
					/>
				</v-form>
			</v-card-text>
			<v-card-actions>
				<v-btn color="error" class="mr-4" min-width="130" @click="reset">
					<v-icon>mdi-restore</v-icon>
					Reset
				</v-btn>
				<!-- Delete account -->
				<v-btn v-if="!isNew" class="mr-4" min-width="130" @click="deleteAccount">
					<v-icon>mdi-delete</v-icon>
					Delete
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
					Save
				</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
	<!-- Plex Accounts -->
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import Log from 'consola';
import IAccount from '@dto/IAccount';
import LoadingSpinner from '@/components/LoadingSpinner.vue';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class AccountCard extends Vue {
	@Prop({ type: Object as () => IAccount })
	readonly account!: IAccount;

	validateLoading: boolean = false;

	dialog: boolean = false;

	valid: boolean = false;

	displayName: string = '';

	username: string = '';

	password: string = '';

	isEnabled: boolean = true;

	isValidated: string = '';

	get isNew(): boolean {
		return !this.account;
	}

	get getDisplayName(): string {
		return this.displayName !== '' ? `Plex account: ${this.displayName}` : 'Plex account';
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

	checkAccount(account: IAccount): void {
		Log.debug(account);
	}

	async validate(): Promise<void> {
		this.getForm.validate();

		if (this.valid) {
			this.validateLoading = true;
			await this.$axios
				.post('/accounts/validate', {
					displayName: this.displayName,
					username: this.username,
					password: this.password,
				})
				.then((res) => {
					Log.debug('The validation api result: ', res);
					if (res.status === 200) {
						this.isValidated = 'OK';
					}
				})
				.catch((e) => {
					Log.error('Validation Api Error: ', e);
					if (e.response.status === 401) {
						this.isValidated = 'ERROR';
					}

					if (e.response.status === 422) {
						this.isValidated = 'ERROR';
						// TODO Add pop-up error dialog
					}
				});
			this.validateLoading = false;
		}
	}

	inputChanged(): void {
		if (this.isValidated === 'OK') {
			this.isValidated = '';
		}
	}

	reset(): void {
		Log.debug('Reset form');
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
		this.dialog = false;
	}

	async saveAccount(): Promise<void> {
		await this.$axios
			.post('/accounts/', {
				isEnabled: this.isEnabled,
				displayName: this.displayName,
				username: this.username,
				password: this.password,
			})
			.then((res) => {
				Log.debug('The validation api result: ', res);
				if (res.status === 200) {
					this.isValidated = 'OK';
				}
			})
			.catch((e) => {
				Log.error('Validation Api Error: ', e);
				if (e.response.status === 401) {
					this.isValidated = 'ERROR';
				}

				if (e.response.status === 422) {
					this.isValidated = 'ERROR';
					// TODO Add pop-up error dialog
				}
			});
		this.closeDialog();
	}

	async deleteAccount(): Promise<void> {
		if (this.account?.id) {
			await this.$axios
				.delete(`/accounts/${this.account.id}`)
				.then(() => {
					this.closeDialog();
				})
				.catch((e) => {
					Log.error('Accounts API Error:', e);
				});
			return;
		}
		Log.error('Could not delete Account as the ID is not available');
	}

	openDialog(): void {
		if (this.account) {
			this.isEnabled = this.account.isEnabled;
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
