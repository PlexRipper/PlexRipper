<template>
	<QSection header="Integrations">
		<QRow justify="center">
			<QCol
				v-for="integration in items"
				:key="integration.id"
				class="q-pa-sm"
				cols="12"
				sm="6"
				md="3">
				<IntegrationCard
					:integration="integration"
					@edit="openEdit" />
			</QCol>
			<QCol
				class="q-pa-sm"
				cols="12"
				sm="6"
				md="3">
				<QCard
					flat
					bordered
					class="integration-add-card flex flex-center"
					data-cy="add-integration"
					role="button"
					tabindex="0"
					:aria-label="$t('help.settings.integrations.choose-title')"
					@click="openAdd"
					@keydown.enter="openAdd"
					@keydown.space.prevent="openAdd">
					<QIcon
						name="mdi-plus-box-outline"
						style="font-size: 90px" />
				</QCard>
			</QCol>
		</QRow>
		<IntegrationDialog />
	</QSection>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import type { IntegrationSummary } from '@dto';
import { useDialogStore, useIntegrationStore } from '@store';
import { storeToRefs } from 'pinia';

const dialogStore = useDialogStore();
const integrationStore = useIntegrationStore();
const { items } = storeToRefs(integrationStore);

function refresh(): void {
	useSubscription(integrationStore.refresh().subscribe());
}

function openAdd(): void {
	dialogStore.openIntegrationDialog(null);
}

function openEdit(integration: IntegrationSummary): void {
	dialogStore.openIntegrationDialog(integration);
}

onMounted(refresh);
</script>

<style lang="scss">
.integration-add-card {
  border: 2px solid red;
  max-height: 124px;
  min-height: 124px;
  transition: box-shadow 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);

  &:hover {
    box-shadow: 0 0 20px 3px red;
    cursor: pointer;
  }

  &:focus-visible {
    outline: 2px solid white;
    outline-offset: 3px;
  }
}
</style>
