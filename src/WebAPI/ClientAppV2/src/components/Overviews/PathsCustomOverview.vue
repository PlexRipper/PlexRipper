<template>
	<!--	Custom FolderPaths	-->
	<template v-if="getFolderPaths.length > 0">
		<q-row v-for="folderPath in getFolderPaths" :key="folderPath.id" no-gutters class="q-my-sm" align="center">
			<q-col cols="3">
				<editable-text
					:value="folderPath.displayName"
					:disabled="!allowEditing"
					@save="saveDisplayName(folderPath.id, $event)" />
			</q-col>
			<q-col cols="7">
				<q-input :model-value="folderPath.directory" readonly>
					<BaseButton icon="mdi-folder-open-outline" icon-only square flat @click="openDirectoryBrowser(folderPath)" />
				</q-input>
			</q-col>
			<q-col cols="2">
				<!--	Is Valid Icon -->
				<valid-icon
					:valid="folderPath.isValid"
					:valid-text="$t('general.alerts.valid-directory')"
					:invalid-text="$t('general.alerts.invalid-directory')" />
				<!--	Delete Button -->
				<DeleteIconButton :disabled="!allowEditing" @click="deleteFolderPath(folderPath.id)" />
			</q-col>
		</q-row>
	</template>
	<!--	No custom FolderPaths	Warning-->
	<template v-else>
		<q-row justify="center" class="q-my-sm">
			<q-col cols="auto">
				<h2>{{ $t('components.paths-custom-overview.no-paths') }}</h2>
			</q-col>
		</q-row>
	</template>
	<!--	Add Path Button	-->
	<q-row justify="center" class="q-my-sm">
		<q-col cols="1">
			<AddIconButton :disabled="!allowEditing" @click="addFolderPath" />
		</q-col>
	</q-row>
	<!--	Directory Browser	-->
	<q-row>
		<q-col>
			<DirectoryBrowser :name="directoryBrowserName" @confirm="confirmDirectoryBrowser" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { withDefaults, defineProps, computed, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { FolderPathDTO, FolderType, PlexMediaType } from '@dto/mainApi';
import { DownloadService, FolderPathService } from '@service';
import DirectoryBrowser from '@components/General/DirectoryBrowser.vue';
import { useOpenControlDialog } from '#imports';

const props = withDefaults(
	defineProps<{
		folderType: FolderType;
	}>(),
	{
		folderType: FolderType.None,
	},
);

const directoryBrowserName = 'customDirectoryBrowser';
const folderPaths = ref<FolderPathDTO[]>([]);
const allowEditing = ref(true);

const getFolderPaths = computed((): FolderPathDTO[] => {
	// x.id >= 4 is to filter out the default paths.
	return folderPaths.value.filter((x) => x.folderType === props.folderType && x.id >= 4);
});

const getMediaType = computed((): PlexMediaType => {
	switch (props.folderType) {
		case FolderType.TvShowFolder:
			return PlexMediaType.TvShow;

		case FolderType.MovieFolder:
			return PlexMediaType.Movie;

		case FolderType.MusicFolder:
			return PlexMediaType.Music;

		case FolderType.PhotosFolder:
			return PlexMediaType.Photos;

		case FolderType.GamesVideosFolder:
			return PlexMediaType.Games;
		default:
			Log.error(`PathsCustomOverview.vue => Failed to convert ${props.folderType} to PlexMediaType`);
			return PlexMediaType.Unknown;
	}
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

const addFolderPath = (): void => {
	FolderPathService.createFolderPath({
		id: 0,
		displayName: `${props.folderType.replace('Folder', ' Folder')} Path`,
		directory: '',
		folderType: props.folderType,
		mediaType: getMediaType.value,
		isValid: false,
	});
};

const deleteFolderPath = (id: number): void => {
	FolderPathService.deleteFolderPath(id);
};

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
</script>
