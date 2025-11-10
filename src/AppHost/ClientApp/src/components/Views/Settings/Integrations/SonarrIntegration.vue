<template>
	<QSection header="Sonarr">
		<q-stepper
			ref="stepper"
			v-model="step"
			alternative-labels
			animated
			color="primary"
			flat
			header-nav>
			<!-- Setup Sonarr Connection -->
			<QStep
				:done="testSuccess"
				:error="!testSuccess && testMessage !== ''"
				:name="1"
				done-color="positive"
				done-icon="mdi-check"
				icon="mdi-connection"
				:title="t('components.sonarr-integration.nav-bar.connection.title')">
				<!-- Base URL -->
				<HelpRow
					:col-label="3"
					:label="t('help.settings.integrations.sonarr.base-url-input.label')"
					:text="t('help.settings.integrations.sonarr.base-url-input.text')"
					:title="t('help.settings.integrations.sonarr.base-url-input.title')">
					<QInput
						v-model="settingsStore.integrationsSettings.sonarr.sonarrBaseUrl"
						hint="http://localhost:8989" />
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
					:text="t('help.settings.integrations.sonarr.test-connection.title')"
					:title="t('help.settings.integrations.sonarr.test-connection.text')"
					hide-label>
					<BaseButton
						:loading="isTesting"
						icon="mdi-connection"
						label="Test Connection"
						@click="testSonarrConnection" />
				</HelpRow>
			</QStep>
			<!-- Configure Sonarr Integration -->
			<QStep
				:done="settingsStore.integrationsSettings.sonarr.isConfigured"
				:name="2"
				done-color="positive"
				done-icon="mdi-check"
				icon="mdi-cog"
				:title="t('components.sonarr-integration.nav-bar.configure.title')">
				<!-- Setup Sonarr Integration -->
				<HelpRow
					:col-label="3"
					:text="t('help.settings.integrations.sonarr.setup-configuration.text')"
					:title="t('help.settings.integrations.sonarr.setup-configuration.title')"
					hide-label>
					<BaseButton
						:loading="isConfiguring"
						icon="mdi-connection"
						:label="t('components.sonarr-integration.nav-bar.configure.button')"
						@click="configureSonarrSetup" />
				</HelpRow>
			</QStep>
		</q-stepper>
		<!-- Test Connection Status -->
		<QRow>
			<QCol>
				<QAlert
					v-if="configuringMessage"
					:type="configuringSuccess ? NotificationLevel.Success : NotificationLevel.Error">
					{{ configuringMessage }}
				</QAlert>
				<QAlert
					v-else-if="testMessage"
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
import { NotificationLevel, TestConnectionStatus } from '@dto';
import { tap } from 'rxjs/operators';

const settingsStore = useSettingsStore();
const { t } = useI18n();

const step = ref(3);
const passwordInputFocus = ref(false);
const isTesting = ref(false);
const isConfiguring = ref(false);
const testSuccess = ref(false);
const testMessage = ref('');
const configuringSuccess = ref(false);
const configuringMessage = ref('');

function testSonarrConnection() {
	if (settingsStore.integrationsSettings.sonarr.sonarrBaseUrl === '' || settingsStore.integrationsSettings.sonarr.sonarrApiKey === '') {
		return;
	}

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
					set(step, settingsStore.integrationsSettings.sonarr.isConfigured ? 3 : 2);
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
	set(testMessage, '');
	useSubscription(integrationApi.configureSonarrIntegrationEndpoint({
		url: settingsStore.integrationsSettings.sonarr.sonarrBaseUrl,
		apiKey: settingsStore.integrationsSettings.sonarr.sonarrApiKey,
	}).pipe(tap(() => settingsStore.refreshSettings())).subscribe(({ isSuccess, errors }) => {
		set(configuringSuccess, isSuccess);
		set(isConfiguring, false);
		if (isSuccess) {
			set(configuringMessage, t('components.sonarr-integration.configuration-status.success'));
			set(step, 3);
		} else {
			set(configuringMessage, errors.map((x) => x.metadata).join(', '));
		}
	}));
}

onBeforeMount(async () => {
	testSonarrConnection();
	if (settingsStore.integrationsSettings.sonarr.isConfigured) {
		set(configuringSuccess, true);
		set(configuringMessage, t('components.sonarr-integration.configuration-status.success'));
	}
});
</script>
