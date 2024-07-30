<template>
	<q-form
		ref="accountForm"
		greedy
		autofocus
		autocapitalize="off"
		spellcheck="false"
		@reset="onReset"
		@validation-success="$emit('is-valid', true)"
		@validation-error="$emit('is-valid', false)">
		<!-- Is account enabled -->
		<HelpRow
			:header-width="labelCol"
			:label="$t('help.account-form.is-enabled.label')"
			:title="$t('help.account-form.is-enabled.title')"
			:text="$t('help.account-form.is-enabled.text')">
			<q-toggle
				class="q-ma-sm pt-0"
				:model-value="value.isEnabled"
				color="red"
				data-cy="account-form-is-enabled"
				@update:model-value="inputChanged({ prop: 'isEnabled', value: $event })" />
		</HelpRow>

		<!-- Is main account -->
		<HelpRow
			:header-width="labelCol"
			:label="$t('help.account-form.is-main.label')"
			:title="$t('help.account-form.is-main.title')"
			:text="$t('help.account-form.is-main.text')">
			<q-toggle
				class="q-ma-sm pt-0"
				:model-value="value.isMain"
				color="red"
				data-cy="account-form-is-main"
				@update:model-value="inputChanged({ prop: 'isMain', value: $event })" />
		</HelpRow>

		<!-- Display Name -->
		<HelpRow
			:header-width="labelCol"
			:label="$t('help.account-form.display-name.label')"
			:title="$t('help.account-form.display-name.title')"
			:text="$t('help.account-form.display-name.text')">
			<q-input
				:model-value="value.displayName"
				:rules="getDisplayNameRules"
				color="red"
				full-width
				outlined
				required
				data-cy="account-form-display-name-input"
				@update:model-value="inputChanged({ prop: 'displayName', value: $event })" />
		</HelpRow>

		<!-- Username -->
		<HelpRow
			:header-width="labelCol"
			:label="$t('help.account-form.username.label')"
			:title="$t('help.account-form.username.title')"
			:text="$t('help.account-form.username.text')">
			<q-input
				:model-value="value.username"
				:rules="getUsernameRules"
				color="red"
				full-width
				outlined
				required
				data-cy="account-form-username-input"
				@update:model-value="inputChanged({ prop: 'username', value: $event })" />
		</HelpRow>

		<!-- Password -->
		<HelpRow
			:label="$t('help.account-form.password.label')"
			:title="$t('help.account-form.password.title')"
			:text="$t('help.account-form.password.text')">
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
					<q-btn
						flat
						:icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
						@click="showPassword = !showPassword" />
				</template>
			</q-input>
		</HelpRow>
	</q-form>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import type { PlexAccountDTO } from '@dto';
import { QForm } from '#components';

const labelCol = ref(30);
const showPassword = ref(false);
const accountForm = ref<InstanceType<typeof QForm> | null>(null);

defineProps<{
	value: PlexAccountDTO;
}>();

const emit = defineEmits<{
	(event: 'input', value: { prop: string; value: string | number | null }): void;
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

function inputChanged({ prop, value }: { prop: string; value: string | number | null }): void {
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
