<template>
	<QCardDialog
		:name="DialogType.IntegrationSetupDialog"
		width="520px"
		persistent
		close-button
		cy="integration-setup-dialog">
		<template #title>
			<div class="row items-center q-gutter-sm">
				<QImg
					:src="integrationLogo"
					:alt="store.draft.type"
					fit="contain"
					width="28px"
					height="28px" />
				<span>{{ `Setting up ${store.draft.type}` }}</span>
			</div>
		</template>
		<template #default>
			<QStepper
				v-model="activeStep"
				vertical
				flat
				animated
				class="integration-setup-stepper">
				<QStep
					:name="1"
					title="Connect"
					icon="mdi-lan-connect"
					:done="steps.connect.status === 'success'"
					:header-nav="false"
					:error="steps.connect.status === 'error'">
					<div
						v-if="steps.connect.error"
						class="text-negative">
						{{ steps.connect.error }}
					</div>
					<div v-else-if="steps.connect.status === 'running'">
						{{ `Connecting to ${store.draft.type}...` }}
					</div>
					<div
						v-else-if="steps.connect.status === 'success'"
						class="text-positive">
						{{ connectedText }}
					</div>
				</QStep>
				<QStep
					:name="2"
					title="Download client"
					icon="mdi-download"
					:done="steps.downloadClient.status === 'success'"
					:header-nav="false"
					:error="steps.downloadClient.status === 'error'">
					<div
						v-if="steps.downloadClient.error"
						class="text-negative">
						{{ steps.downloadClient.error }}
					</div>
					<div v-else-if="steps.downloadClient.status === 'running'">
						{{ downloadClientText }}
					</div>
					<div
						v-else-if="steps.downloadClient.status === 'success'"
						class="text-positive">
						{{ downloadClientReadyText }}
					</div>
				</QStep>
				<QStep
					:name="3"
					title="Indexer"
					icon="mdi-database-search"
					:done="steps.indexer.status === 'success'"
					:header-nav="false"
					:error="steps.indexer.status === 'error'">
					<div
						v-if="steps.indexer.error"
						class="text-negative">
						{{ steps.indexer.error }}
					</div>
					<div v-else-if="steps.indexer.status === 'running'">
						{{ indexerText }}
					</div>
					<div
						v-else-if="steps.indexer.status === 'success'"
						class="text-positive">
						{{ indexerReadyText }}
					</div>
				</QStep>
				<QStep
					:name="4"
					title="Done"
					icon="mdi-check-circle-outline"
					:done="steps.done.status === 'success'"
					:header-nav="false"
					:error="steps.done.status === 'error'">
					<div
						v-if="steps.done.error"
						class="text-negative">
						{{ steps.done.error }}
					</div>
					<div
						v-else-if="steps.done.status === 'success'"
						class="text-positive">
						{{ completeText }}
					</div>
				</QStep>
			</QStepper>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { DialogType } from '@enums';
import { IntegrationSetupProgressStage, IntegrationType, type IntegrationSetupProgressDTO } from '@dto';
import { useIntegrationStore, useSignalrStore } from '@store';
import { useSubscription } from '@vueuse/rxjs';

const store = useIntegrationStore();
const signalrStore = useSignalrStore();
const activeStep = ref(1);
const integrationLogo = computed(() => store.draft.type === IntegrationType.Radarr ? '/img/logo/radarr.png' : '/img/logo/sonarr.png');
const connectedText = 'Connected.';
const downloadClientText = 'Setting up the Reaparr download client...';
const downloadClientReadyText = 'Download client ready.';
const indexerText = 'Setting up the Reaparr indexer...';
const indexerReadyText = 'Indexer ready.';
const completeText = 'Integration setup complete.';
type StepStatus = 'pending' | 'running' | 'success' | 'error';
type Step = { status: StepStatus; error: string };
const steps = reactive<{ connect: Step; downloadClient: Step; indexer: Step; done: Step }>({
	connect: { status: 'pending', error: '' },
	downloadClient: { status: 'pending', error: '' },
	indexer: { status: 'pending', error: '' },
	done: { status: 'pending', error: '' },
});

function reset(): void {
	activeStep.value = 1;
	for (const step of Object.values(steps)) {
		step.status = 'pending';
		step.error = '';
	}
}

function updateStep(progress: IntegrationSetupProgressDTO): void {
	const step = progress.stage === IntegrationSetupProgressStage.Connecting
		? steps.connect
		: progress.stage === IntegrationSetupProgressStage.DownloadClient
			? steps.downloadClient
			: progress.stage === IntegrationSetupProgressStage.Indexer ? steps.indexer : steps.done;
	step.status = progress.isRunning ? 'running' : progress.isSuccess ? 'success' : 'error';
	step.error = progress.error ?? '';
	activeStep.value = progress.stage === IntegrationSetupProgressStage.Connecting ? 1 : progress.stage === IntegrationSetupProgressStage.DownloadClient ? 2 : progress.stage === IntegrationSetupProgressStage.Indexer ? 3 : 4;
}

useSubscription(signalrStore.integrationSetupProgressSubject.subscribe((progress) => {
	if (progress.integrationId === store.detail?.id) updateStep(progress);
}));

onMounted(reset);
watch(() => store.isSettingUp, (isSettingUp) => {
	if (isSettingUp) reset();
});
</script>

<style lang="scss">
.integration-setup-stepper {
	background: transparent;
}
</style>
