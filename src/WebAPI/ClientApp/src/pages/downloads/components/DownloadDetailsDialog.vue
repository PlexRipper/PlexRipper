<template>
	<v-dialog :value="dialog" max-width="800" @click:outside="close">
		<v-card v-if="downloadTask">
			<v-card-title class="headline">
				<v-icon large class="mx-3">{{ downloadTask.mediaType | mediaTypeIcon }}</v-icon>
				{{ downloadTask.title }}
			</v-card-title>

			<v-card-text>
				<v-container>
					<v-row>
						<v-col>
							<v-simple-table class="section-table">
								<tbody>
									<tr>
										<td style="width: 25%">Status:</td>
										<td>{{ downloadTask.status }}</td>
									</tr>
									<tr>
										<td>File Name:</td>
										<td>{{ downloadTask.fileName }}</td>
									</tr>
									<tr>
										<td>Download Path:</td>
										<td>{{ downloadTask.downloadPath }}</td>
									</tr>
									<tr>
										<td>Destination Path:</td>
										<td>{{ downloadTask.destinationPath }}</td>
									</tr>
									<tr>
										<td>Download URL:</td>
										<td>
											<v-row no-gutters>
												<v-col>
													{{ downloadTask.downloadUrl }}
												</v-col>
												<v-col cols="auto">
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
import ExternalLink from '@components/General/ExternalLink.vue';

@Component<DownloadDetailsDialog>({
	components: {
		ExternalLink,
	},
})
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
