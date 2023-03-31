<template>
	<!-- The "Are you sure" dialog -->
	<q-dialog :model-value="showDialog">
		<q-card bordered class="download-confirmation">
			<q-card-title> {{ $t('components.download-confirmation.header') }}</q-card-title>
			<q-card-subtitle>
				<span>{{ $t('components.download-confirmation.description') }}</span> <br />
				<span>{{ $t('components.download-confirmation.total-size') }}</span>
				<q-file-size :size="totalSize" class="q-ml-sm" />
			</q-card-subtitle>
			<q-separator />
			<!-- Show Download Task Preview -->
			<q-card-section class="download-confirmation-container scroll">
				<q-tree :nodes="downloadPreview" label-key="title" node-key="id" default-expand-all>
					<template #default-header="{ node }">
						<q-row align="center">
							<q-col cols="auto">
								<QMediaTypeIcon :media-type="node.type" :size="28" class="q-mr-sm" />
							</q-col>
							<q-col>
								<div class="text-weight-bold">{{ node.title }}</div>
							</q-col>
							<q-col cols="auto">
								<q-file-size :size="node.size" />
							</q-col>
						</q-row>
					</template>
				</q-tree>
				<q-loading-overlay :loading="loading" />
			</q-card-section>

			<q-separator />

			<q-card-actions>
				<CancelButton @click="closeDialog" />
				<q-space />
				<ConfirmButton @click="confirmDownload" />
			</q-card-actions>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import { defineEmits, ref, defineExpose } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import sum from 'lodash-es/sum';
import { DownloadMediaDTO, DownloadPreviewDTO } from '@dto/mainApi';
import { DownloadService } from '@service';

const emit = defineEmits<{
	(e: 'download', downloadCommand: DownloadMediaDTO[]): void;
}>();

const loading = ref(true);
const showDialog = ref(false);
const downloadPreview = ref<DownloadPreviewDTO[]>([]);
const downloadMediaCommand = ref<DownloadMediaDTO[]>([]);
const totalSize = ref(0);

const openDialog = (data: DownloadMediaDTO[]): void => {
	loading.value = true;
	showDialog.value = true;
	downloadMediaCommand.value = data;
	useSubscription(
		DownloadService.previewDownload(data).subscribe((result) => {
			downloadPreview.value = result;
			loading.value = false;
			totalSize.value = sum(result.map((x) => x.size));
		}),
	);
};

const confirmDownload = (): void => {
	emit('download', downloadMediaCommand.value);
	showDialog.value = false;
};

const closeDialog = (): void => {
	showDialog.value = false;
	downloadPreview.value = [];
};

defineExpose({
	openDialog,
});
</script>
<style lang="scss">
.download-confirmation {
	width: 550px;

	.download-confirmation-container {
		min-height: 60vh;
		max-height: 60vh;
		overflow-x: hidden;
	}
}
</style>
