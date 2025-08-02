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
		</template>
		<template #default>
			<TreeTable
				v-if="!loading"
				v-model:expanded-keys="expandedKeys"
				:value="downloadPreview">
				<Column
					v-for="(col, i) in getDownloadPreviewTableColumns"
					:key="i"
					:expander="i === 0"
					:field="col.field"
					:header="col.label">
					<template #body="{ node }: { node: IDownloadPreviewNode }">
						<template v-if="col.type === 'title'">
							<QMediaTypeIcon
								v-if="node.mediaType"
								:size="26"
								:media-type="node.mediaType" />
							<QText
								:cy="`column-title-${node.id}`"
								:value="node.title" />
						</template>
						<!-- Media Quality -->
						<MediaQuality
							v-else-if="col.type === 'media-quality'"
							:align="'center'"
							:data-cy="`column-${col.field}-${node.id}`"
							:qualities="node[col.field] as PlexMediaQualityDTO[]" />
						<!-- File Size -->
						<QFileSize
							v-else-if="col.type === 'file-size'"
							:cy="`column-dataTotal-${node.id}`"
							:size="node.size" />
					</template>
				</Column>

				<Column
					field="dataTotal"
					header="Size"
					style="max-width: 10rem">
					<template #body="{ node }: { node: IDownloadPreviewNode }">
						<QFileSize
							:cy="`column-dataTotal-${node.id}`"
							:size="node.size" />
					</template>
				</Column>
			</TreeTable>
			<Print> {{ downloadPreview }}</Print>
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
	type CreateDownloadTasksRequest, type DownloadMediaDTO,
	type DownloadPreviewDTO, type FolderPathDTO, FolderType, type PlexMediaQualityDTO,
	PlexMediaType,
} from '@dto';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import { useFolderPathStore, useDownloadStore, useDialogStore } from '@store';
import { sum } from 'lodash-es';
import type { TreeNode } from 'primevue/treenode';

interface IDownloadPreviewNode extends TreeNode, Omit<DownloadPreviewDTO, 'children'> {
	children?: IDownloadPreviewNode[];
}

const { t } = useI18n();
const downloadStore = useDownloadStore();
const folderPathStore = useFolderPathStore();
const dialogStore = useDialogStore();

const emits = defineEmits<{
	(e: 'download', downloadCommand: CreateDownloadTasksRequest): void;
}>();
const expandedKeys = ref<Record<string, boolean>>({});
const loading = ref(true);
const downloadPreview = ref<IDownloadPreviewNode[]>([]);
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
		},
		{
			label: t('components.download-confirmation.columns.file-size'),
			field: 'size',
			type: 'file-size',
		},
	];
});

const selectedFolderPath = ref<FolderPathDTO>(get(customDirectory));

const folderPathDestinations = computed(() => folderPathStore.getFolderPaths().filter((x) => x.mediaType === get(mediaType)));

function mapDownloadPreviewToNode(item: DownloadPreviewDTO): IDownloadPreviewNode {
	return {
		id: item.id,
		key: item.id.toString(),
		label: item.title,
		title: item.title,
		data: item,
		dataTotal: item.size,
		size: item.size,
		qualities: item.qualities,
		childCount: item.childCount,
		mediaType: item.mediaType,
		children: item.children?.map(mapDownloadPreviewToNode),
	};
}

function collectAllNodeKeys(nodes: IDownloadPreviewNode[]): Record<string, boolean> {
	const keys: Record<string, boolean> = {};

	function collectKeysRecursive(node: IDownloadPreviewNode): void {
		keys[node.key] = true;
		if (node.children && node.children.length > 0) {
			node.children.forEach(collectKeysRecursive);
		}
	}

	nodes.forEach(collectKeysRecursive);
	return keys;
}

function toggleExpanded(state: boolean): void {
	if (state) {
		const allKeys = collectAllNodeKeys(get(downloadPreview));
		set(expandedKeys, allKeys);
	} else {
		set(expandedKeys, {});
	}
}

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
			set(downloadPreview, result.map(mapDownloadPreviewToNode));
			toggleExpanded(true);
			set(totalSize, sum(result.map((x) => x.size)));
			set(loading, false);
		}),
	);
}

function closeDialog(): void {
	set(downloadPreview, []);
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
