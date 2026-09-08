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
				<span>{{ setupTexts.title }}</span>
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
					:title="setupTexts.steps.connect"
					icon="mdi-lan-connect"
					done-icon="mdi-check-circle"
					active-icon="mdi-loading"
					error-icon="mdi-alert-circle"
					done-color="positive"
					error-color="negative"
					:done="steps.connect.status === 'success'"
					:header-nav="false"
					:error="steps.connect.status === 'error'"
					:data-status="steps.connect.status"
					data-cy="integration-setup-step-connect">
					<div
						v-if="steps.connect.error"
						class="text-negative">
						{{ steps.connect.error }}
					</div>
					<div v-else-if="steps.connect.status === 'running'">
						{{ setupTexts.progress.connecting }}
					</div>
					<div
						v-else-if="steps.connect.status === 'success'"
						class="text-positive">
						{{ setupTexts.progress.connected }}
					</div>
				</QStep>
				<QStep
					:name="2"
					:title="setupTexts.steps.downloadClient"
					icon="mdi-download"
					done-icon="mdi-check-circle"
					active-icon="mdi-loading"
					error-icon="mdi-alert-circle"
					done-color="positive"
					error-color="negative"
					:done="steps.downloadClient.status === 'success'"
					:header-nav="false"
					:error="steps.downloadClient.status === 'error'"
					:data-status="steps.downloadClient.status"
					data-cy="integration-setup-step-download-client">
					<div
						v-if="steps.downloadClient.error"
						class="text-negative">
						{{ steps.downloadClient.error }}
					</div>
					<div v-else-if="steps.downloadClient.status === 'running'">
						{{ setupTexts.progress.downloadClient }}
					</div>
					<div
						v-else-if="steps.downloadClient.status === 'success'"
						class="text-positive">
						{{ setupTexts.progress.downloadClientReady }}
					</div>
				</QStep>
				<QStep
					:name="3"
					:title="setupTexts.steps.indexer"
					icon="mdi-database-search"
					done-icon="mdi-check-circle"
					active-icon="mdi-loading"
					error-icon="mdi-alert-circle"
					done-color="positive"
					error-color="negative"
					:done="steps.indexer.status === 'success'"
					:header-nav="false"
					:error="steps.indexer.status === 'error'"
					:data-status="steps.indexer.status"
					data-cy="integration-setup-step-indexer">
					<div
						v-if="steps.indexer.error"
						class="text-negative">
						{{ steps.indexer.error }}
					</div>
					<div v-else-if="steps.indexer.status === 'running'">
						{{ setupTexts.progress.indexer }}
					</div>
					<div
						v-else-if="steps.indexer.status === 'success'"
						class="text-positive">
						{{ setupTexts.progress.indexerReady }}
					</div>
				</QStep>
				<QStep
					:name="4"
					:title="setupTexts.steps.validation"
					icon="mdi-connection"
					done-icon="mdi-check-circle"
					:active-icon="steps.validation.status === 'success' ? 'mdi-check-circle' : 'mdi-loading'"
					error-icon="mdi-alert-circle"
					done-color="positive"
					:active-color="steps.validation.status === 'success' ? 'positive' : 'primary'"
					error-color="negative"
					:done="steps.validation.status === 'success'"
					:header-nav="false"
					:error="steps.validation.status === 'error'"
					:data-status="steps.validation.status"
					data-cy="integration-setup-step-validation">
					<div
						v-if="steps.validation.error"
						class="text-negative">
						{{ steps.validation.error }}
					</div>
					<div v-else-if="steps.validation.status === 'running'">
						{{ setupTexts.progress.validation }}
					</div>
					<div
						v-else-if="steps.validation.status === 'success'"
						class="text-positive">
						{{ setupTexts.progress.validationReady }}
					</div>
				</QStep>
				<QStep
					:name="5"
					:title="setupTexts.steps.done"
					icon="mdi-check-circle-outline"
					done-icon="mdi-check-circle"
					active-icon="mdi-check-circle"
					done-color="positive"
					:done="steps.done.status === 'success'"
					:header-nav="false"
					:data-status="steps.done.status"
					data-cy="integration-setup-step-done">
					<div
						v-if="steps.done.status === 'success'"
						class="text-positive">
						{{ setupTexts.progress.complete }}
					</div>
				</QStep>
			</QStepper>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import Log from 'consola';
