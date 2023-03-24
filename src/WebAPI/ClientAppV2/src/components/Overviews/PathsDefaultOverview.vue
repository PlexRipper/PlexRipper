<template>
	<!--	Show warning when no allowed to edit	-->
	<template v-if="!allowEditing">
		<q-row>
			<q-col>
				<q-alert border="bottom" colored-border type="warning" elevation="2">
					{{ $t('general.alerts.disabled-paths') }}
				</q-alert>
			</q-col>
		</q-row>
	</template>
	<!--	Default FolderPaths	-->
	<q-row v-for="folderPath in getFolderPaths" :key="folderPath.id" no-gutters class="q-my-sm">
		<q-col cols="3">
			<help-icon :help-id="toTranslation(folderPath.folderType)" />
		</q-col>
		<q-col cols="7">
			<p-text-field
				append-icon="mdi-folder-open-outline"
				:model-value="folderPath.directory"
				readonly
				:disable="!allowEditing"
				@click:append="openDirectoryBrowser(folderPath)" />
		</q-col>
		<!--	Is Valid Icon -->
		<q-col cols="auto" align-self="center">
			<valid-icon
				:valid="folderPath.isValid"
				:valid-text="$t('general.alerts.valid-directory')"
				:invalid-text="$t('general.alerts.invalid-directory')" />
		</q-col>
	</q-row>
	<!--	Directory Browser	-->
	<q-row>
		<q-col>
			<DirectoryBrowser ref="directoryBrowser" @confirm="confirmDirectoryBrowser" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, computed } from 'vue';
import { kebabCase } from 'lodash-es';
import { useSubscription } from '@vueuse/rxjs';
import { FolderPathDTO } from '@dto/mainApi';
import { updateFolderPath } from '@api/pathApi';
import { DownloadService, FolderPathService } from '@service';
import DirectoryBrowser from '@components/General/DirectoryBrowser.vue';

const folderPaths = ref<FolderPathDTO[]>([]);
const allowEditing = ref(true);
const selectedFolderPath = ref<FolderPathDTO | null>(null);

const directoryBrowser = ref<InstanceType<typeof DirectoryBrowser> | null>(null);

const getFolderPaths = computed(() => {
	// The first 3 folderPaths are always the default ones.
	return folderPaths.value.filter((x) => x.id === 1 || x.id === 2 || x.id === 3);
});

function openDirectoryBrowser(path: FolderPathDTO): void {
	selectedFolderPath.value = path;
	directoryBrowser.value?.open(path);
}

function confirmDirectoryBrowser(path: FolderPathDTO): void {
	selectedFolderPath.value = path;

	useSubscription(
		updateFolderPath(path).subscribe((data) => {
			if (data.isSuccess && data.value) {
				Log.debug(`Successfully updated folder path ${path.displayName}`, data.value);
				const i = folderPaths.value.findIndex((x) => x.id === data.value?.id);
				if (i > -1) {
					folderPaths.value.splice(i, 1, data.value);
				}
			}
		}),
	);
}

function toTranslation(type: string): string {
	return `help.settings.paths.${kebabCase(type)}`;
}

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
