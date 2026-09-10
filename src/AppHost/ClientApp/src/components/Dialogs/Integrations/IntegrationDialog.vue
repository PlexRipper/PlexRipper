<template>
	<QCardDialog
		:name="DialogType.IntegrationDialog"
		width="960px"
		persistent
		close-button
		cy="integration-dialog"
		@opened="open"
		@closed="close">
		<template #title>
			<div
				v-if="store.detail || stage === 2"
				class="row items-center q-gutter-sm">
				<QImg
					no-spinner
					:src="integrationLogo"
					:alt="store.draft.type"
					fit="contain"
					width="32px"
					height="32px" />
				<span>{{ integrationTitle }}</span>
			</div>
			<span v-else>{{ $t('help.settings.integrations.choose-title') }}</span>
		</template>
		<template #default>
			<div
				v-if="!store.detail && stage === 1"
				class="row q-col-gutter-md q-pa-sm">
				<div
					v-for="integrationType in integrationTypes"
					:key="integrationType.type"
					class="col-12 col-sm-6">
					<QCard
						flat
						bordered
						class="q-ma-md red-glow-hover"
						:data-cy="`integration-type-${integrationType.type.toLowerCase()}`"
						@click="selectType(integrationType.type)">
						<QCardSection class="text-center">
							<QImg
								no-spinner
								:src="integrationType.icon"
								:alt="integrationType.type"
								fit="contain"
								width="64px"
								height="64px" />
							<div class="text-h6 q-mt-sm">
								{{ integrationType.type }}
							</div>
						</QCardSection>
					</QCard>
				</div>
			</div>
			<QForm
				v-else
				class="q-gutter-md"
				@submit="save">
				<QAlert
					v-if="store.error"
					type="error">
					{{ formatError(store.error) }}
				</QAlert>
				<QAlert
					v-if="store.testResult"
					:type="store.testResult.result === TestConnectionStatus.Success ? 'success' : 'error'">
					<div>
						{{
							store.testResult.result === TestConnectionStatus.Success ? $t('help.settings.integrations.test-success') : $t('help.settings.integrations.test-failed')
						}}
					</div>
					<div class="text-caption">
						{{ formatTestDetails() }}
					</div>
				</QAlert>
				<!-- Display Name -->
				<HelpRow
					:label="integrationHelp.displayName.label"
					:title="integrationHelp.displayName.title"
					:text="integrationHelp.displayName.text"
					align="start"
					disable-responsive>
					<QInput
						v-model="store.draft.name"
						data-cy="integration-name"
						:hint="integrationHelp.displayName.hint"
						hide-bottom-space
						:rules="requiredRules" />
				</HelpRow>
				<!-- Base URL -->
				<HelpRow
					:label="integrationHelp.baseUrl.label"
					:title="integrationHelp.baseUrl.title"
					:text="integrationHelp.baseUrl.text"
					align="start"
					disable-responsive>
					<QInput
						v-model="store.draft.url"
						:hint="integrationHelp.baseUrl.hint"
						data-cy="integration-base-url"
						hide-bottom-space
						:rules="requiredRules" />
				</HelpRow>
				<!-- API Key -->
				<HelpRow
					:label="integrationHelp.apiKey.label"
					:title="integrationHelp.apiKey.title"
					:text="integrationHelp.apiKey.text"
					align="start"
					disable-responsive>
					<ApiKeyInputField
						v-model="store.draft.apiKey"
						cy="integration-arr-key"
						:hint="integrationHelp.apiKey.hint"
						:rules="requiredRules" />
				</HelpRow>
				<!-- Category -->
				<HelpRow
					:label="integrationHelp.category.label"
					:title="integrationHelp.category.title"
					:text="integrationHelp.category.text"
					align="start"
					disable-responsive>
					<QInput
						v-model="store.draft.category"
						data-cy="integration-category"
						:hint="integrationHelp.category.hint"
						hide-bottom-space
						:rules="requiredRules" />
				</HelpRow>
				<!-- Download Folder -->
				<HelpRow
					:label="integrationHelp.downloadFolder.label"
					:title="integrationHelp.downloadFolder.title"
					:text="integrationHelp.downloadFolder.text"
					align="start"
					disable-responsive>
					<QSelect
						v-model="store.draft.downloadFolderId"
						:options="downloadFolders"
						option-label="displayName"
						option-value="id"
						emit-value
						map-options
						:dense="false"
						data-cy="integration-download-folder"
						:rules="requiredRules">
						<template #option="scope">
							<QItem v-bind="scope.itemProps">
								<QItemSection>
									<QItemLabel>
										{{ scope.opt.displayName }}
									</QItemLabel>
									<QItemLabel caption>
										{{ scope.opt.directory }}
									</QItemLabel>
								</QItemSection>
							</QItem>
						</template>
					</QSelect>
				</HelpRow>
			</QForm>
		</template>
		<!-- Actions -->
		<template #actions>
			<QRow
				v-if="store.detail || stage === 2"
				gutter="md">
				<QCol v-if="store.detail">
					<DeleteButton
						block
						cy="integration-delete"
						@click="dialogStore.openDialog(DialogType.IntegrationDeleteConfirmationDialog)" />
				</QCol>
				<QCol>
					<BaseButton
						block
						icon="mdi-cloud-search-outline"
						:label="$t('general.commands.check-connection')"
						:loading="store.isTesting"
						:disable="!store.isDraftValid"
						data-cy="integration-test"
						@click="test" />
				</QCol>
				<QCol>
					<BaseButton
						label="Setup"
						block
						icon="mdi-cog-sync"
						:disable="!store.detail"
						:loading="store.isSettingUp"
						data-cy="integration-setup"
						@click="setup" />
				</QCol>
				<QCol>
					<SaveButton
						block
						label="Save"
						:loading="store.isSaving"
						:disable="!store.isDraftValid"
						data-cy="integration-save"
						@click="save" />
				</QCol>
			</QRow>
		</template>
	</QCardDialog>

	<ConfirmationDialog
		:name="DialogType.IntegrationDeleteConfirmationDialog"
		:title="$t('confirmation.delete-integration.title', { name: store.detail?.name ?? '' })"
		:text="$t('confirmation.delete-integration.text')"
		:warning="$t('confirmation.delete-integration.warning', { type: store.detail?.type ?? '' })"
		:confirm-label="$t('general.commands.delete')"
		:confirm-loading="store.isDeleting"
		@confirm="deleteIntegration" />
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { FolderType, IntegrationType, TestConnectionStatus, type IntegrationSummary } from '@dto';
import { DialogType } from '@enums';
import { useDialogStore, useFolderPathStore, useIntegrationStore } from '@store';

