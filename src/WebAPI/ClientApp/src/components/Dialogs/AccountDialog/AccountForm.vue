<template>
	<q-form
		ref="accountForm"
		greedy
		autofocus
		autocapitalize="off"
		spellcheck="false"
		@validation-success="accountDialogStore.isInputValid = true"
		@validation-error="accountDialogStore.isInputValid = false">
		<HelpGroup>
			<!-- Is account enabled -->
			<HelpRow
				:label="$t('help.account-form.is-enabled.label')"
				:title="$t('help.account-form.is-enabled.title')"
				:text="$t('help.account-form.is-enabled.text')">
				<q-toggle
					v-model="accountDialogStore.isEnabled"
					class="q-ma-sm pt-0"
					color="red"
					data-cy="account-form-is-enabled" />
			</HelpRow>

			<!-- Is main account -->
			<HelpRow
				:label="$t('help.account-form.is-main.label')"
				:title="$t('help.account-form.is-main.title')"
				:text="$t('help.account-form.is-main.text')">
				<q-toggle
					v-model="accountDialogStore.isMain"
					class="q-ma-sm pt-0"
					color="red"
					data-cy="account-form-is-main" />
			</HelpRow>

			<!-- Display Name -->
			<HelpRow
				:label="$t('help.account-form.display-name.label')"
				:title="$t('help.account-form.display-name.title')"
				:text="$t('help.account-form.display-name.text')">
				<q-input
					v-model="accountDialogStore.displayName"
					:rules="getDisplayNameRules"
					color="red"
					full-width
					outlined
					required
					hide-bottom-space
					data-cy="account-form-display-name-input" />
			</HelpRow>
		</HelpGroup>
		<q-tabs
			v-model="tab"
			class="q-my-lg"
			align="justify">
			<q-tab
				data-cy="account-dialog-credentials-mode-button"
				:name="credentialsTab"
				:label="$t('components.account-form.credentials-tab')" />
			<q-tab
				data-cy="account-dialog-auth-token-mode-button"
				:name="tokenTab"
				:label="$t('components.account-form.token-tab')" />
		</q-tabs>
		<q-tab-panels
			v-model="tab"
			animated>
			<q-tab-panel
				:name="credentialsTab"
				class="account-dialog-panel">
				<HelpGroup>
					<!-- Username -->
					<HelpRow
						:label="$t('help.account-form.username.label')"
						:title="$t('help.account-form.username.title')"
						:text="$t('help.account-form.username.text')">
						<q-input
							v-model="accountDialogStore.username"
							:rules="getUsernameRules"
							color="red"
							full-width
							outlined
							required
							hide-bottom-space
							data-cy="account-form-username-input" />
					</HelpRow>

					<!-- Password -->
					<HelpRow
						:label="$t('help.account-form.password.label')"
						:title="$t('help.account-form.password.title')"
						:text="$t('help.account-form.password.text')">
						<PasswordInputField
							v-model="accountDialogStore.password"
							class="q-my-md"
							cy="account-form-password-input" />
					</HelpRow>
				</HelpGroup>
			</q-tab-panel>

			<q-tab-panel
				:name="tokenTab"
				class="account-dialog-panel">
				<HelpGroup>
					<!-- Plex Token -->
					<HelpRow
						:label="$t('help.account-form.auth-token.label')"
						:title="$t('help.account-form.auth-token.title')"
						:text="$t('help.account-form.auth-token.text')">
						<PasswordInputField
							v-model="accountDialogStore.authenticationToken"
							class="q-my-md"
							cy="account-form-auth-token-input" />
					</HelpRow>
				</HelpGroup>
			</q-tab-panel>
		</q-tab-panels>
	</q-form>
</template>

<script setup lang="ts">
import { useAccountDialogStore } from '@store';

const tokenTab = 'token';
const credentialsTab = 'credentials';

const tab = computed({
	get: () => accountDialogStore.isAuthTokenMode ? tokenTab : credentialsTab,
	set: (value: string) => accountDialogStore.isAuthTokenMode = value === tokenTab,
});
const accountDialogStore = useAccountDialogStore();

const getDisplayNameRules = computed(() => [
	(v: string): boolean | string => !!v || 'Display name is required',
	(v: string): boolean | string => (v && v.length >= 4) || 'Display name must be at least 4 characters',
]);

const getUsernameRules = computed(() => [(v: string): boolean | string => !!v || 'Username is required']);
</script>

<style lang="scss">
.account-dialog-panel {
  min-height: 11rem;
}
</style>
