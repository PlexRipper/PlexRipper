<template>
	<!-- The "Are you sure" dialog -->
	<QCardDialog
		:loading="loading"
		:name="DialogType.MediaDownloadConfirmationDialog"
		:type="[] as DownloadMediaDTO[]"
		full-height
		@closed="closeDialog"
		@opened="openDialog">
		<template #top-row>
			<QRow class="q-pa-md">
				<QCol>
					<QText size="h5">
						{{ t('components.download-confirmation.description') }}
					</QText>
				</QCol>
				<QCol cols="auto">
					<QText>
						{{ t('components.download-confirmation.total-size') }}
						<QFileSize
							:size="totalSize"
							class="q-ml-sm" />
					</QText>
				</QCol>
			</QRow>
			<TreeTable
				:lazy="true"
				:loading="loading"
				:size="'small'"
				class="download-confirmation-table-header">
				<Column
					v-for="(col, i) in getDownloadPreviewTableColumns"
					:key="i"
					:expander="i === 0"
					:field="col.field"
					:header="col.label"
					:style="{ width: col.width ? `${col.width}px` : 'auto' }" />
			</TreeTable>
		</template>
		<template #default="{ size }">
			<TreeTable
				v-model:expanded-keys="expandedKeys"
				:lazy="true"
				:loading="loading"
				:size="'small'"
				:value="downloadPreview"
				class="download-confirmation-table-body">
				<Column
					v-for="(col, i) in getDownloadPreviewTableColumns"
					:key="i"
					:expander="i === 0"
					:field="col.field"
					:header="col.label"
					:style="{ width: col.width ? `${col.width}px` : 'auto' }">
					<template #body="{ node }: { node: DownloadPreviewDTO }">
						<template v-if="col.type === 'title'">
							<QMediaTypeIcon
								:media-type="node.type"
								:size="26" />
							<QText
								:cy="`column-title-${node.key}`"
								:value="node.title" />
						</template>
						<!-- Media Quality -->
						<MediaQuality
							v-else-if="col.type === 'media-quality'"
							:align="'center'"
							:data-cy="`column-${col.field}-${node.key}`"
							:qualities="node.qualities" />
						<!-- File Size -->
						<QFileSize
							v-else-if="col.type === 'file-size'"
							:cy="`column-dataTotal-${node.key}`"
							:size="node.size" />
					</template>
				</Column>
			</TreeTable>
		</template>
		<!-- Download Actions -->
		<template #actions="{ close }">
			<CancelButton @click="close()" />
			<q-btn-dropdown
				color="green"
				label="Download"
				outline
				split
				@click="onDownload(close)">
				<QSection :header="$t('components.download-confirmation.destination.header')">
					<!-- Download Destination -->
					<q-list>
						<q-item
							v-for="folderPath in folderPathDestinations"
							:key="folderPath.id"
							clickable
							tag="label">
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
				</QSection>
			</q-btn-dropdown>
			<!--	Directory Browser	-->
			<DirectoryBrowser @confirm="onCustomDirectorySelected" />
		</template>
	</QCardDialog>
</template>

<script lang="ts" setup>
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import {
	type CreateDownloadTasksRequest,
	type DownloadMediaDTO,
	type DownloadPreviewDTO,
	type FolderPathDTO,
	FolderType,
	PlexMediaType,
} from '@dto';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import { useDialogStore, useDownloadStore, useFolderPathStore } from '@store';
import Log from 'consola';
import QCardDialog from '@components/Common/QCardDialog.vue';

const { t } = useI18n();
const downloadStore = useDownloadStore();
const folderPathStore = useFolderPathStore();
const dialogStore = useDialogStore();

const emits = defineEmits<{
	(e: 'download', downloadCommand: CreateDownloadTasksRequest): void;
}>();
const expandedKeys = ref<Record<string, boolean>>({});
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
	isDefault: false,
});

const getDownloadPreviewTableColumns = computed((): {
	label: string;
	field: keyof DownloadPreviewDTO;
	type?: 'title' | 'duration' | 'file-size' | 'file-speed' | 'date' | 'actions' | 'datetime' | 'percentage' | 'index' | 'media-quality';
	width?: number;
}[] => {
	return [
		{
			label: t('components.download-confirmation.columns.title'),
			field: 'title',
			type: 'title',
		},
		{
			label: t('components.media-list.columns.quality'),
			field: 'qualities',
			type: 'media-quality',
			width: 200,
		},
		{
			label: t('components.download-confirmation.columns.file-size'),
			field: 'size',
			type: 'file-size',
			width: 150,
		},
	];
});

const selectedFolderPath = ref<FolderPathDTO>(get(customDirectory));

const folderPathDestinations = computed(() => folderPathStore.getFolderPaths().filter((x) => x.mediaType === get(mediaType)));

function openDialog(data: DownloadMediaDTO[]): void {
	set(loading, true);

	// This assumes that the data is always 1 category, either movie or tv show
	if (data.some((x) => x.type === PlexMediaType.Movie)) {
		set(mediaType, PlexMediaType.Movie);
	} else if (data.some((x) => x.type === PlexMediaType.TvShow || x.type === PlexMediaType.Season || x.type === PlexMediaType.Episode)) {
		set(mediaType, PlexMediaType.TvShow);
	} else {
		set(mediaType, PlexMediaType.Unknown);
	}

	set(selectedFolderPath, get(folderPathDestinations)[0]);

	set(downloadMediaCommand, data);
	useSubscription(
		downloadStore.previewDownload(data).subscribe((result) => {
			if (!result) {
				Log.error('Download preview failed, no data received');
				set(loading, false);
				return;
			}

			set(downloadPreview, Object.freeze(result.previews));
			set(totalSize, result.totalSize);
			set(expandedKeys, result.expanded);
			set(loading, false);
		}),
	);
}

function closeDialog(): void {
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

<style lang="scss">
.download-confirmation-table-header {
  padding-right: 10px;
  .p-treetable-empty-message {
    display: none;
  }
}

.download-confirmation-table-body {
  .p-treetable-thead {
    display: none;
  }
}
</style>
