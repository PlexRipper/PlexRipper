<template>
	<v-form v-model="valid">
		<!-- Is account enabled -->
		<v-row no-gutters>
			<v-col :cols="labelCol">
				<help-icon help-id="help.account-form.is-enabled" />
			</v-col>
			<v-col>
				<v-checkbox v-model="isEnabled" color="red" class="ma-3 pt-0" hide-details @change="inputChanged" />
			</v-col>
		</v-row>
		<!-- Is main account -->
		<v-row no-gutters>
			<v-col :cols="labelCol">
				<help-icon help-id="help.account-form.is-main" />
			</v-col>
			<v-col>
				<v-checkbox v-model="isMain" color="red" class="ma-3 pt-0" hide-details @change="inputChanged" />
			</v-col>
		</v-row>
		<!-- Display Name -->
		<v-row no-gutters>
			<v-col :cols="labelCol">
				<help-icon help-id="help.account-form.display-name" />
			</v-col>
			<v-col>
				<v-text-field
					v-model="displayName"
					:rules="getDisplayNameRules"
					color="red"
					full-width
					outlined
					required
					@input="inputChanged"
				/>
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
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { PlexAccountDTO } from '@dto/mainApi';

@Component<AccountForm>({})
export default class AccountForm extends Vue {
	isEnabled: boolean = true;
	isMain: boolean = true;
	displayName: string = '';
	username: string = '';
	password: string = '';
	showPassword: boolean = false;

	valid: boolean = false;
	isValidated: string = '';
	labelCol: number = 3;

	@Prop({ required: false, type: Object as () => PlexAccountDTO })
	readonly value!: PlexAccountDTO | null;

	// region Validation Rules

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
	// endregion

	get getAccount(): PlexAccountDTO {
		return {
			...this.value,
			isEnabled: this.isEnabled,
			isMain: this.isMain,
			displayName: this.displayName,
			username: this.username,
			password: this.password,
		} as PlexAccountDTO;
	}

	inputChanged(): void {
		this.$emit('input', this.getAccount);
		this.$emit('is-valid', this.valid);
	}

	reset(): void {
		this.isMain = true;
		this.displayName = '';
		this.username = '';
		this.password = '';
		this.isValidated = '';
		this.valid = false;
	}

	mounted() {
		this.inputChanged();
	}
}
</script>
