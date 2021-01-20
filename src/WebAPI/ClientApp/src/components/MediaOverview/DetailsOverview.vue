<template>
	<v-overlay :value="isOpen" opacity="0.8" absolute>
		<v-row>
			<v-col cols="12">
				<span>{{ mediaId }}</span>
				<v-btn @click="close"> Close </v-btn>
			</v-col>
		</v-row>
		<v-row v-if="mediaItem">
			<v-col>
				<media-table :items="[mediaItem]" :media-type="mediaType" />
			</v-col>
		</v-row>
		<v-row>
			<v-col>
				<loading-spinner />
			</v-col>
		</v-row>
	</v-overlay>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';
import { PlexMediaType } from '@dto/mainApi';
import LoadingSpinner from '@components/LoadingSpinner.vue';

@Component<DetailsOverview>({
	components: {
		MediaTable,
		LoadingSpinner,
	},
})
export default class DetailsOverview extends Vue {
	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ required: true, type: (Object as () => ITreeViewItem) | null })
	readonly mediaItem!: ITreeViewItem | null;

	isOpen: boolean = false;
	mediaId: number = 0;

	close(): void {
		this.isOpen = false;
		this.$emit('close');
	}

	openDetails(): void {
		// this.mediaItem = mediaItem;
		this.isOpen = true;
	}
}
</script>
