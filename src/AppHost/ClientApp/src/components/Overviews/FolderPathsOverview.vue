<template>
	<template v-if="false">
		<!--	Show warning when not allowed to edit	-->
		<QRow>
			<QCol>
				<q-alert
					border="bottom"
					colored-border
					elevation="2"
					type="warning">
					{{ $t('general.alerts.disabled-paths') }}
				</q-alert>
			</QCol>
		</QRow>
	</template>
	<template v-else-if="onlyDefaults">
		<QSection
			v-for="(folderGroup, i) in folderPathStore.getFolderPathsGroups(true)"
			:key="i">
			<template v-if="folderGroup.paths.length > 0">
				<HelpRow
					v-for="folderPath in folderGroup.paths"
					:key="folderPath.id"
					disable-responsive
					:col-label="3"
					:col-content="8"
					:edit-model="folderPath.displayName"
					:allow-label-edit="folderGroup.isFolderNameEditable"
					:title="toTranslation(folderPath.folderType).title"
					:label="toTranslation(folderPath.folderType).label"
					:text="toTranslation(folderPath.folderType).text"
					@update:edit-model="saveDisplayName(folderPath.id, $event!)">
					<!--	Folder Path Display	-->
					<QRow :cy="`default-${kebabCase(folderPath.folderType)}-row`">
						<QCol cols="7">
							<q-input
								:model-value="folderPath.directory"
								class="folder-path-input"
								:data-cy="`default-${kebabCase(folderPath.folderType)}-input`"
								readonly>
								<IconSquareButton
									:cy="`default-${kebabCase(folderPath.folderType)}-edit-button`"
									icon="mdi-folder-open-outline"
									@click="dialogStore.openDirectoryBrowserDialog(folderPath)" />
							</q-input>
						</QCol>
						<!--	Is Valid Icon -->
						<QCol
							align-self="center"
							cols="auto">
							<ValidIcon
								:cy="`default-${kebabCase(folderPath.folderType)}-valid-icon`"
								:invalid-text="$t('general.alerts.invalid-directory')"
								:valid="folderPath.isValid ? ValidationLevel.Valid : ValidationLevel.Invalid"
								:valid-text="$t('general.alerts.valid-directory')" />
						</QCol>
						<!--	Delete Button -->
						<QCol
							v-if="folderGroup.IsFolderDeletable && !folderPath.isDefault"
							cols="auto">
							<DeleteIconButton @click="deleteFolderPath(folderPath)" />
						</QCol>
					</QRow>
				</HelpRow>
			</template>
			<QRow
				v-if="folderGroup.isFolderAddable"
				class="q-my-sm"
				justify="center">
				<QCol cols="auto">
					<AddIconButton @click="addFolderPath(folderGroup)" />
				</QCol>
			</QRow>
		</QSection>
	</template>
	<template v-else>
		<QSection
			:header="$t('components.folder-paths-overview.main.header')">
			<q-tabs
				v-model="activeTab"
				active-color="primary"
				align="justify"
				indicator-color="primary"
				class="folder-path-tabs">
				<q-tab
					v-for="tab in folderPathTabs"
					:key="tab.name"
					:name="tab.name"
					:data-cy="`folder-path-tab-${tab.name}`">
					<q-icon
						v-if="tab.name === 'download'"
						name="mdi-download"
						class="q-mr-sm" />
					<QMediaTypeIcon
						v-else
						:media-type="tab.mediaType"
						class="q-mr-sm" />
					<span>{{ tab.label }}</span>
				</q-tab>
			</q-tabs>
			<q-tab-panels
				v-model="activeTab"
				animated>
				<q-tab-panel
					v-for="tab in folderPathTabs"
					:key="tab.name"
					:name="tab.name"
					class="q-pa-none q-mt-md">
					<HelpRow
						v-for="folderPath in tab.paths"
						:key="folderPath.id"
						disable-responsive
						:col-label="3"
						:col-content="8"
						:edit-model="folderPath.displayName"
						:allow-label-edit="!folderPath.isDefault"
						:title="toTranslation(folderPath.folderType).title"
						:label="toTranslation(folderPath.folderType).label"
						:text="toTranslation(folderPath.folderType).text"
						@update:edit-model="saveDisplayName(folderPath.id, $event!)">
						<QRow
							no-wrap
							align="center"
							:cy="`${getFolderPathCyPrefix(folderPath)}-row`">
							<QCol
								cols="grow"
								class="folder-path-directory">
								<q-input
									:model-value="folderPath.directory"
									class="folder-path-input"
									:data-cy="`${getFolderPathCyPrefix(folderPath)}-input`"
									readonly>
									<IconSquareButton
										:cy="`${getFolderPathCyPrefix(folderPath)}-edit-button`"
										icon="mdi-folder-open-outline"
										@click="dialogStore.openDirectoryBrowserDialog(folderPath)" />
								</q-input>
							</QCol>
							<QCol
								:width="56"
								class="folder-path-action">
								<ValidIcon
									:cy="`${getFolderPathCyPrefix(folderPath)}-valid-icon`"
									:invalid-text="$t('general.alerts.invalid-directory')"
									:valid="folderPath.isValid ? ValidationLevel.Valid : ValidationLevel.Invalid"
									:valid-text="$t('general.alerts.valid-directory')" />
							</QCol>
							<QCol
								:width="56"
								class="folder-path-action">
								<DeleteIconButton
									v-if="!folderPath.isDefault"
									:cy="`${getFolderPathCyPrefix(folderPath)}-delete-button`"
									@click="deleteFolderPath(folderPath)" />
							</QCol>
						</QRow>
					</HelpRow>
					<QRow
						v-if="tab.isFolderAddable"
						class="q-my-sm"
						justify="center">
						<QCol cols="auto">
							<AddIconButton
								:cy="`${tab.name}-add-button`"
								@click="addFolderPath(tab)" />
						</QCol>
					</QRow>
				</q-tab-panel>
			</q-tab-panels>
		</QSection>
	</template>

	<ConfirmationDialog
		:name="DialogType.FolderPathDeleteConfirmationDialog"
		:title="$t('confirmation.delete-folder-path.title')"
		:text="$t('confirmation.delete-folder-path.text', { displayName: pendingDeleteFolderPath?.displayName ?? '' })"
		:confirm-label="$t('general.commands.delete')"
		@cancel="cancelDeleteFolderPath"
		@confirm="confirmDeleteFolderPath" />

	<!--	Directory Browser	-->
	<DirectoryBrowser @confirm="confirmDirectoryBrowser" />
