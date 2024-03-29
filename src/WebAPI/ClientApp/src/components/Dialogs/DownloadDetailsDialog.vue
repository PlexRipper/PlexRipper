<template>
	<QCardDialog :name="name" :loading="loading" content-height="60" max-width="60vw" @opened="onOpen">
		<template #title>
			<template v-if="downloadTask">
				<q-media-type-icon :size="36" :media-type="downloadTask.mediaType" />
				<span data-cy="download-details-dialog-title">
					{{ downloadTask.fullTitle }}
				</span>
			</template>
			<template v-else> {{ $t('components.download-details-dialog.invalid-download-task.title') }} </template>
		</template>
		<template #default>
			<q-markup-table v-if="downloadTask" class="section-table">
				<tbody>
					<tr>
						<td style="width: 25%">{{ t('components.download-details-dialog.overview.status') }}:</td>
						<td data-cy="download-details-dialog-status">{{ downloadTask.status }}</td>
					</tr>
					<tr v-if="downloadTask.fileName">
						<td>{{ t('components.download-details-dialog.overview.file-name') }}:</td>
						<td data-cy="download-details-dialog-file-name">{{ downloadTask.fileName }}</td>
					</tr>
					<tr>
						<td>{{ t('components.download-details-dialog.overview.download-path') }}:</td>
						<td data-cy="download-details-dialog-download-path">{{ downloadTask.downloadDirectory }}</td>
					</tr>
					<tr>
						<td>{{ t('components.download-details-dialog.overview.destination-path') }}:</td>
						<td data-cy="download-details-dialog-destination-path">{{ downloadTask.destinationDirectory }}</td>
					</tr>
					<tr v-if="downloadTask.downloadUrl">
						<td>{{ t('components.download-details-dialog.overview.download-url') }}:</td>
						<td data-cy="download-details-dialog-download-url">
							<q-row no-gutters class="no-wrap">
								<q-col>{{ downloadTask.downloadUrl }}</q-col>
								<q-col cols="auto">
									<ExternalLinkButton :href="downloadTask.downloadUrl" />
								</q-col>
							</q-row>
						</td>
					</tr>
				</tbody>
			</q-markup-table>
			<div v-else>
				<h4>{{ $t('components.download-details-dialog.invalid-download-task.body') }}</h4>
				<ul>
					<li v-for="(error, index) in errors" :key="index">
						{{ error.message }}
					</li>
				</ul>
			</div>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { set } from '@vueuse/core';
import { DownloadTaskDTO, ReasonDTO } from '@dto/mainApi';
import { detailDownloadTask } from '@api/plexDownloadApi';

const { t } = useI18n();

const loading = ref(true);
const downloadTask = ref<DownloadTaskDTO>();
const errors = ref<ReasonDTO[]>([]);
defineProps<{
	name: string;
}>();

function onOpen(downloadTaskId: number) {
	set(loading, true);
	detailDownloadTask(downloadTaskId).subscribe((data) => {
		if (data.isSuccess && data.value) {
			set(downloadTask, data.value);
		} else {
			set(errors, data?.reasons ?? []);
		}
		set(loading, false);
	});
}
</script>
