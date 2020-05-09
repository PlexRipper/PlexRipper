<template>
	<v-dialog v-model="dialog" persistent max-width="700">
		<template v-slot:activator="{ on }">
			<!-- Add new account -->
			<v-card v-if="isNew" v-on="on" hover>
				<v-card-text class="text-center">
					<v-icon style="font-size: 100px; height: 100px">mdi-plus-box-outline</v-icon>
				</v-card-text>
			</v-card>
			<!-- Edit Account -->
			<v-card v-else v-on="on" hover>
				<v-card-title class="headline">{{ account ? account.username : '' }}</v-card-title>
				<v-card-text>
					<template>
						<v-chip v-if="account.isConfirmed" class="ma-2" color="green" text-color="white">
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
				<v-form ref="form" v-model="valid" lazy-validation>
					<!-- Display Name -->
					<v-text-field
						v-model="displayName"
						label="Display Name"
						full-width
						single-line
						outlined
						:value="account ? account.displayName : ''"
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
						:value="account ? account.username : ''"
						required
					/>

					<!-- Password -->
					<v-text-field
						v-model="password"
						:rules="getPasswordRules"
						label="Password"
						full-width
						single-line
						outlined
						:value="account ? account.password : ''"
						required
					/>
				</v-form>
			</v-card-text>
			<v-card-actions>
				<v-btn color="error" class="mr-4" @click="reset">
					Reset Form
				</v-btn>
				<v-btn v-if="!isNew" color="error" class="mr-4" @click="reset">
					Delete
				</v-btn>
				<v-spacer />
				<v-btn :disabled="!valid" color="success" class="mr-4" @click="validate">
					Validate
				</v-btn>
				<v-btn :disabled="!valid" color="warning" class="mr-4" @click="confirm">
					Cancel
				</v-btn>
				<v-btn :disabled="!valid" color="success" class="mr-4" @click="confirm">
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

interface IAccount {
	username: string;
	password: string;
	isConfirmed: boolean;
}

@Component
export default class AccountCard extends Vue {
	@Prop({ type: Object as () => IAccount })
	readonly account!: IAccount;

	dialog: boolean = false;

	valid: boolean = false;

	displayName: string = '';

	username: string = '';

	password: string = '';

	get isNew(): boolean {
		return !this.account;
	}

	get getDisplayName(): string {
		return this.displayName !== '' ? `Plex account: ${this.displayName}` : 'Plex account';
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

	get getForm(): Vue & { validate: () => boolean; reset: () => void; resetValidation: () => void } {
		return this.$refs.form as Vue & { validate: () => boolean; reset: () => void; resetValidation: () => void };
	}

	checkAccount(account: IAccount): void {
		Log.debug(account);
	}

	validate(): void {
		this.getForm.validate();
	}

	reset(): void {
		this.getForm.reset();
	}

	resetValidation(): void {
		this.getForm.resetValidation();
	}

	confirm(): void {
		this.dialog = false;
	}
}
</script>
