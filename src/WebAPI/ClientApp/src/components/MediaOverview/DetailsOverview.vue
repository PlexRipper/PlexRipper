<template>
	<page v-show="isOpen">
		<v-row class="mx-0">
			<media-overview-bar detail-mode :has-selected="true" @download="sendDownloadCommand" />
		</v-row>

		<v-row>
			<v-col cols="12">
				<span>{{ mediaId }}</span>
				<v-btn @click="close"> Close </v-btn>
			</v-col>
		</v-row>
		<v-row v-if="mediaItem">
			{{ mediaItem.id }}
			<v-col>
				<media-table :items="[mediaItem]" :media-type="mediaType" />
			</v-col>
		</v-row>
		<v-row>
			<v-col>
				<loading-spinner />
			</v-col>
		</v-row>
	</page>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import { DownloadMediaDTO, PlexMediaType } from '@dto/mainApi';
import LoadingSpinner from '@components/LoadingSpinner.vue';
import MediaOverviewBar from '@mediaOverview/MediaOverviewBar.vue';

@Component<DetailsOverview>({
	components: {
		MediaTable,
		LoadingSpinner,
		MediaOverviewBar,
	},
})
export default class DetailsOverview extends Vue {
	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ type: Object as () => ITreeViewItem })
	readonly mediaItem!: ITreeViewItem | null;

	isOpen: boolean = false;
	mediaId: number = 0;

	close(): void {
		this.isOpen = false;
		this.$emit('close');
	}

	openDetails(): void {
		this.isOpen = true;
	}

	sendDownloadCommand(downloadMediaCommand: DownloadMediaDTO): void {
		this.$emit('download', downloadMediaCommand);
	}
}
</script>
