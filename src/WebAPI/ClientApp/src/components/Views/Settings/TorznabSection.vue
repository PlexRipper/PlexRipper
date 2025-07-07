<template>
	<QSection :header="$t('pages.settings.advanced.torznab-settings.header')">
		<HelpGroup class="q-mt-md">
			<!--	Enable Torznab API	-->
			<HelpRow
				:label="$t('help.settings.advanced.torznab-settings.enable-torznab.label')"
				:title="$t('help.settings.advanced.torznab-settings.enable-torznab.title')"
				:text="$t('help.settings.advanced.torznab-settings.enable-torznab.text')">
				<q-toggle
					v-model:model-value="settingsStore.torznabSettings.isEnabled"
					size="lg"
					data-cy="toggle-torznab-enabled" />
			</HelpRow>

			<!--	API Key	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.api-key.label')"
				:title="$t('help.settings.advanced.torznab-settings.api-key.title')"
				:text="$t('help.settings.advanced.torznab-settings.api-key.text')">
				<div class="row items-center q-gutter-sm">
					<q-input
						v-model="settingsStore.torznabSettings.apiKey"
						:type="showApiKey ? 'text' : 'password'"
						readonly
						outlined
						dense
						class="col"
						data-cy="torznab-api-key">
						<template #append>
							<q-icon
								:name="showApiKey ? 'visibility_off' : 'visibility'"
								class="cursor-pointer"
								@click="showApiKey = !showApiKey" />
						</template>
					</q-input>
					<q-btn
						icon="refresh"
						color="primary"
						outline
						dense
						@click="generateNewApiKey"
						:loading="generatingKey"
						data-cy="generate-api-key">
						{{ $t('pages.settings.advanced.torznab-settings.generate-key') }}
					</q-btn>
				</div>
			</HelpRow>

			<!--	Torznab URL	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.torznab-url.label')"
				:title="$t('help.settings.advanced.torznab-settings.torznab-url.title')"
				:text="$t('help.settings.advanced.torznab-settings.torznab-url.text')">
				<div class="row items-center q-gutter-sm">
					<q-input
						:model-value="torznabUrl"
						readonly
						outlined
						dense
						class="col"
						data-cy="torznab-url" />
					<q-btn
						icon="content_copy"
						color="primary"
						outline
						dense
						@click="copyToClipboard(torznabUrl)"
						data-cy="copy-torznab-url">
						{{ $t('common.copy') }}
					</q-btn>
				</div>
			</HelpRow>

			<!--	Max Results	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.max-results.label')"
				:title="$t('help.settings.advanced.torznab-settings.max-results.title')"
				:text="$t('help.settings.advanced.torznab-settings.max-results.text')">
				<q-input
					v-model.number="settingsStore.torznabSettings.maxResultsPerRequest"
					type="number"
					:min="1"
					:max="1000"
					outlined
					dense
					data-cy="torznab-max-results" />
			</HelpRow>

			<!--	Search Timeout	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.search-timeout.label')"
				:title="$t('help.settings.advanced.torznab-settings.search-timeout.title')"
				:text="$t('help.settings.advanced.torznab-settings.search-timeout.text')">
				<q-input
					v-model.number="settingsStore.torznabSettings.searchTimeoutSeconds"
					type="number"
					:min="5"
					:max="300"
					suffix="seconds"
					outlined
					dense
					data-cy="torznab-search-timeout" />
			</HelpRow>

			<!--	Auto Create Downloads	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.auto-create-downloads.label')"
				:title="$t('help.settings.advanced.torznab-settings.auto-create-downloads.title')"
				:text="$t('help.settings.advanced.torznab-settings.auto-create-downloads.text')">
				<q-toggle
					v-model:model-value="settingsStore.torznabSettings.autoCreateDownloadTasks"
					size="lg"
					data-cy="toggle-auto-create-downloads" />
			</HelpRow>

			<!--	Webhook Notifications	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.enable-webhooks.label')"
				:title="$t('help.settings.advanced.torznab-settings.enable-webhooks.title')"
				:text="$t('help.settings.advanced.torznab-settings.enable-webhooks.text')">
				<q-toggle
					v-model:model-value="settingsStore.torznabSettings.enableWebhookNotifications"
					size="lg"
					data-cy="toggle-webhook-notifications" />
			</HelpRow>

			<!--	Webhook URL	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled && settingsStore.torznabSettings.enableWebhookNotifications"
				:label="$t('help.settings.advanced.torznab-settings.webhook-url.label')"
				:title="$t('help.settings.advanced.torznab-settings.webhook-url.title')"
				:text="$t('help.settings.advanced.torznab-settings.webhook-url.text')">
				<q-input
					v-model="settingsStore.torznabSettings.webhookUrl"
					placeholder="https://your-webhook-endpoint.com/notify"
					outlined
					dense
					data-cy="webhook-url" />
			</HelpRow>

			<!--	ARR Integration Instructions	-->
			<HelpRow
				v-if="settingsStore.torznabSettings.isEnabled"
				:label="$t('help.settings.advanced.torznab-settings.arr-integration.label')"
				:title="$t('help.settings.advanced.torznab-settings.arr-integration.title')"
				:text="$t('help.settings.advanced.torznab-settings.arr-integration.text')">
				<div class="text-body2 text-grey-7">
					<p class="q-mb-sm">{{ $t('pages.settings.advanced.torznab-settings.arr-setup-instructions') }}</p>
					<ul class="q-pl-md">
						<li><strong>URL:</strong> {{ torznabUrl }}</li>
						<li>
							<strong>API Key:</strong> 
							<span class="q-ml-sm">{{ showApiKeyInInstructions ? settingsStore.torznabSettings.apiKey : '••••••••••••••••' }}</span>
							<q-btn
								:icon="showApiKeyInInstructions ? 'visibility_off' : 'visibility'"
								flat
								dense
								size="sm"
								class="q-ml-xs"
								@click="showApiKeyInInstructions = !showApiKeyInInstructions"
								:title="showApiKeyInInstructions ? 'Hide API key' : 'Show API key'" />
						</li>
						<li><strong>Categories:</strong> Movies (2000), TV (5000)</li>
					</ul>
				</div>
			</HelpRow>
		</HelpGroup>
	</QSection>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useSettingsStore } from '@store';
