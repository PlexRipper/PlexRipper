<template>
	<HelpGroup>
		<HelpRow
			:label="$t('help.settings.accounts.app-username.label')"
			:title="$t('help.settings.accounts.app-username.title')"
			:text="$t('help.settings.accounts.app-username.text')">
			<q-input
				v-model="authStore.username"
				:rules="getUsernameRules"
				color="red"
				full-width
				outlined
				required
				hide-bottom-space
				data-cy="app-username-input" />
		</HelpRow>
		<HelpRow
			:label="$t('help.settings.accounts.app-password.label')"
			:title="$t('help.settings.accounts.app-password.title')"
			:text="$t('help.settings.accounts.app-password.text')">
			<PasswordInputField
				v-model="authStore.password"
				hide-mask-button
				class="q-my-md"
				cy="app-password-input" />
		</HelpRow>
		<HelpRow
			:label="$t('help.settings.accounts.app-confirm-password.label')">
			<PasswordInputField
				v-model="authStore.confirmPassword"
				hide-mask-button
				class="q-my-md"
				cy="app-password-input" />
		</HelpRow>
		<HelpRow hide-label>
			<SaveButton
				class="q-pa-md"
				block
				:disabled="!authStore.canUpdateCredentials"
				@click="onUpdateCredentials" />
		</HelpRow>
	</HelpGroup>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import SaveButton from '@components/Buttons/SaveButton.vue';

const authStore = useAuthenticationStore();
const { t } = useI18n();
const getUsernameRules = computed(() => [
	(v: string): boolean | string => !!v || t('components.authentication-overview.validation.username-is-required'),
	(v: string): boolean | string => (v && v.length >= 8) || t('components.authentication-overview.validation.username-length', {
		count: 8,
	}),
]);

function onUpdateCredentials() {
	useSubscription(authStore.updateCredentials().subscribe());
}

onMounted(() =>
	useSubscription(authStore.refreshCredentials().subscribe()),
);
</script>
