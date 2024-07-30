<template>
	<!--	Show warning when no allowed to edit	-->
	<template v-if="!allowEditing">
		<QRow>
			<QCol>
				<q-alert
					border="bottom"
					colored-border
					type="warning"
					elevation="2"
				>
					{{ t('general.alerts.disabled-paths') }}
				</q-alert>
			</QCol>
		</QRow>
	</template>
	<template v-else>
		<QSection
			v-for="(folderGroup, i) in folderPathStore.getFolderPathsGroups(onlyDefaults)"
			:key="i"
		>
			<template
				v-if="!onlyDefaults"
				#header
			>
				{{ folderGroup.header }}
			</template>
			<template v-if="folderGroup.paths.length > 0">
				<QRow
					v-for="folderPath in folderGroup.paths"
					:key="folderPath.id"
					class="q-my-sm"
				>
					<QCol cols="3">
						<EditableText
							v-if="folderGroup.isFolderNameEditable"
							:value="folderPath.displayName"
							:disabled="!allowEditing"
							@save="saveDisplayName(folderPath.id, $event)"
						/>
						<HelpIcon
							v-else
							:help-id="toTranslation(folderPath.folderType)"
						/>
					</QCol>
					<!--	Folder Path Display	-->
					<QCol cols="7">
						<q-input
							:model-value="folderPath.directory"
							readonly
							class="folder-path-input"
						>
							<IconSquareButton
								icon="mdi-folder-open-outline"
								@click="openDirectoryBrowser(folderPath)"
							/>
						</q-input>
					</QCol>
					<!--	Is Valid Icon -->
					<QCol
						cols="auto"
						align-self="center"
					>
						<ValidIcon
							:valid="folderPath.isValid"
							:valid-text="t('general.alerts.valid-directory')"
							:invalid-text="t('general.alerts.invalid-directory')"
						/>
					</QCol>
					<!--	Delete Button -->
					<QCol
						v-if="folderGroup.IsFolderDeletable"
						cols="auto"
					>
						<DeleteIconButton
							:disabled="!allowEditing"
							@click="deleteFolderPath(folderPath.id)"
						/>
					</QCol>
				</QRow>
			</template>
			<!--	No custom FolderPaths	Warning -->
			<template v-else>
				<QRow
					justify="center"
					class="q-my-sm"
				>
					<QCol cols="auto">
						<h2>{{ t('components.folder-paths-overview.no-paths') }}</h2>
					</QCol>
				</QRow>
			</template>
			<!--	Add Path Button	-->
			<QRow
				v-if="folderGroup.isFolderAddable"
				justify="center"
				class="q-my-sm"
			>
				<QCol cols="auto">
					<AddIconButton
						:disabled="!allowEditing"
						@click="addFolderPath(folderGroup)"
					/>
				</QCol>
			</QRow>
		</QSection>
	</template>

	<!--	Directory Browser	-->
	<DirectoryBrowser
		:name="directoryBrowserName"
		@confirm="confirmDirectoryBrowser"
	/>
</template>

<script setup lang="ts">
import type { FolderPathDTO } from '@dto';
import type IFolderPathGroup from '@interfaces/IFolderPathGroup';
import {
	useI18n,
	useOpenControlDialog,
	toFolderPathStringId,
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