const store = useIntegrationStore();
const dialogStore = useDialogStore();
const folderPathStore = useFolderPathStore();
const integrationTypes = [
	{ type: IntegrationType.Sonarr, icon: '/img/logo/sonarr.svg' },
	{ type: IntegrationType.Radarr, icon: '/img/logo/radarr.svg' },
] as const;
const stage = ref(1);
const requiredRules = [(value: string | number) => (typeof value === 'number' ? value > 0 : Boolean(value?.trim())) || 'Required'];
const downloadFolders = computed(() => folderPathStore.folderPaths.filter((folderPath) => folderPath.folderType === FolderType.DownloadFolder));
const integrationLogo = computed(() => integrationTypes.find(({ type }) => type === store.draft.type)?.icon ?? '');

const integrationTitle = computed(() => {
	if (store.draft.type === IntegrationType.Radarr) {
		return store.detail ? $t('help.settings.integrations.radarr.edit-title') : $t('help.settings.integrations.radarr.add-title');
	}

	return store.detail ? $t('help.settings.integrations.sonarr.edit-title') : $t('help.settings.integrations.sonarr.add-title');
});
const integrationHelp = computed(() => {
	if (store.draft.type === IntegrationType.Radarr) {
		return {
			displayName: {
				label: $t('help.settings.integrations.radarr.display-name.label'),
				title: $t('help.settings.integrations.radarr.display-name.title'),
				text: $t('help.settings.integrations.radarr.display-name.text'),
				hint: $t('help.settings.integrations.radarr.display-name.hint'),
			},
			baseUrl: {
				label: $t('help.settings.integrations.radarr.base-url-input.label'),
				title: $t('help.settings.integrations.radarr.base-url-input.title'),
				text: $t('help.settings.integrations.radarr.base-url-input.text'),
				hint: $t('help.settings.integrations.radarr.base-url-input.hint'),
			},
			apiKey: {
				label: $t('help.settings.integrations.radarr.api-key-input.label'),
				title: $t('help.settings.integrations.radarr.api-key-input.title'),
				text: $t('help.settings.integrations.radarr.api-key-input.text'),
				hint: $t('help.settings.integrations.radarr.api-key-input.hint'),
			},
			category: {
				label: $t('help.settings.integrations.radarr.category.label'),
				title: $t('help.settings.integrations.radarr.category.title'),
				text: $t('help.settings.integrations.radarr.category.text'),
				hint: $t('help.settings.integrations.radarr.category.hint'),
			},
			downloadFolder: {
				label: $t('help.settings.integrations.radarr.download-folder.label'),
				title: $t('help.settings.integrations.radarr.download-folder.title'),
				text: $t('help.settings.integrations.radarr.download-folder.text'),
			},
		};
	}

	return {
		displayName: {
			label: $t('help.settings.integrations.sonarr.display-name.label'),
			title: $t('help.settings.integrations.sonarr.display-name.title'),
			text: $t('help.settings.integrations.sonarr.display-name.text'),
			hint: $t('help.settings.integrations.sonarr.display-name.hint'),
		},
		baseUrl: {
			label: $t('help.settings.integrations.sonarr.base-url-input.label'),
			title: $t('help.settings.integrations.sonarr.base-url-input.title'),
			text: $t('help.settings.integrations.sonarr.base-url-input.text'),
			hint: $t('help.settings.integrations.sonarr.base-url-input.hint'),
		},
		apiKey: {
			label: $t('help.settings.integrations.sonarr.api-key-input.label'),
			title: $t('help.settings.integrations.sonarr.api-key-input.title'),
			text: $t('help.settings.integrations.sonarr.api-key-input.text'),
			hint: $t('help.settings.integrations.sonarr.api-key-input.hint'),
		},
		category: {
			label: $t('help.settings.integrations.sonarr.category.label'),
			title: $t('help.settings.integrations.sonarr.category.title'),
			text: $t('help.settings.integrations.sonarr.category.text'),
			hint: $t('help.settings.integrations.sonarr.category.hint'),
		},
		downloadFolder: {
			label: $t('help.settings.integrations.sonarr.download-folder.label'),
			title: $t('help.settings.integrations.sonarr.download-folder.title'),
			text: $t('help.settings.integrations.sonarr.download-folder.text'),
		},
	};
});

