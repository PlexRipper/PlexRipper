<template>
	<!--	Show warning when no allowed to edit	-->
	<template v-if="!allowEditing">
		<q-row>
			<q-col>
				<q-alert border="bottom" colored-border type="warning" elevation="2">
					{{ t('general.alerts.disabled-paths') }}
				</q-alert>
			</q-col>
		</q-row>
	</template>
	<template v-else>
		<q-section v-for="(folderGroup, i) in folderPathStore.getFolderPathsGroups(onlyDefaults)" :key="i">
			<template v-if="!onlyDefaults" #header> {{ folderGroup.header }}</template>
			<template v-if="folderGroup.paths.length > 0">
				<q-row v-for="folderPath in folderGroup.paths" :key="folderPath.id" class="q-my-sm">
					<q-col cols="3">
						<editable-text
							v-if="folderGroup.isFolderNameEditable"
							:value="folderPath.displayName"
							:disabled="!allowEditing"
							@save="saveDisplayName(folderPath.id, $event)" />
						<help-icon v-else :help-id="toTranslation(folderPath.folderType)" />
					</q-col>
					<!--	Folder Path Display	-->
					<q-col cols="7">
						<q-input :model-value="folderPath.directory" readonly class="folder-path-input">
							<IconSquareButton icon="mdi-folder-open-outline" @click="openDirectoryBrowser(folderPath)" />
						</q-input>
					</q-col>
					<!--	Is Valid Icon -->
					<q-col cols="auto" align-self="center">
						<valid-icon
							:valid="folderPath.isValid"
							:valid-text="t('general.alerts.valid-directory')"
							:invalid-text="t('general.alerts.invalid-directory')" />
					</q-col>
					<!--	Delete Button -->
					<q-col v-if="folderGroup.IsFolderDeletable" cols="auto">
						<DeleteIconButton :disabled="!allowEditing" @click="deleteFolderPath(folderPath.id)" />
					</q-col>
				</q-row>
			</template>
			<!--	No custom FolderPaths	Warning-->
			<template v-else>
				<q-row justify="center" class="q-my-sm">
					<q-col cols="auto">
						<h2>{{ t('components.folder-paths-overview.no-paths') }}</h2>
					</q-col>
				</q-row>
			</template>
			<!--	Add Path Button	-->
			<q-row v-if="folderGroup.isFolderAddable" justify="center" class="q-my-sm">
				<q-col cols="auto">
					<AddIconButton :disabled="!allowEditing" @click="addFolderPath(folderGroup)" />
				</q-col>
			</q-row>
		</q-section>
	</template>

	<!--	Directory Browser	-->
	<DirectoryBrowser :name="directoryBrowserName" @confirm="confirmDirectoryBrowser" />
</template>

<script setup lang="ts">
import { FolderPathDTO } from '@dto/mainApi';
import {
	useI18n,
	useOpenControlDialog,
	toFolderPathStringId,
	useFolderPathStore,
	useSubscription,
	showErrorNotification,
} from '#imports';
import IFolderPathGroup from '@interfaces/IFolderPathGroup';

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
	useSubscription(
		folderPathStore
			.createFolderPath({
				id: 0,
				displayName: t(`components.folder-paths-overview.${toFolderPathStringId(folderGroup.folderType)}.default-name`),
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

function toTranslation(type: string): string {
	return `help.settings.paths.${kebabCase(type)}`;
}

const saveDisplayName = (id: number, value: string): void => {
	useSubscription(
		folderPathStore.setFolderPathDirectory(id, value).subscribe({
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
