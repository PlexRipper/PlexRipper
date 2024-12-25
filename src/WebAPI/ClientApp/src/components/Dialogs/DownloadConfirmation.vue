<template>
	<!-- The "Are you sure" dialog -->
	<QCardDialog
		:name="DialogType.MediaDownloadConfirmationDialog"
		:loading="loading"
		full-height
		:type="[] as DownloadMediaDTO[]"
		@opened="openDialog"
		@closed="closeDialog">
		<template #title>
			{{ t('components.download-confirmation.header') }}
		</template>
		<template #top-row>
			<span>{{ t('components.download-confirmation.description') }}</span> <br>
			<span>{{ t('components.download-confirmation.total-size') }}</span>
			<QFileSize
				:size="totalSize"
				class="q-ml-sm" />
		</template>
		<template #default>
			<div>
				<QTreeViewTable
					:columns="getDownloadPreviewTableColumns()"
					:nodes="downloadPreview"
					default-expand-all
					connectors
					not-selectable />
			</div>
		</template>
		<template #actions="{ close }">
			<CancelButton @click="close()" />

			<q-btn-dropdown
				outline
				color="green"
				label="Download"
				split
				@click="onDownload(close)">
				<QRow>
					<QCol>
						<QText size="h6">
							{{ $t('components.download-confirmation.destination.header') }}
						</QText>

						<q-scroll-area style="height: 200px; width: 400px; max-width: 400px">
							<!-- Download Destination -->
							<q-list>
								<q-item
									v-for="folderPath in folderPathDestinations"
									:key="folderPath.id"
									tag="label"
									clickable>
									<q-item-section avatar>
										<q-radio
											v-model="selectedFolderPath"
											:val="folderPath" />
									</q-item-section>
									<q-item-section>
										<q-item-label>{{ folderPath.displayName }}</q-item-label>
										<q-item-label caption>
											{{ folderPath.directory }}
										</q-item-label>
									</q-item-section>
								</q-item>
								<!-- Custom Directory -->
								<q-item
									clickable
									@click="dialogStore.openDirectoryBrowserDialog(customDirectory)">
									<q-item-section avatar>
										<q-radio
											v-model="selectedFolderPath"
											:val="customDirectory" />
									</q-item-section>
									<q-item-section>
										<q-item-label>
											{{ $t('components.download-confirmation.destination.custom-destination-option') }}
										</q-item-label>
										<q-item-label caption>
											{{ customDirectory.directory }}
										</q-item-label>
									</q-item-section>
								</q-item>
							</q-list>
						</q-scroll-area>
					</QCol>
				</QRow>
			</q-btn-dropdown>
			<!--	Directory Browser	-->
			<DirectoryBrowser @confirm="onCustomDirectorySelected" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import {
	type CreateDownloadTasksRequest,
	type DownloadMediaDTO,
	type DownloadPreviewDTO,
	type FolderPathDTO, FolderType,
	PlexMediaType,
} from '@dto';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import { useFolderPathStore, useDownloadStore, useDialogStore } from '@store';

const { t } = useI18n();
const downloadStore = useDownloadStore();
const folderPathStore = useFolderPathStore();
const dialogStore = useDialogStore();

const emits = defineEmits<{
	(e: 'download', downloadCommand: CreateDownloadTasksRequest): void;
}>();

const loading = ref(true);
const downloadPreview = ref<DownloadPreviewDTO[]>([]);
const downloadMediaCommand = ref<DownloadMediaDTO[]>([]);
const mediaType = ref<PlexMediaType>(PlexMediaType.Unknown);
const totalSize = ref(0);
const customDirectory = ref<FolderPathDTO>({
	id: 0,
	displayName: 'Custom',
	directory: '',
	mediaType: get(mediaType),
	folderType: FolderType.TvShowFolder,
	isValid: true,
});
const selectedFolderPath = ref<FolderPathDTO>(get(customDirectory));

const folderPathDestinations = computed(() => folderPathStore.getFolderPaths().filter((x) => x.mediaType === get(mediaType)));

function openDialog(data: DownloadMediaDTO[]): void {
	set(loading, true);

	// This assumes that the data is always 1 category, either movie or tv show
	if (data.some((x) => x.type === PlexMediaType.Movie)) {
		set(mediaType, PlexMediaType.Movie);
	} else if (data.some((x) => x.type === PlexMediaType.TvShow)) {
		set(mediaType, PlexMediaType.TvShow);
	}

	set(selectedFolderPath, folderPathDestinations.value[0]);

	set(downloadMediaCommand, data);
	useSubscription(
		downloadStore.previewDownload(data).subscribe((result) => {
			set(downloadPreview, result);
			set(totalSize, sum(result.map((x) => x.size)));
			set(loading, false);
		}),
	);
}

function closeDialog(): void {
	downloadPreview.value = [];
}

function onCustomDirectorySelected(path: FolderPathDTO): void {
	set(customDirectory, path);
	set(selectedFolderPath, get(customDirectory));
}

function onDownload(close: () => void) {
	emits('download', {
		downloadMedias: get(downloadMediaCommand),
		destinationFolderPathId: get(selectedFolderPath).id > 0 ? get(selectedFolderPath).id : 0,
		customDestinationFolderPath: get(selectedFolderPath).id === 0 ? get(selectedFolderPath).directory : '',
	});
	close();
}
</script>