watch(
	() => [store.draft.url, store.draft.apiKey],
	() => {
		if (!store.isTesting) store.testResult = null;
	},
);

function open(event: unknown): void {
	const integration = event as IntegrationSummary | null;
	set(stage, integration ? 2 : 1);
	if (integration) useSubscription(store.openEdit(integration).subscribe());
	else {
		store.openAdd();
	}
}

function deleteIntegration() {
	useSubscription(store.delete().subscribe(() => {
		dialogStore.closeDialog(DialogType.IntegrationDeleteConfirmationDialog);
		if (!store.detail) dialogStore.closeDialog(DialogType.IntegrationDialog);
	}));
}

function selectType(type: IntegrationType): void {
	store.openAdd(type);
	set(stage, 2);
}

function formatError(error: unknown): string {
	return typeof error === 'string' ? error : $t('help.settings.integrations.test-failed');
}

function formatTestDetails(): string {
	if (!store.testResult) return '';

	return [
		store.testResult.result !== TestConnectionStatus.Success ? $t('help.settings.integrations.test-url', { url: store.draft.url }) : null,
		store.testResult.httpStatusCode ? `HTTP ${store.testResult.httpStatusCode}` : null,
		store.testResult.errorMessage,
		new Date(store.testResult.testedAt).toLocaleString(),
	].filter(Boolean).join(' · ');
}

function test(): void {
	useSubscription(store.test().subscribe((result) => {
		const isSuccessful = result?.isSuccess && result.value?.result === TestConnectionStatus.Success;
		if (!isSuccessful) return;

		if (!store.detail) save();
	}));
}

function save(): void {
	const isEditing = Boolean(store.detail);
	useSubscription(store.save().subscribe((result) => {
		if (isEditing && result?.isSuccess && result.value) dialogStore.closeDialog(DialogType.IntegrationDialog);
	}));
}

function setup(): void {
	if (!store.detail) return;
	dialogStore.openIntegrationSetupDialog();
	useSubscription(store.setupIntegration().subscribe());
}

function close(): void {
	store.close();
	dialogStore.closeDialog(DialogType.IntegrationDialog);
}
</script>
