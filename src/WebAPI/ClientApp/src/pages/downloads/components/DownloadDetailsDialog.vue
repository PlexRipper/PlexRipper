<template>
	<v-dialog :value="dialog" max-width="800" @click:outside="close">
		<v-card v-if="downloadTask">
			<v-card-title class="headline">
				<media-type-icon class="mx-3" :size="36" :media-type="downloadTask.mediaType" />
				<span class="mt-2">
					{{ downloadTask.fullTitle }}
				</span>
			</v-card-title>

			<v-card-text>
				<v-container>
					<v-row>
						<v-col>
							<v-simple-table class="section-table">
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
										<td>{{ downloadTask.downloadPath }}</td>
									</tr>
									<tr>
										<td>{{ $t('components.download-details-dialog.overview.destination-path') }} :</td>
										<td>{{ downloadTask.destinationFilePath }}</td>
									</tr>
									<tr>
										<td>{{ $t('components.download-details-dialog.overview.download-url') }} :</td>
										<td>
											<v-row no-gutters class="no-wrap">
												<v-col>
													{{ downloadTask.downloadUrl }}
												</v-col>
												<v-col v-if="downloadTask.downloadUrl" cols="auto">
													<external-link :href="downloadTask.downloadUrl" />
												</v-col>
											</v-row>
										</td>
									</tr>
								</tbody>
							</v-simple-table>
						</v-col>
					</v-row>
				</v-container>
			</v-card-text>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { DownloadTaskDTO } from '@dto/mainApi';

@Component
export default class DownloadDetailsDialog extends Vue {
	@Prop({ required: false, type: Object as () => DownloadTaskDTO })
	readonly downloadTask!: DownloadTaskDTO | null;

	@Prop({ required: true, type: Boolean, default: false })
	dialog!: boolean;

	close(): void {
		this.$emit('close');
	}
}
</script>