</template>

<script lang="ts" setup>
import { get, set } from '@vueuse/core';
import { type FolderPathDTO, FolderType, PlexMediaType } from '@dto';
import type { IHelp, IFolderPathGroup } from '@interfaces';
import { DialogType, ValidationLevel } from '@enums';
import { kebabCase } from 'lodash-es';
import { showErrorNotification, useDialogStore, useFolderPathStore, useI18n, useSubscription } from '#imports';

const { t } = useI18n();

const dialogStore = useDialogStore();
const folderPathStore = useFolderPathStore();
const activeTab = ref('download');
const pendingDeleteFolderPath = ref<FolderPathDTO | null>(null);

type FolderPathTab = Pick<IFolderPathGroup, 'paths' | 'mediaType' | 'folderType' | 'isFolderAddable'> & {
	name: 'download' | 'movie' | 'tv-show';
	label: string;
};

withDefaults(defineProps<{ onlyDefaults?: boolean }>(), {
	onlyDefaults: false,
});

const folderPathTabs = computed<FolderPathTab[]>(() => {
	const allPaths = folderPathStore.getFolderPaths();
	const definitions = [
		{
			name: 'download' as const,
			label: t('components.folder-paths-overview.tabs.download'),
			mediaType: PlexMediaType.None,
			folderType: FolderType.DownloadFolder,
		},
		{
			name: 'movie' as const,
			label: t('components.folder-paths-overview.tabs.movie'),
			mediaType: PlexMediaType.Movie,
			folderType: FolderType.MovieFolder,
		},
		{
			name: 'tv-show' as const,
			label: t('components.folder-paths-overview.tabs.tv-show'),
			mediaType: PlexMediaType.TvShow,
			folderType: FolderType.TvShowFolder,
		},
	];

	return definitions.map((definition) => {
		const paths = allPaths.filter((folderPath) => folderPath.folderType === definition.folderType);

		return {
			...definition,
			paths: paths.sort((left, right) => Number(right.isDefault) - Number(left.isDefault)),
			isFolderAddable: true,
		};
	});
});

