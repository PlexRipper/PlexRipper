<template>
	<v-col cols="auto">
		<v-lazy
			v-model="isVisible"
			:options="{
				threshold: 0.5,
			}"
			:width="200"
			:height="300"
			transition="fade-transition"
		>
			<v-card :max-width="200">
				<v-img :src="imageUrl" :height="300" :width="200"></v-img>
			</v-card>
		</v-lazy>
	</v-col>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { getThumbnail } from '@api/plexLibraryApi';
import { PlexMediaType, PlexMovieDTO } from '@dto/mainApi';

@Component<MediaPoster>({
	components: {},
})
export default class MediaPoster extends Vue {
	@Prop({ required: true, type: Object as () => PlexMovieDTO })
	readonly movie!: PlexMovieDTO;

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	@Prop({ required: true, type: Number })
	readonly accountId!: number;

	isVisible: boolean = false;
	imageUrl: string = '';

	@Watch('isVisible')
	getThumbnail(): void {
		if (this.isVisible && !this.imageUrl) {
			Log.debug(`${this.movie.title} is visible!`);
			getThumbnail(this.movie.id, this.accountId, this.mediaType).subscribe((response) => {
				this.imageUrl = URL.createObjectURL(response.data);
			});
		}
	}
}
</script>
