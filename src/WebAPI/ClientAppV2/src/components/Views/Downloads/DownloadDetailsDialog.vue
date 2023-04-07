<template>
	<q-card-dialog>
		<template #title>
			<q-media-type-icon :size="36" :media-type="downloadTask.mediaType" />
			<span class="mt-2">
				{{ downloadTask.fullTitle }}
			</span>
		</template>
		<template>
			<q-markup-table v-if="downloadTask" class="section-table">
				<tbody>
					<tr>
						<td style="width: 25%">{{ $t('components.download-details-dialog.overview.status') }}:</td>
						<td>{{ downloadTask.status }}</td>
					</tr>
					<tr>
						<td>{{ $t('components.download-details-dialog.overview.file-name') }}:</td>
						<td>{{ downloadTask.fileName }}</td>
					</tr>
					<tr>
						<td>{{ $t('components.download-details-dialog.overview.download-path') }} :</td>
						<td>{{ downloadTask.downloadDirectory }}</td>
					</tr>
					<tr>
						<td>{{ $t('components.download-details-dialog.overview.destination-path') }} :</td>
						<td>{{ downloadTask.destinationDirectory }}</td>
					</tr>
					<tr>
						<td>{{ $t('components.download-details-dialog.overview.download-url') }} :</td>
						<td>
							<q-row no-gutters class="no-wrap">
								<q-col>
									{{ downloadTask.downloadUrl }}
								</q-col>
								<q-col v-if="downloadTask.downloadUrl" cols="auto">
									<ExternalLinkButton :href="downloadTask.downloadUrl" />
								</q-col>
							</q-row>
						</td>
					</tr>
				</tbody>
			</q-markup-table>
		</template>
	</q-card-dialog>
</template>

<script setup lang="ts">
import { DownloadTaskDTO } from '@dto/mainApi';

const props = defineProps<{
	downloadTask?: DownloadTaskDTO | null;
	dialog: boolean;
}>();
</script>
