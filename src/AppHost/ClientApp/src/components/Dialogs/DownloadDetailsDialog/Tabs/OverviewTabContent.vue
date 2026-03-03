<template>
	<q-markup-table
		v-if="downloadTask"
		class="section-table">
		<tbody>
			<tr>
				<td style="width: 25%">
					{{ t('components.download-details-dialog.overview.status') }}
				</td>
				<td data-cy="download-details-dialog-status">
					{{ downloadTask.status }}
				</td>
			</tr>
			<tr v-if="downloadTask.fileName">
				<td>{{ t('components.download-details-dialog.overview.file-name') }}</td>
				<td data-cy="download-details-dialog-file-name">
					{{ downloadTask.fileName }}
				</td>
			</tr>
			<tr>
				<td>{{ t('components.download-details-dialog.overview.download-path') }}</td>
				<td data-cy="download-details-dialog-download-path">
					{{ downloadTask.downloadDirectory }}
				</td>
			</tr>
			<tr>
				<td>{{ t('components.download-details-dialog.overview.destination-path') }}</td>
				<td data-cy="download-details-dialog-destination-path">
					{{ downloadTask.destinationDirectory }}
				</td>
			</tr>
			<tr v-if="downloadTask.downloadUrl">
				<td>{{ t('components.download-details-dialog.overview.download-url') }}</td>
				<td data-cy="download-details-dialog-download-url">
					<QRow
						class="no-wrap"
						no-gutters>
						<QCol>{{ downloadTask.downloadUrl }}</QCol>
						<QCol cols="auto">
							<ExternalLinkButton :href="downloadTask.downloadUrl" />
						</QCol>
					</QRow>
				</td>
			</tr>
		</tbody>
	</q-markup-table>
	<div v-else>
		<h4>{{ $t('components.download-details-dialog.invalid-download-task.body') }}</h4>
		<ul>
			<li
				v-for="(error, index) in errors"
				:key="index">
				{{ error.message }}
			</li>
		</ul>
	</div>
</template>

<script lang="ts" setup>
import type { DownloadTaskDTO, ErrorDTO } from '@dto';

const { t } = useI18n();

defineProps<{
	downloadTask: DownloadTaskDTO | undefined;
	errors: ErrorDTO[];
}>();
</script>