const confirmDirectoryBrowser = (path: FolderPathDTO): void => {
	if (path.id === 0) {
		// New folder path — create with the confirmed directory
		useSubscription(
			folderPathStore.createFolderPath(path).subscribe({
				error(err) {
					showErrorNotification(err);
				},
			}),
		);
	} else {
		// Existing folder path — update the directory
		useSubscription(
			folderPathStore.setFolderPathDirectory(path.id, path.directory).subscribe({
				error(err) {
					showErrorNotification(err);
				},
			}),
		);
	}
};

function addFolderPath(folderGroup: Pick<IFolderPathGroup, 'folderType' | 'mediaType'>): void {
	let displayName = '';
	switch (folderGroup.folderType) {
		case FolderType.DownloadFolder:
			displayName = t('components.folder-paths-overview.download.default-name');
			break;
		case FolderType.MovieFolder:
			displayName = t('components.folder-paths-overview.movie.default-name');
			break;
		case FolderType.TvShowFolder:
			displayName = t('components.folder-paths-overview.tv-show.default-name');
			break;
		default:
			throw new Error(`Unknown folder type: ${folderGroup.folderType}`);
	}

	dialogStore.openDirectoryBrowserDialog({
		id: 0,
		displayName,
		directory: '',
		folderType: folderGroup.folderType,
		mediaType: folderGroup.mediaType,
		isValid: false,
		isDefault: false,
	});
}

function deleteFolderPath(folderPath: FolderPathDTO): void {
	if (folderPath.isDefault) {
		return;
	}

	set(pendingDeleteFolderPath, folderPath);
	dialogStore.openDialog(DialogType.FolderPathDeleteConfirmationDialog);
}

function cancelDeleteFolderPath(): void {
	set(pendingDeleteFolderPath, null);
}

function confirmDeleteFolderPath(): void {
	const folderPath = get(pendingDeleteFolderPath);
	if (!folderPath) {
		return;
	}

	useSubscription(
		folderPathStore.deleteFolderPath(folderPath.id).subscribe({
			next: (result) => {
				if (result.isSuccess) {
					dialogStore.closeDialog(DialogType.FolderPathDeleteConfirmationDialog);
					set(pendingDeleteFolderPath, null);
				}
			},
			error(err) {
				showErrorNotification(err);
			},
		}),
	);
}

function getFolderPathCyPrefix(folderPath: FolderPathDTO): string {
	return `${folderPath.isDefault ? 'default' : 'custom'}-${kebabCase(folderPath.folderType)}`;
}

function toTranslation(type: FolderType): IHelp {
	switch (type) {
		case FolderType.DownloadFolder:
			return {
				label: t('help.settings.paths.download-folder.label'),
				text: t('help.settings.paths.download-folder.text'),
				title: t('help.settings.paths.download-folder.title'),
			};
		case FolderType.MovieFolder:
			return {
				label: t('help.settings.paths.movie-folder.label'),
				text: t('help.settings.paths.movie-folder.text'),
				title: t('help.settings.paths.movie-folder.title'),
			};
		case FolderType.TvShowFolder:
			return {
				label: t('help.settings.paths.tv-show-folder.label'),
				text: t('help.settings.paths.tv-show-folder.text'),
				title: t('help.settings.paths.tv-show-folder.title'),
			};
		default:
			throw new Error('FolderType not supported');
	}
}

function saveDisplayName(id: number, value: string) {
	useSubscription(
		folderPathStore.setFolderPathDisplayName(id, value).subscribe({
			error(err) {
				showErrorNotification(err);
			},
		}),
	);
}
</script>

<style lang="scss">
.folder-path-tabs {
  min-height: 72px;

  .q-tab {
    min-height: 72px;
  }
}

.folder-path-directory {
  min-width: 0;
}

.folder-path-action {
  display: flex;
  align-items: center;
  justify-content: center;
}

.folder-path-input {
  .q-field__control {
    // Ensures the folder button is outlined to the right border
    padding: 0 0 0 12px;
  }
}
</style>
