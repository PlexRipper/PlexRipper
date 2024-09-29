<template>
	<!-- The "Are you sure" dialog -->
	<QCardDialog
		:name="name"
		max-width="1200px"
		content-height="80"
		:loading="loading"
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
			<QTreeViewTable
				:columns="getDownloadPreviewTableColumns()"
				:nodes="downloadPreview"
				default-expand-all
				connectors
				not-selectable />
		</template>
		<template #actions="{ close }">
			<CancelButton @click="close()" />
			<q-space />
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
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import type { DownloadMediaDTO, DownloadPreviewDTO } from '@dto';
import { getDownloadPreviewTableColumns } from '#imports';

const { t } = useI18n();
const downloadStore = useDownloadStore();

defineProps<{ name: string }>();

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
