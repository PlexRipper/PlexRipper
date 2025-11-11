<template>
	<QSection header="Radarr">
		<q-stepper
			ref="stepper"
			v-model="step"
			alternative-labels
			animated
			color="primary"
			flat
			header-nav>
			<!-- Setup Radarr Connection -->
			<QStep
				:done="testSuccess"
				:error="!testSuccess && testMessage !== ''"
				:name="1"
				done-color="positive"
				done-icon="mdi-check"
				active-icon="mdi-connection"
				:title="t('components.radarr-integration.nav-bar.connection.title')">
				<!-- Base URL -->
				<HelpRow
					:col-label="3"
					:label="t('help.settings.integrations.radarr.base-url-input.label')"
					:text="t('help.settings.integrations.radarr.base-url-input.text')"
					:title="t('help.settings.integrations.radarr.base-url-input.title')">
					<QInput
						v-model="settingsStore.integrationsSettings.radarr.radarrBaseUrl"
						hint="http://localhost:7878" />
				</HelpRow>
				<!-- API Key -->
				<HelpRow
					:col-label="3"
					:label="t('help.settings.integrations.radarr.api-key-input.label')"
					:text="t('help.settings.integrations.radarr.api-key-input.text')"
					:title="t('help.settings.integrations.radarr.api-key-input.title')">
					<ApiKeyInputField
						v-model="settingsStore.integrationsSettings.radarr.radarrApiKey"
						v-model:has-focus="passwordInputFocus"
						cy="radarr-api-key-input"
						hint="a02a22a436504e15b7e46764b12825db"
						show-strength />
				</HelpRow>

				<!-- Test Connection -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.radarr.test-connection.title')"
					:title="t('help.settings.integrations.radarr.test-connection.text')"
					hide-label>
					<BaseButton
						:loading="isTesting"
						icon="mdi-connection"
						label="Test Connection"
						@click="testRadarrConnection" />
				</HelpRow>
			</QStep>
			<!-- Configure Radarr Integration -->
			<QStep
				:done="settingsStore.integrationsSettings.radarr.isConfigured"
				:name="2"
				done-color="positive"
				done-icon="mdi-check"
				icon="mdi-cog"
				active-icon="mdi-cog"
				:disable="testSuccess === false"
				:title="t('components.radarr-integration.nav-bar.configure.title')">
				<!-- Setup Radarr Integration -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.radarr.setup-configuration.text')"
					:title="t('help.settings.integrations.radarr.setup-configuration.title')"
					hide-label>
					<BaseButton
						:loading="isConfiguring"
						icon="mdi-connection"
						:label="t('components.radarr-integration.nav-bar.configure.button')"
						@click="configureRadarrSetup" />
				</HelpRow>
			</QStep>
		</q-stepper>
		<!-- Test Connection Status -->
		<QRow>
			<QCol>
				<QAlert
					v-if="configuringSuccess !== null"
					:type="configuringSuccess ? NotificationLevel.Success : NotificationLevel.Error">
					{{ configuringSuccess ? configuringMessage : formatErrorResponse(error) }}
				</QAlert>
				<!-- Always returns 200 -->
				<QAlert
					v-else-if="testSuccess !== null"
					:type="testSuccess ? NotificationLevel.Success : NotificationLevel.Error">
					{{ testMessage }}
				</QAlert>
			</QCol>
		</QRow>
	</QSection>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import { set } from '@vueuse/core';
import { useSettingsStore } from '@store';
import { integrationApi } from '@api';
import { type BaseResultDTO, NotificationLevel, TestConnectionStatus } from '@dto';
import { tap } from 'rxjs/operators';
import { useSubscription } from '@vueuse/rxjs';
import { formatErrorResponse } from '@composables';

const settingsStore = useSettingsStore();
const { t } = useI18n();

const step = ref(1);
const passwordInputFocus = ref(false);
const isTesting = ref(false);
const isConfiguring = ref(false);
const testSuccess = ref(false);
const testMessage = ref('');
const configuringSuccess = ref(false);
const configuringMessage = ref('');
const error = ref<BaseResultDTO | null>(null);

function testRadarrConnection() {
	if (settingsStore.integrationsSettings.radarr.radarrBaseUrl === '' || settingsStore.integrationsSettings.radarr.radarrApiKey === '') {
		return;
	}

	set(isTesting, true);
	set(testMessage, '');
	useSubscription(integrationApi.testConnectionToRadarrEndpoint({
		url: settingsStore.integrationsSettings.radarr.radarrBaseUrl,
		apiKey: settingsStore.integrationsSettings.radarr.radarrApiKey,
	}).subscribe((response) => {
		if (response.isSuccess) {
			switch (response.value?.result) {
				case TestConnectionStatus.Success:
					set(testSuccess, true);
					set(testMessage, t('components.radarr-integration.connection-status.success'));
					set(step, settingsStore.integrationsSettings.radarr.isConfigured ? 3 : 2);
					break;
				case TestConnectionStatus.InvalidApiKey:
					set(testSuccess, false);
					set(testMessage, t('components.radarr-integration.connection-status.invalid-api-key'));
					break;
				case TestConnectionStatus.ConnectionFailed:
					set(testSuccess, false);
					set(testMessage, t('components.radarr-integration.connection-status.connection-failed'));
					break;
				case TestConnectionStatus.UrlIsInvalid:
					set(testSuccess, false);
					set(testMessage, t('components.radarr-integration.connection-status.url-is-invalid'));
					break;
				default:
					set(testSuccess, false);
					set(testMessage, t('components.radarr-integration.connection-status.unknown-connection-status'));
					break;
			}
		} else {
			set(testSuccess, false);
			set(error, response);
		}
		set(isTesting, false);
	}));
}

function configureRadarrSetup() {
	set(isConfiguring, true);
	set(testMessage, '');
	useSubscription(integrationApi.configureRadarrIntegrationEndpoint({
		url: settingsStore.integrationsSettings.radarr.radarrBaseUrl,
		apiKey: settingsStore.integrationsSettings.radarr.radarrApiKey,
	}).pipe(tap(() => settingsStore.refreshSettings())).subscribe((response) => {
		set(configuringSuccess, response.isSuccess);
		set(isConfiguring, false);
		if (response.isSuccess) {
			set(configuringMessage, t('components.radarr-integration.configuration-status.success'));
			set(step, 3);
		} else {
			set(error, response);
		}
	}));
}

onBeforeMount(async () => {
	testRadarrConnection();
	if (settingsStore.integrationsSettings.radarr.isConfigured) {
		set(configuringSuccess, true);
		set(configuringMessage, t('components.radarr-integration.configuration-status.success'));
	}
});
</script>

