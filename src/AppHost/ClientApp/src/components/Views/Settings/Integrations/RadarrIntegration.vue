<template>
	<QSection header="Radarr">
		<q-stepper
			ref="stepper"
			v-model="integrationStore.radarr.step"
			alternative-labels
			animated
			color="primary"
			flat
			header-nav>
			<!-- Setup Radarr Connection -->
			<QStep
				:done="integrationStore.radarr.testSuccess === true"
				:error="integrationStore.radarr.testSuccess === false && integrationStore.radarr.testStatus !== null"
				:name="1"
				:title="t('components.radarr-integration.nav-bar.connection.title')"
				active-icon="mdi-connection"
				done-color="positive"
				done-icon="mdi-check">
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
						:loading="integrationStore.radarr.isTesting"
						icon="mdi-connection"
						label="Test Connection"
						@click="testRadarrConnection" />
				</HelpRow>
			</QStep>
			<!-- Configure Radarr Integration -->
			<QStep
				:disable="integrationStore.radarr.testSuccess === false"
				:done="settingsStore.integrationsSettings.radarr.isConfigured"
				:name="2"
				:title="t('components.radarr-integration.nav-bar.configure.title')"
				active-icon="mdi-cog"
				done-color="positive"
				done-icon="mdi-check"
				icon="mdi-cog">
				<!-- Setup Radarr Integration -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.radarr.setup-configuration.text')"
					:title="t('help.settings.integrations.radarr.setup-configuration.title')"
					hide-label>
					<BaseButton
						:loading="integrationStore.radarr.isConfiguring"
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
					v-if="integrationStore.radarr.configuringSuccess !== null"
					:type="integrationStore.radarr.configuringSuccess ? NotificationLevel.Success : NotificationLevel.Error">
					{{ integrationStore.radarr.configuringSuccess ? t('components.radarr-integration.configuration-status.success') : formatErrorResponse(integrationStore.radarr.error) }}
				</QAlert>
				<!-- Always returns 200 -->
				<QAlert
					v-else-if="integrationStore.radarr.testSuccess !== null"
					:type="integrationStore.radarr.testSuccess ? NotificationLevel.Success : NotificationLevel.Error">
					{{ getTestStatusMessage() }}
				</QAlert>
			</QCol>
		</QRow>
	</QSection>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import { useSettingsStore, useIntegrationStore } from '@store';
import { NotificationLevel, TestConnectionStatus } from '@dto';
import { formatErrorResponse } from '@composables';

const settingsStore = useSettingsStore();
const integrationStore = useIntegrationStore();
const { t } = useI18n();

const passwordInputFocus = ref(false);

function testRadarrConnection() {
	integrationStore.testConnection(
		'radarr',
		settingsStore.integrationsSettings.radarr.radarrBaseUrl,
		settingsStore.integrationsSettings.radarr.radarrApiKey,
	);
}

function configureRadarrSetup() {
	integrationStore.configureIntegration(
		'radarr',
		settingsStore.integrationsSettings.radarr.radarrBaseUrl,
		settingsStore.integrationsSettings.radarr.radarrApiKey,
	);
}

function getTestStatusMessage(): string {
	const status = integrationStore.radarr.testStatus;

	if (!status) {
		return integrationStore.radarr.error
			? formatErrorResponse(integrationStore.radarr.error)
			: '';
	}

	switch (status) {
		case TestConnectionStatus.Success:
			return t('components.radarr-integration.connection-status.success');
		case TestConnectionStatus.InvalidApiKey:
			return t('components.radarr-integration.connection-status.invalid-api-key');
		case TestConnectionStatus.ConnectionFailed:
			return t('components.radarr-integration.connection-status.connection-failed');
		case TestConnectionStatus.UrlIsInvalid:
			return t('components.radarr-integration.connection-status.url-is-invalid');
		case TestConnectionStatus.Unknown:
		default:
			return t('components.radarr-integration.connection-status.unknown-connection-status');
	}
}
</script>
