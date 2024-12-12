<template>
	<!-- The "Are you sure" dialog -->
	<QCardDialog
		:name="DialogType.MediaDownloadConfirmationDialog"
		:loading="loading"
		full-height
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
			<ConfirmButton
				@click="
					() => {
						close();
						$emit('download', downloadMediaCommand);
					}
				" />
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import type { DownloadMediaDTO, DownloadPreviewDTO } from '@dto';
import { DialogType } from '@enums';
import { useI18n } from 'vue-i18n';
import { useDownloadStore } from '@store';

const { t } = useI18n();
const downloadStore = useDownloadStore();

defineEmits<{
	(e: 'download', downloadCommand: DownloadMediaDTO[]): void;
}>();

const loading = ref(true);
const downloadPreview = ref<DownloadPreviewDTO[]>([]);
const downloadMediaCommand = ref<DownloadMediaDTO[]>([]);
const totalSize = ref(0);

function openDialog(event: unknown): void {
	const data = event as DownloadMediaDTO[];
	set(loading, true);
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
</script>
