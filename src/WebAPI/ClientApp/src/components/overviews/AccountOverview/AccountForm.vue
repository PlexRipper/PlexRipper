<template>
	<v-form ref="accountForm" v-model="valid">
		<!-- Is account enabled -->
		<v-row no-gutters>
			<v-col :cols="labelCol">
				<help-icon help-id="help.account-form.is-enabled" />
			</v-col>
			<v-col>
				<v-checkbox
					:input-value="value.isEnabled"
					color="red"
					class="ma-3 pt-0"
					hide-details
					@change="inputChanged({ prop: 'isEnabled', value: $event })"
				/>
			</v-col>
		</v-row>
		<!-- Is main account -->
		<v-row no-gutters>
			<v-col :cols="labelCol">
				<help-icon help-id="help.account-form.is-main" />
			</v-col>
			<v-col>
				<v-checkbox
					:input-value="value.isMain"
					color="red"
					class="ma-3 pt-0"
					hide-details
					@change="inputChanged({ prop: 'isMain', value: $event })"
				/>
			</v-col>
		</v-row>
		<!-- Display Name -->
		<v-row no-gutters>
			<v-col :cols="labelCol">
				<help-icon help-id="help.account-form.display-name" />
			</v-col>
			<v-col>
				<v-text-field
					:value="value.displayName"
					:rules="getDisplayNameRules"
					color="red"
					full-width
					outlined
					required
					@input="inputChanged({ prop: 'displayName', value: $event })"
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
					:value="value.username"
					:rules="getUsernameRules"
					color="red"
					full-width
					outlined
					required
					@input="inputChanged({ prop: 'username', value: $event })"
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
					:value="value.password"
					:rules="getPasswordRules"
					color="red"
					full-width
					outlined
					required
					:append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
					:type="showPassword ? 'text' : 'password'"
					@click:append="showPassword = !showPassword"
					@input="inputChanged({ prop: 'password', value: $event })"
				/>
			</v-col>
		</v-row>
	</v-form>
</template>

<script lang="ts">
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import { PlexAccountDTO } from '@dto/mainApi';
import VForm from 'vuetify/lib/components/VForm/VForm.js';

@Component<AccountForm>({})
export default class AccountForm extends Vue {
	labelCol: number = 3;
	valid: boolean = true;
	isValidated: string = '';
	showPassword: boolean = false;

	@Prop({ required: false, type: Object as () => PlexAccountDTO })
	readonly value!: PlexAccountDTO | null;

	@Ref('accountForm')
	readonly accountForm!: VForm;

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

	inputChanged({ prop, value }: { prop: string; value: string | boolean }): void {
		this.$emit('input', { prop, value });
		this.$emit('is-valid', this.valid);
	}

	reset(): void {
		this.isValidated = '';
		this.valid = true;
		this.showPassword = false;
		this.accountForm.resetValidation();
	}
}
</script>
