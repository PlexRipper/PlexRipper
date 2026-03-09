<template>
	<QSection :header="t('components.sonarr-integration.title')">
		<QAlert
			type="info"
			to="/settings/advanced#reverse-proxy-settings">
			{{ t('components.sonarr-integration.reverse-proxy-alert') }}
		</QAlert>
		<q-stepper
			ref="stepper"
			v-model="integrationStore.sonarr.step"
			alternative-labels
			animated
			color="primary"
			flat
			header-nav>
			<!-- Setup Sonarr Connection -->
			<QStep
				:done="integrationStore.sonarr.testSuccess === true"
				:error="integrationStore.sonarr.testSuccess === false && integrationStore.sonarr.testStatus !== null"
				:name="1"
				:title="t('components.sonarr-integration.nav-bar.connection.title')"
				active-icon="mdi-connection"
				done-color="positive"
				done-icon="mdi-check">
				<!-- Base URL -->
				<HelpRow
					:col-label="3"
					:label="t('help.settings.integrations.sonarr.base-url-input.label')"
					:text="t('help.settings.integrations.sonarr.base-url-input.text')"
					:title="t('help.settings.integrations.sonarr.base-url-input.title')">
					<QInput
						v-model="settingsStore.integrationsSettings.sonarr.sonarrBaseUrl"
						hint="http://localhost:8989"
						data-cy="sonarr-base-url-input" />
				</HelpRow>
				<!-- API Key -->
				<HelpRow
					:col-label="3"
					:label="t('help.settings.integrations.sonarr.api-key-input.label')"
					:text="t('help.settings.integrations.sonarr.api-key-input.text')"
					:title="t('help.settings.integrations.sonarr.api-key-input.title')">
					<ApiKeyInputField
						v-model="settingsStore.integrationsSettings.sonarr.sonarrApiKey"
						v-model:has-focus="passwordInputFocus"
						cy="sonarr-api-key-input"
						hint="a02a22a436504e15b7e46764b12825db"
						show-strength />
				</HelpRow>

				<!-- Test Connection -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.sonarr.test-connection.text')"
					:title="t('help.settings.integrations.sonarr.test-connection.title')"
					hide-label>
					<BaseButton
						:loading="integrationStore.sonarr.isTesting"
						:disabled="!integrationStore.isSonarrConnectionValid"
						icon="mdi-connection"
						label="Test Connection"
						cy="test-sonarr-connection-button"
						@click="testSonarrConnection" />
				</HelpRow>
				<!-- Clear Configuration -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.sonarr.clear-configuration.text')"
					:title="t('help.settings.integrations.sonarr.clear-configuration.title')"
					hide-label>
					<BaseButton
						color="negative"
						icon="mdi-trash-can-outline"
						label="Clear Configuration"
						cy="clear-sonarr-configuration-button"
						@click="clearSonarrConfiguration" />
				</HelpRow>
			</QStep>
			<!-- Configure Sonarr Integration -->
			<QStep
				:disable="integrationStore.sonarr.testSuccess === false"
				:done="settingsStore.integrationsSettings.sonarr.isConfigured"
				:name="2"
				:title="t('components.sonarr-integration.nav-bar.configure.title')"
				active-icon="mdi-cog"
				done-color="positive"
				done-icon="mdi-check"
				icon="mdi-cog">
				<!-- Setup Sonarr Integration -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.sonarr.setup-configuration.text')"
					:title="t('help.settings.integrations.sonarr.setup-configuration.title')"
					hide-label>
					<BaseButton
						:loading="integrationStore.sonarr.isConfiguring"
						icon="mdi-connection"
						:label="t('components.sonarr-integration.nav-bar.configure.button')"
						cy="configure-sonarr-button"
						@click="configureSonarrSetup" />
				</HelpRow>
			</QStep>
		</q-stepper>
		<!-- Test Connection Status -->
		<QRow>
			<QCol>
				<QAlert
					v-if="integrationStore.sonarr.configuringSuccess !== null"
					cy="test-sonarr-configuration-status-alert"
					:type="integrationStore.sonarr.configuringSuccess ? NotificationLevel.Success : NotificationLevel.Error">
					{{ integrationStore.sonarr.configuringSuccess ? t('components.sonarr-integration.configuration-status.success') : formatErrorResponse(integrationStore.sonarr.error) }}
				</QAlert>
				<!-- Always returns 200 -->
				<QAlert
					v-else-if="integrationStore.sonarr.testSuccess !== null"
					:type="integrationStore.sonarr.testSuccess ? NotificationLevel.Success : NotificationLevel.Error"
					cy="test-sonarr-connection-status-alert">
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

function testSonarrConnection() {
	useSubscription(integrationStore.testConnectionToSonarr().subscribe());
}

function clearSonarrConfiguration() {
	useSubscription(integrationStore.clearSonarrConfiguration().subscribe());
}

function configureSonarrSetup() {
	useSubscription(integrationStore.configureSonarrIntegration().subscribe());
}

function getTestStatusMessage(): string {
	const status = integrationStore.sonarr.testStatus;

	if (!status) {
		return integrationStore.sonarr.error
			? formatErrorResponse(integrationStore.sonarr.error)
			: '';
	}

	switch (status) {
		case TestConnectionStatus.Success:
			return t('components.sonarr-integration.connection-status.success');
		case TestConnectionStatus.InvalidApiKey:
			return t('components.sonarr-integration.connection-status.invalid-api-key');
		case TestConnectionStatus.ConnectionFailed:
			return t('components.sonarr-integration.connection-status.connection-failed');
		case TestConnectionStatus.UrlIsInvalid:
			return t('components.sonarr-integration.connection-status.url-is-invalid');
		case TestConnectionStatus.Unknown:
		default:
			return t('components.sonarr-integration.connection-status.unknown-connection-status');
	}
}
</script>