import { useQuasar } from 'quasar';
import { useI18n } from 'vue-i18n';
import { from } from 'rxjs';
import Axios from 'axios';

const settingsStore = useSettingsStore();
const $q = useQuasar();
const { t } = useI18n();

const showApiKey = ref(false);
const showApiKeyInInstructions = ref(false);
const generatingKey = ref(false);

const torznabUrl = computed(() => {
	const baseUrl = window.location.origin;
	return `${baseUrl}/api/torznab`;
});

const generateNewApiKey = async () => {
	generatingKey.value = true;
	try {
		const response = await from(
			Axios.request<{ apiKey: string }>({
				url: '/api/settings/torznab/generate-key',
				method: 'POST',
				secure: true,
				format: 'json',
			})
		).toPromise();
		
		if (response?.data?.apiKey) {
			settingsStore.torznabSettings.apiKey = response.data.apiKey;
			$q.notify({
				type: 'positive',
				message: t('pages.settings.advanced.torznab-settings.api-key-generated'),
			});
		} else {
			throw new Error('Invalid response from server');
		}
	} catch (error) {
		console.error('Failed to generate API key:', error);
		$q.notify({
			type: 'negative',
			message: t('pages.settings.advanced.torznab-settings.api-key-generation-failed'),
		});
	} finally {
		generatingKey.value = false;
	}
};

const copyToClipboard = async (text: string) => {
	try {
		await navigator.clipboard.writeText(text);
		$q.notify({
			type: 'positive',
			message: t('common.copied-to-clipboard'),
		});
	} catch (error) {
		$q.notify({
			type: 'negative',
			message: t('common.copy-failed'),
		});
	}
};
</script>