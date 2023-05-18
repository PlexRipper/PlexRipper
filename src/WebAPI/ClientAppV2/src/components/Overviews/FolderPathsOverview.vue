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
		<q-section v-for="(folderGroup, i) in getFolderPathsGroups" :key="i">
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
				<q-col cols="1">
					<AddIconButton :disabled="!allowEditing" @click="addFolderPath(folderGroup)" />
				</q-col>
			</q-row>
		</q-section>
	</template>

	<!--	Directory Browser	-->
	<DirectoryBrowser :name="directoryBrowserName" @confirm="confirmDirectoryBrowser" />
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get } from '@vueuse/core';
import { kebabCase } from 'lodash-es';
import { FolderPathDTO, FolderType, PlexMediaType } from '@dto/mainApi';
import { DownloadService, FolderPathService } from '@service';
import { useI18n, useOpenControlDialog, toFolderPathStringId } from '#imports';

const { t } = useI18n();
const props = withDefaults(defineProps<{ onlyDefaults: boolean }>(), {
	onlyDefaults: false,
});

const directoryBrowserName = 'customDirectoryBrowser';
const folderPaths = ref<FolderPathDTO[]>([]);
const allowEditing = ref(true);

const getFolderPathsGroups = computed((): IFolderPathGroup[] => {
	const folderPathGroups: IFolderPathGroup[] = [];
	// Default Paths
	folderPathGroups.push({
		header: t('components.folder-paths-overview.main.header'),
		// The first 3 folderPaths are always the default ones.
		paths: get(folderPaths).filter((x) => x.id === 1 || x.id === 2 || x.id === 3),
		mediaType: PlexMediaType.None,
		folderType: FolderType.None,
		IsFolderDeletable: false,
		isFolderNameEditable: false,
		isFolderAddable: false,
	});

	if (props.onlyDefaults) {
		return folderPathGroups;
	}

	// Movie Paths
	folderPathGroups.push({
		header: t('components.folder-paths-overview.movie.header'),
		paths: get(folderPaths).filter(
			(x) => x.folderType === FolderType.MovieFolder && !folderPathGroups[0].paths.some((y) => y.id === x.id),
		),
		mediaType: PlexMediaType.Movie,
		folderType: FolderType.MovieFolder,
		IsFolderDeletable: true,
		isFolderNameEditable: true,
		isFolderAddable: true,
	});

	// TvShow Paths
	folderPathGroups.push({
		header: t('components.folder-paths-overview.tv-show.header'),
		paths: get(folderPaths).filter(
			(x) => x.folderType === FolderType.TvShowFolder && !folderPathGroups[0].paths.some((y) => y.id === x.id),
		),
		mediaType: PlexMediaType.TvShow,
		folderType: FolderType.TvShowFolder,
		IsFolderDeletable: true,
		isFolderNameEditable: true,
		isFolderAddable: true,
	});

	return folderPathGroups;
});

const openDirectoryBrowser = (path: FolderPathDTO): void => {
	useOpenControlDialog(directoryBrowserName, path);
};

const confirmDirectoryBrowser = (path: FolderPathDTO): void => {
	const i = folderPaths.value.findIndex((x) => x.id === path.id);
	if (i > -1) {
		const folderPath = { ...folderPaths.value[i], directory: path.directory };
		FolderPathService.updateFolderPath(folderPath);
	}
};

const addFolderPath = (folderGroup: IFolderPathGroup): void => {
	FolderPathService.createFolderPath({
		id: 0,
		displayName: t(`components.folder-paths-overview.${toFolderPathStringId(folderGroup.folderType)}.default-name`),
		directory: '',
		folderType: folderGroup.folderType,
		mediaType: folderGroup.mediaType,
		isValid: false,
	});
};

const deleteFolderPath = (id: number): void => {
	FolderPathService.deleteFolderPath(id);
};

function toTranslation(type: string): string {
	return `help.settings.paths.${kebabCase(type)}`;
}

const saveDisplayName = (id: number, value: string): void => {
	const folderPathIndex = folderPaths.value.findIndex((x) => x.id === id);
	if (folderPathIndex > -1) {
		const folderPath = { ...folderPaths.value[folderPathIndex], displayName: value };
		FolderPathService.updateFolderPath(folderPath);
	}
};

onMounted(() => {
	useSubscription(
		FolderPathService.getFolderPaths().subscribe((data) => {
			folderPaths.value = data ?? [];
		}),
	);

	// Ensure there are no active downloads before being allowed to change.
	useSubscription(
		DownloadService.getActiveDownloadList().subscribe((data) => {
			allowEditing.value = data?.length === 0 ?? false;
		}),
	);
});

interface IFolderPathGroup {
	header: string;
	paths: FolderPathDTO[];
	mediaType: PlexMediaType;
	folderType: FolderType;
	isFolderNameEditable: boolean;
	isFolderAddable: boolean;
	IsFolderDeletable: boolean;
}
</script>
<style lang="scss">
.folder-path-input {
	.q-field__control {
		// Ensures the folder button is outlined to the right border
		padding: 0 0 0 12px;
	}
}
</style>
