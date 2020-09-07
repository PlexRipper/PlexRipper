<template>
	<v-col cols="auto">
		<v-lazy
			v-model="isVisible"
			:options="{
				threshold: 0.5,
			}"
			:width="thumbWidth"
			:height="thumbHeight"
			transition="fade-transition"
		>
			<v-card :max-width="thumbWidth" :width="thumbWidth">
				<v-img :src="imageUrl" :width="thumbWidth" :height="thumbHeight"></v-img>
			</v-card>
		</v-lazy>
	</v-col>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { getThumbnail } from '@api/plexLibraryApi';
import { PlexMediaType } from '@dto/mainApi';

@Component<MediaPoster>({
	components: {},
})
export default class MediaPoster extends Vue {
	@Prop({ required: true, type: Number })
	readonly accountId!: number;

	@Prop({ required: true, type: Number })
	readonly mediaId!: number;

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	private thumbWidth: number = 200;
	private thumbHeight: number = 300;

	isVisible: boolean = false;
	imageUrl: string = '';

	@Watch('isVisible')
	getThumbnail(): void {
		if (this.isVisible && !this.imageUrl) {
			getThumbnail(this.mediaId, this.accountId, this.mediaType, this.thumbWidth, this.thumbHeight).subscribe((response) => {
				this.imageUrl = URL.createObjectURL(response.data);
			});
		}
	}
}
</script>
