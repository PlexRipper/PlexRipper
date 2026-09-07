<template>
	<OverviewCard
		mode="edit"
		class="integration-card"
		cy="integration-card"
		:aria-label="integration.name"
		:icon="integrationIcon"
		:title="integration.name"
		:subtitle="integration.baseUrl"
		:chips="[{ color: stateColor, value: integration.provisioningState }]"
		@click="open" />
</template>

<script setup lang="ts">
import type { NamedColor } from 'quasar';
import { IntegrationProvisioningState, IntegrationType, type IntegrationSummary } from '@dto';

const props = defineProps<{ integration: IntegrationSummary }>();
const emit = defineEmits<{
	(e: 'edit', integration: IntegrationSummary): void;
}>();

function open(): void {
	emit('edit', props.integration);
}

const stateColor = computed((): NamedColor => {
	if (props.integration.provisioningState === IntegrationProvisioningState.Configured) return 'positive';
	if (props.integration.provisioningState === IntegrationProvisioningState.ChangesPending) return 'warning';
	return 'negative';
});

const integrationIcon = computed(() => props.integration.type === IntegrationType.Radarr ? 'radarr' : 'sonarr');
</script>
