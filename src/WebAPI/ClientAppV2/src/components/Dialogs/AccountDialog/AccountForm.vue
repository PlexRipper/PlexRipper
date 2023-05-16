<template>
	<QForm
		ref="accountForm"
		greedy
		autofocus
		autocapitalize="off"
		spellcheck="false"
		@reset="onReset"
		@validation-success="$emit('is-valid', true)"
		@validation-error="$emit('is-valid', false)">
		<!-- Is account enabled -->
		<q-row no-gutters align="center">
			<q-col :cols="labelCol">
				<help-icon help-id="help.account-form.is-enabled" />
			</q-col>
			<q-col>
				<q-toggle
					class="q-ma-sm pt-0"
					:model-value="value.isEnabled"
					color="red"
					data-cy="account-form-is-enabled"
					@update:model-value="inputChanged({ prop: 'isEnabled', value: $event })" />
			</q-col>
		</q-row>
		<!-- Is main account -->
		<q-row no-gutters align="center">
			<q-col :cols="labelCol">
				<help-icon help-id="help.account-form.is-main" />
			</q-col>
			<q-col>
				<q-toggle
					class="q-ma-sm pt-0"
					:model-value="value.isMain"
					color="red"
					data-cy="account-form-is-main"
					@update:model-value="inputChanged({ prop: 'isMain', value: $event })" />
			</q-col>
		</q-row>
		<!-- Display Name -->
		<q-row no-gutters align="center">
			<q-col :cols="labelCol" class="q-mb-md">
				<help-icon help-id="help.account-form.display-name" />
			</q-col>
			<q-col>
				<q-input
					:model-value="value.displayName"
					:rules="getDisplayNameRules"
					color="red"
					full-width
					outlined
					required
					data-cy="account-form-display-name-input"
					@update:model-value="inputChanged({ prop: 'displayName', value: $event })" />
			</q-col>
		</q-row>

		<!-- Username -->
		<q-row no-gutters align="center" class="q-mt-md">
			<q-col :cols="labelCol" class="q-mb-md">
				<help-icon help-id="help.account-form.username" />
			</q-col>
			<q-col>
				<q-input
					:model-value="value.username"
					:rules="getUsernameRules"
					color="red"
					full-width
					outlined
					required
					data-cy="account-form-username-input"
					@update:model-value="inputChanged({ prop: 'username', value: $event })" />
			</q-col>
		</q-row>

		<!-- Password -->
		<q-row no-gutters align="center" class="q-mt-md">
			<q-col :cols="labelCol" class="q-mb-md">
				<help-icon help-id="help.account-form.password" />
			</q-col>
			<q-col>
				<q-input
					:model-value="value.password"
					:rules="getPasswordRules"
					color="red"
					full-width
					outlined
					required
					data-cy="account-form-password-input"
					:append-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
					:type="showPassword ? 'text' : 'password'"
					@click:append="showPassword = !showPassword"
					@update:model-value="inputChanged({ prop: 'password', value: $event })">
					<template #append>
						<q-btn flat :icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'" @click="showPassword = !showPassword" />
					</template>
				</q-input>
			</q-col>
		</q-row>
	</QForm>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { PlexAccountDTO } from '@dto/mainApi';
import { QForm } from '#components';

const labelCol = ref(3);
const showPassword = ref(false);
const accountForm = ref<InstanceType<typeof QForm> | null>(null);

defineProps<{
	value: PlexAccountDTO;
}>();

const emit = defineEmits<{
	(event: 'input', value: { prop: string; value: string | boolean }): void;
	(event: 'is-valid', valid: boolean): void;
}>();

// region Validation Rules

const getDisplayNameRules = computed(() => [
	(v: string): boolean | string => !!v || 'Display name is required',
	(v: string): boolean | string => (v && v.length >= 4) || 'Display name must be at least 4 characters',
]);

const getUsernameRules = computed(() => [(v: string): boolean | string => !!v || 'Username is required']);

const getPasswordRules = computed(() => [
	(v: string): boolean | string => !!v || 'Password is required',
	(v: string): boolean | string => (v && v.length >= 8) || 'Password must be at least 8 characters',
]);

// endregion

function inputChanged({ prop, value }: { prop: string; value: string | boolean }): void {
	emit('input', { prop, value });
}

function onReset(): void {
	set(showPassword, false);
	get(accountForm)?.resetValidation();
}

defineExpose({
	onReset,
});
</script>
