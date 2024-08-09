<template>
	<!--	Show warning when no allowed to edit	-->
	<template v-if="!allowEditing">
		<QRow>
			<QCol>
				<q-alert
					border="bottom"
					colored-border
					elevation="2"
					type="warning">
					{{ t('general.alerts.disabled-paths') }}
				</q-alert>
			</QCol>
		</QRow>
	</template>
	<template v-else>
		<QSection
			v-for="(folderGroup, i) in folderPathStore.getFolderPathsGroups(onlyDefaults)"
			:key="i">
			<template
				v-if="!onlyDefaults"
				#header>
				{{ folderGroup.header }}
			</template>
			<template v-if="folderGroup.paths.length > 0">
				<QRow
					v-for="folderPath in folderGroup.paths"
					:key="folderPath.id"
					class="q-my-sm">
					<QCol cols="3">
						<EditableText
							v-if="folderGroup.isFolderNameEditable"
							:disabled="!allowEditing"
							:value="folderPath.displayName"
							@save="saveDisplayName(folderPath.id, $event)" />
						<HelpIcon
							v-else
							:value="toTranslation(folderPath.folderType)" />
					</QCol>
					<!--	Folder Path Display	-->
					<QCol cols="7">
						<q-input
							:model-value="folderPath.directory"
							class="folder-path-input"
							readonly>
							<IconSquareButton
								icon="mdi-folder-open-outline"
								@click="openDirectoryBrowser(folderPath)" />
						</q-input>
					</QCol>
					<!--	Is Valid Icon -->
					<QCol
						align-self="center"
						cols="auto">
						<ValidIcon
							:invalid-text="t('general.alerts.invalid-directory')"
							:valid="folderPath.isValid"
							:valid-text="t('general.alerts.valid-directory')" />
					</QCol>
					<!--	Delete Button -->
					<QCol
						v-if="folderGroup.IsFolderDeletable"
						cols="auto">
						<DeleteIconButton
							:disabled="!allowEditing"
							@click="deleteFolderPath(folderPath.id)" />
					</QCol>
				</QRow>
			</template>
			<!--	No custom FolderPaths	Warning -->
			<template v-else>
				<QRow
					class="q-my-sm"
					justify="center">
					<QCol cols="auto">
						<QText size="h4">
							{{ $t('components.folder-paths-overview.no-paths') }}
						</QText>
					</QCol>
				</QRow>
			</template>
			<!--	Add Path Button	-->
			<QRow
				v-if="folderGroup.isFolderAddable"
				class="q-my-sm"
				justify="center">
				<QCol cols="auto">
					<AddIconButton
						:disabled="!allowEditing"
						@click="addFolderPath(folderGroup)" />
				</QCol>
			</QRow>
		</QSection>
	</template>

	<!--	Directory Browser	-->
	<DirectoryBrowser
		:name="directoryBrowserName"
		@confirm="confirmDirectoryBrowser" />
</template>

<script lang="ts" setup>
import { type FolderPathDTO, FolderType } from '@dto';
import type IFolderPathGroup from '@interfaces/IFolderPathGroup';
import type { IHelp } from '@interfaces';
import {
	useI18n,
	useOpenControlDialog,
	useFolderPathStore,
	useSubscription,
	showErrorNotification,
} from '#imports';

const { t } = useI18n();
const folderPathStore = useFolderPathStore();
const downloadStore = useDownloadStore();
withDefaults(defineProps<{ onlyDefaults?: boolean }>(), {
	onlyDefaults: false,
});

const directoryBrowserName = 'customDirectoryBrowser';

const openDirectoryBrowser = (path: FolderPathDTO): void => {
	useOpenControlDialog(directoryBrowserName, path);
};

const allowEditing = computed(() => {
	return downloadStore.getActiveDownloadList().length === 0;
});

const confirmDirectoryBrowser = (path: FolderPathDTO): void => {
	useSubscription(
		folderPathStore.setFolderPathDirectory(path.id, path.directory).subscribe({
			error(err) {
				showErrorNotification(err);
			},
		}),
	);
};

function addFolderPath(folderGroup: IFolderPathGroup): void {
	let displayName = '';
	switch (folderGroup.folderType) {
		case FolderType.MovieFolder:
			displayName = t('components.folder-paths-overview.movie.default-name');
			break;
		case FolderType.TvShowFolder:
			displayName = t('components.folder-paths-overview.tv-show.default-name');
			break;
		default:
			throw new Error(`Unknown folder type: ${folderGroup.folderType}`);
	}

	useSubscription(
		folderPathStore
			.createFolderPath({
				id: 0,
				displayName,
				directory: '',
				folderType: folderGroup.folderType,
				mediaType: folderGroup.mediaType,
				isValid: false,
			})
			.subscribe(),
	);
}

function deleteFolderPath(id: number): void {
	folderPathStore.deleteFolderPath(id).subscribe();
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

const saveDisplayName = (id: number, value: string): void => {
	useSubscription(
		folderPathStore.setFolderPathDisplayName(id, value).subscribe({
			error(err) {
				showErrorNotification(err);
			},
		}),
	);
};
</script>

<style lang="scss">
.folder-path-input {
	.q-field__control {
		// Ensures the folder button is outlined to the right border
		padding: 0 0 0 12px;
	}
}
</style>
