<template>
	<QSection header="Sonarr">
		<HelpRow
			:col-label="3"
			:label="t('help.settings.integrations.sonarr.base-url-input.label')"
			:title="t('help.settings.integrations.sonarr.base-url-input.title')"
			:text="t('help.settings.integrations.sonarr.base-url-input.text')">
			<QInput
				v-model="settingsStore.integrationsSettings.sonarr.sonarrBaseUrl"
				hint="http://localhost:8989" />
		</HelpRow>

		<HelpRow
			:col-label="3"
			:label="t('help.settings.integrations.sonarr.api-key-input.label')"
			:title="t('help.settings.integrations.sonarr.api-key-input.title')"
			:text="t('help.settings.integrations.sonarr.api-key-input.text')">
			<ApiKeyInputField
				v-model="settingsStore.integrationsSettings.sonarr.sonarrApiKey"
				v-model:has-focus="passwordInputFocus"
				show-strength
				hint="a02a22a436504e15b7e46764b12825db"
				cy="sonarr-api-key-input" />
		</HelpRow>

		<!-- Action Buttons -->
		<QRow gutter="xs">
			<QCol cols="3" />
			<QCol>
				<!-- Test Connection -->
				<BaseButton
					v-if="!testSuccess"
					icon="mdi-connection"
					:loading="isTesting"
					label="Test Connection"
					@click="testSonarrConnection" />
				<!-- Setup Sonarr Integration -->
				<BaseButton
					v-else
					:loading="isConfiguring"
					icon="mdi-connection"
					label="Setup Sonarr Integration"
					@click="configureSonarrSetup" />
			</QCol>
		</QRow>

		<!-- Test Connection Status -->
		<QAlert
			v-if="testMessage"
			:type="testSuccess ? NotificationLevel.Success : NotificationLevel.Error">
			{{ testMessage }}
		</QAlert>
	</QSection>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { set } from '@vueuse/core';
import { useSettingsStore } from '@store';
import { integrationApi } from '@api';
import { NotificationLevel, TestConnectionStatus } from '@dto';

const settingsStore = useSettingsStore();
const { t } = useI18n();

const passwordInputFocus = ref(false);
const isTesting = ref(false);
const isConfiguring = ref(false);
const testSuccess = ref(false);
const testMessage = ref('');

function testSonarrConnection() {
	set(isTesting, true);
	set(testMessage, '');
	useSubscription(integrationApi.testConnectionToSonarrEndpoint({
		url: settingsStore.integrationsSettings.sonarr.sonarrBaseUrl,
		apiKey: settingsStore.integrationsSettings.sonarr.sonarrApiKey,
	}).subscribe(({ isSuccess, value, errors }) => {
		if (isSuccess) {
			switch (value?.result) {
				case TestConnectionStatus.Success:
					set(testSuccess, true);
					set(testMessage, t('components.sonarr-integration.connection-status.success'));
					break;
				case TestConnectionStatus.InvalidApiKey:
					set(testSuccess, false);
					set(testMessage, t('components.sonarr-integration.connection-status.invalid-api-key'));
					break;
				case TestConnectionStatus.ConnectionFailed:
					set(testSuccess, false);
					set(testMessage, t('components.sonarr-integration.connection-status.connection-failed'));
					break;
				case TestConnectionStatus.UrlIsInvalid:
					set(testSuccess, false);
					set(testMessage, t('components.sonarr-integration.connection-status.url-is-invalid'));
					break;
				default:
					set(testSuccess, false);
					set(testMessage, t('components.sonarr-integration.connection-status.unknown-connection-status'));
					break;
			}
		} else {
			set(testSuccess, false);
			set(testMessage, errors.map((x) => x.metadata).join(', '));
		}
		set(isTesting, false);
	}));
}

function configureSonarrSetup() {
	set(isConfiguring, true);
	useSubscription(integrationApi.configureSonarrIntegrationEndpoint({
		url: settingsStore.integrationsSettings.sonarr.sonarrBaseUrl,
		apiKey: settingsStore.integrationsSettings.sonarr.sonarrApiKey,
	}).subscribe(() => {
		set(isConfiguring, false);
	}));
}
</script>