import { set } from '@vueuse/core';
import { DialogType } from '@enums';
import { IntegrationSetupProgressStage, IntegrationType, type IntegrationSetupProgressDTO } from '@dto';
import { useIntegrationStore, useSignalrStore } from '@store';
import { useSubscription } from '@vueuse/rxjs';

const store = useIntegrationStore();
const signalrStore = useSignalrStore();
const activeStep = ref(1);
const integrationLogo = computed(() => store.draft.type === IntegrationType.Radarr ? '/img/logo/radarr.png' : '/img/logo/sonarr.png');
const setupTexts = computed(() => {
	const values = { type: store.draft.type };
	return {
		title: $t('components.integration-setup-dialog.setup.title', values),
		steps: {
			connect: $t('components.integration-setup-dialog.setup.steps.connect', values),
			downloadClient: $t('components.integration-setup-dialog.setup.steps.download-client', values),
			indexer: $t('components.integration-setup-dialog.setup.steps.indexer', values),
			validation: $t('components.integration-setup-dialog.setup.steps.validation', values),
			done: $t('components.integration-setup-dialog.setup.steps.done', values),
		},
		progress: {
			connecting: $t('components.integration-setup-dialog.setup.progress.connecting', values),
			connected: $t('components.integration-setup-dialog.setup.progress.connected', values),
			downloadClient: $t('components.integration-setup-dialog.setup.progress.download-client', values),
			downloadClientReady: $t('components.integration-setup-dialog.setup.progress.download-client-ready', values),
			indexer: $t('components.integration-setup-dialog.setup.progress.indexer', values),
			indexerReady: $t('components.integration-setup-dialog.setup.progress.indexer-ready', values),
			validation: $t('components.integration-setup-dialog.setup.progress.validation', values),
			validationReady: $t('components.integration-setup-dialog.setup.progress.validation-ready', values),
			complete: $t('components.integration-setup-dialog.setup.progress.complete', values),
		},
	};
});
type StepStatus = 'pending' | 'running' | 'success' | 'error';
type Step = { status: StepStatus; error: string };
const steps = reactive<{ connect: Step; downloadClient: Step; indexer: Step; validation: Step; done: Step }>({
	connect: { status: 'pending', error: '' },
	downloadClient: { status: 'pending', error: '' },
	indexer: { status: 'pending', error: '' },
	validation: { status: 'pending', error: '' },
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
	const setupStage = {
		[IntegrationSetupProgressStage.Connecting]: { step: steps.connect, number: 1 },
		[IntegrationSetupProgressStage.DownloadClient]: { step: steps.downloadClient, number: 2 },
		[IntegrationSetupProgressStage.Indexer]: { step: steps.indexer, number: 3 },
		[IntegrationSetupProgressStage.Validation]: { step: steps.validation, number: 4 },
		[IntegrationSetupProgressStage.Done]: { step: steps.done, number: 5 },
	}[progress.stage];
	if (!setupStage) {
		Log.warn('Unknown integration setup progress stage', progress.stage);
		return;
	}

	const isDone = progress.stage === IntegrationSetupProgressStage.Done;
	setupStage.step.status = isDone ? 'success' : progress.isRunning ? 'running' : progress.isSuccess ? 'success' : 'error';
	setupStage.step.error = isDone ? '' : progress.error ?? '';
	set(activeStep, setupStage.number);
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

  .mdi-loading {
    animation: integration-setup-loading 1s linear infinite;
  }
}

@keyframes integration-setup-loading {
  from {
    transform: rotate(0deg);
  }

  to {
    transform: rotate(360deg);
  }
}

@media (prefers-reduced-motion: reduce) {
  .integration-setup-stepper .mdi-loading {
    animation: none;
  }
}
</style>
