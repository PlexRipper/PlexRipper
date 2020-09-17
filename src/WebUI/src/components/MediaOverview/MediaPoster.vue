<template>
	<v-col cols="auto">
		<v-lazy
			v-model="isVisible"
			:options="{
				threshold: 0.1,
			}"
			:width="thumbWidth"
			:height="thumbHeight"
			transition="fade-transition"
		>
			<v-hover v-slot:default="{ hover }">
				<v-card :max-width="thumbWidth" :width="thumbWidth" :elevation="hover ? 12 : 2">
					<v-img :src="imageUrl" :width="thumbWidth" :height="thumbHeight" :alt="title">
						<!--	Placeholder	-->
						<template v-slot:placeholder>
							<v-row class="fill-height ma-0" align="center" justify="center">
								<v-col cols="12">
									<h4 class="text-center">{{ title }}</h4>
								</v-col>
								<v-col cols="auto">
									<v-progress-circular indeterminate color="grey lighten-5" />
								</v-col>
							</v-row>
						</template>
						<!--	Overlay	-->
						<v-container fluid :class="$classNames('poster-overlay', { 'on-hover': hover }, 'white--text')">
							<v-row justify="center" align="end" style="height: 100%">
								<v-col cols="auto">
									<v-btn icon large @click="downloadMedia()">
										<v-icon large> mdi-download </v-icon>
									</v-btn>
								</v-col>
							</v-row>
						</v-container>
					</v-img>
				</v-card>
			</v-hover>
		</v-lazy>
	</v-col>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { getThumbnail } from '@api/plexLibraryApi';
import { PlexMediaType } from '@dto/mainApi';
import IMediaId from '@mediaOverview/MediaTable/types/IMediaId';

@Component<MediaPoster>({
	components: {},
})
export default class MediaPoster extends Vue {
	@Prop({ required: true, type: Number })
	readonly accountId!: number;

	@Prop({ required: true, type: Number })
	readonly mediaId!: number;

	@Prop({ required: true, type: String })
	readonly title!: number;

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

	downloadMedia(): void {
		this.$emit('download', { id: this.mediaId, type: this.mediaType } as IMediaId);
	}
}
</script>
