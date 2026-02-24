<template>
	<QSection :header="$t('pages.settings.advanced.network.header')">
		<HelpGroup class="q-mt-md">
			<!-- Reverse Proxy Url -->
			<HelpRow
				:label="$t('help.settings.advanced.network-section.reverse-proxy-url.label')"
				:title="$t('help.settings.advanced.network-section.reverse-proxy-url.title')"
				:text="$t('help.settings.advanced.network-section.reverse-proxy-url.text')">
				<QInput
					v-model="settingsStore.networkSettings.reverseProxyUrl"
					:hint="$t('pages.settings.advanced.network.hints.reverse-proxy-url')" />
			</HelpRow>
			<!-- Base Path -->
			<HelpRow
				:label="$t('help.settings.advanced.network-section.base-path.label')"
				:title="$t('help.settings.advanced.network-section.base-path.title')"
				:text="$t('help.settings.advanced.network-section.base-path.text')">
				<QInput
					v-model="settingsStore.networkSettings.basePath"
					:hint="$t('pages.settings.advanced.network.hints.base-path')" />
			</HelpRow>
			<!-- Trust Proxy Headers -->
			<HelpRow
				:label="$t('help.settings.advanced.network-section.trust-proxy-headers.label')"
				:title="$t('help.settings.advanced.network-section.trust-proxy-headers.title')"
				:text="$t('help.settings.advanced.network-section.trust-proxy-headers.text')">
				<q-toggle
					v-model:model-value="settingsStore.networkSettings.trustProxyHeaders"
					size="lg"
					:hint="$t('pages.settings.advanced.network.hints.trust-proxy-headers')" />
			</HelpRow>
			<!-- Allowed Proxy IPs -->
			<HelpRow
				:label="$t('help.settings.advanced.network-section.allowed-proxy-ips.label')"
				:title="$t('help.settings.advanced.network-section.allowed-proxy-ips.title')"
				:text="$t('help.settings.advanced.network-section.allowed-proxy-ips.text')">
				<QInput
					v-model="allowedProxyIps"
					type="textarea"
					autogrow
					:hint="$t('pages.settings.advanced.network.hints.allowed-proxy-ips')" />
			</HelpRow>
			<!-- Forwarded Host Header -->
			<HelpRow
				:label="$t('help.settings.advanced.network-section.forwarded-host-header.label')"
				:title="$t('help.settings.advanced.network-section.forwarded-host-header.title')"
				:text="$t('help.settings.advanced.network-section.forwarded-host-header.text')">
				<QInput
					v-model="settingsStore.networkSettings.forwardedHostHeader"
					:hint="$t('pages.settings.advanced.network.hints.forwarded-host-header')" />
			</HelpRow>
			<!-- Forwarded Path Header -->
			<HelpRow
				:label="$t('help.settings.advanced.network-section.forwarded-path-header.label')"
				:title="$t('help.settings.advanced.network-section.forwarded-path-header.title')"
				:text="$t('help.settings.advanced.network-section.forwarded-path-header.text')">
				<QInput
					v-model="settingsStore.networkSettings.forwardedPathHeader"
					:hint="$t('pages.settings.advanced.network.hints.forwarded-path-header')" />
			</HelpRow>
		</HelpGroup>
	</QSection>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useSettingsStore } from '@store';

const settingsStore = useSettingsStore();

const allowedProxyIps = computed({
	get: () => settingsStore.networkSettings.allowedProxyIps.join(', '),
	set: (value: string) => {
		const entries = value
			.split(/[\n,]+/)
			.map((entry) => entry.trim())
			.filter(Boolean);

		settingsStore.networkSettings.allowedProxyIps = entries;
	},
});
</script>
