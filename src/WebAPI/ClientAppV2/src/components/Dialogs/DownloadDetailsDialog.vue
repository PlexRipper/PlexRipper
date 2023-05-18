<template>
	<QCardDialog :name="name" :loading="loading" @opened="onOpen">
		<template v-if="downloadTask" #title>
			<q-media-type-icon :size="36" :media-type="downloadTask.mediaType" />
			{{ downloadTask.fullTitle }}
		</template>
		<template v-if="downloadTask" #default>
			<q-markup-table class="section-table">
				<tbody>
					<tr>
						<td style="width: 25%">{{ t('components.download-details-dialog.overview.status') }}:</td>
						<td>{{ downloadTask.status }}</td>
					</tr>
					<tr v-if="downloadTask.fileName">
						<td>{{ t('components.download-details-dialog.overview.file-name') }}:</td>
						<td>{{ downloadTask.fileName }}</td>
					</tr>
					<tr>
						<td>{{ t('components.download-details-dialog.overview.download-path') }} :</td>
						<td>{{ downloadTask.downloadDirectory }}</td>
					</tr>
					<tr>
						<td>{{ t('components.download-details-dialog.overview.destination-path') }} :</td>
						<td>{{ downloadTask.destinationDirectory }}</td>
					</tr>
					<tr v-if="downloadTask.downloadUrl">
						<td>{{ t('components.download-details-dialog.overview.download-url') }} :</td>
						<td>
							<q-row no-gutters class="no-wrap">
								<q-col>
									{{ downloadTask.downloadUrl }}
								</q-col>
								<q-col cols="auto">
									<ExternalLinkButton :href="downloadTask.downloadUrl" />
								</q-col>
							</q-row>
						</td>
					</tr>
				</tbody>
			</q-markup-table>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import { DownloadTaskDTO } from '@dto/mainApi';
import { detailDownloadTask } from '@api/plexDownloadApi';

const { t } = useI18n();

const loading = ref(true);
const downloadTask = ref<DownloadTaskDTO>();

defineProps<{
	name: string;
}>();

function onOpen(downloadTaskId: number) {
	set(loading, true);
	detailDownloadTask(downloadTaskId).subscribe((data) => {
		if (data.isSuccess && data.value) {
			downloadTask.value = data.value;
		}
		set(loading, false);
	});
}
</script>
