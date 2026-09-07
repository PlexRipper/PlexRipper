<template>
	<QCard
		class="integration-card cursor-pointer"
		data-cy="integration-card"
		role="button"
		tabindex="0"
		:aria-label="integration.name"
		@click="open"
		@keydown.enter="open"
		@keydown.space.prevent="open">
		<QCardSection class="q-pa-sm">
			<div class="integration-card__identity row items-center no-wrap q-gutter-sm">
				<QImg
					:src="integrationLogo"
					:alt="integration.type"
					fit="contain"
					width="48px"
					height="48px"
					class="integration-card__logo" />
				<div class="integration-card__details">
					<div class="integration-card__name text-h6 ellipsis">
						{{ integration.name }}
					</div>
					<div class="integration-card__meta text-caption">
						{{ integration.baseUrl }}
					</div>
				</div>
			</div>
			<div class="integration-card__status row q-gutter-sm q-mt-sm">
				<QGlowChip
					:color="stateColor"
					:value="integration.provisioningState" />
			</div>
		</QCardSection>
	</QCard>
</template>

<script setup lang="ts">
import { IntegrationProvisioningState, IntegrationType, type IntegrationSummary } from '@dto';

const props = defineProps<{ integration: IntegrationSummary }>();
const emit = defineEmits<{
	(e: 'edit', integration: IntegrationSummary): void;
}>();

function open(): void {
	emit('edit', props.integration);
}

const stateColor = computed(() => {
	if (props.integration.provisioningState === IntegrationProvisioningState.Configured) return 'positive';
	if (props.integration.provisioningState === IntegrationProvisioningState.ChangesPending) return 'warning';
	return 'negative';
});

const integrationLogo = computed(() => props.integration.type === IntegrationType.Radarr ? '/img/logo/radarr.png' : '/img/logo/sonarr.png');
</script>

<style lang="scss">
.integration-card {
	border: 2px solid red;
	max-height: 124px;
	min-height: 124px;

	&:hover {
		box-shadow: 0 0 20px 3px red;
		cursor: pointer;
		transition: box-shadow 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
	}

	&__identity {
		min-width: 0;
	}

	&__logo {
		flex: 0 0 auto;
		padding: 4px;
		border-radius: 8px;
		background: rgb(255 255 255 / 8%);
	}

	&__name {
		min-width: 0;
	}

	&__details {
		min-width: 0;
	}

	&__meta {
		max-width: 100%;
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	&__status {
		min-width: 0;
	}
}
</style>
