<template>
	<v-lazy
		v-model="isVisible"
		:options="{
			threshold: 0.1,
		}"
		:width="thumbWidth"
		:height="getLazyLoadingHeight"
		:data-title="mediaItem.title"
		transition="fade-transition"
		class="mx-3"
	>
		<v-col cols="auto">
			<v-hover v-slot="{ hover }">
				<v-card :max-width="thumbWidth" :width="thumbWidth" :elevation="hover ? 12 : 2">
					<v-img :src="imageUrl" :width="thumbWidth" :height="thumbHeight" :alt="mediaItem.title">
						<!--	Placeholder	-->
						<template #placeholder>
							<!--	Show fallback image	-->
							<template v-if="defaultImage">
								<v-row align="center" justify="center" class="fill-height">
									<v-col cols="auto">
										<v-icon class="mx-3" style="font-size: 100px">{{ mediaType | mediaTypeIcon }}</v-icon>
									</v-col>
									<v-col cols="12">
										<h4 class="text-center">{{ mediaItem.title }}</h4>
									</v-col>
								</v-row>
							</template>
							<!--	Show  image	-->
							<template v-else>
								<v-row class="fill-height ma-0" align="center" justify="center">
									<v-col cols="12">
										<h4 class="text-center">{{ mediaItem.title }}</h4>
									</v-col>
									<v-col cols="auto">
										<v-progress-circular indeterminate color="grey lighten-5" />
									</v-col>
								</v-row>
							</template>
						</template>
						<!--	Overlay	-->
						<v-container fluid :class="['poster-overlay', hover ? 'on-hover' : '', 'white--text']">
							<v-row justify="center" align="end" style="height: 100%">
								<v-col cols="12" class="text-center">
									<h2>
										{{ mediaItem.title }}
									</h2>
								</v-col>
								<v-col cols="auto">
									<v-btn icon large @click="downloadMedia()">
										<v-icon large> mdi-download </v-icon>
									</v-btn>
								</v-col>
							</v-row>
						</v-container>
					</v-img>
					<v-row justify="center" no-gutters>
						<v-col cols="auto">
							<v-chip
								v-for="item in mediaItem.mediaData"
								:key="item.id"
								class="my-2"
								:color="getQualityColor(item.videoResolution)"
								text
								small
							>
								{{ getQualityString(item.videoResolution) }}
							</v-chip>
						</v-col>
					</v-row>
				</v-card>
			</v-hover>
		</v-col>
	</v-lazy>
</template>

<script lang="ts">
import { Component, Prop, Vue, Watch } from 'vue-property-decorator';
import { getThumbnail } from '@api/plexLibraryApi';
import { DownloadMediaDTO, PlexMediaType } from '@dto/mainApi';
import type ITreeViewItem from '@mediaOverview/MediaTable/types/ITreeViewItem';

@Component
export default class MediaPoster extends Vue {
	@Prop({ required: true, type: Object as () => ITreeViewItem })
	readonly mediaItem!: ITreeViewItem;

	@Prop({ required: true, type: String })
	readonly mediaType!: PlexMediaType;

	private thumbWidth: number = 200;
	private thumbHeight: number = 300;

	isVisible: boolean = false;
	imageUrl: string = '';
	defaultImage: boolean = false;
	get getLazyLoadingHeight(): number {
		return this.thumbHeight + 40;
	}

	get hover(): boolean {
		return true;
	}

	getQualityColor(quality: string): string {
		switch (quality) {
			case 'sd':
				return 'brown darken-4';
			case '480':
				return 'deep-orange';
			case '576':
				return 'yellow darken-1';
			case '720':
				return 'lime accent-4';
			case '1080':
				return 'blue accent-3';
			case '4k':
				return 'red darken-4';
			default:
				return 'white';
		}
	}

	getQualityString(quality: string): string {
		switch (quality) {
			case '480':
				return '480p';
			case '576':
				return '576p';
			case '720':
				return '720p';
			case '1080':
				return '1080p';
			case '1440':
				return '1440p';
			default:
				return quality;
		}
	}

	@Watch('isVisible')
	getThumbnail(): void {
		if (this.isVisible && !this.imageUrl) {
			getThumbnail(this.mediaItem.id, this.mediaType, this.thumbWidth, this.thumbHeight).subscribe((response) => {
				if (response.status === 204) {
					// this.imageUrl = require('@img/logo/full-logo-256.png');
					this.defaultImage = true;
					return;
				}
				this.imageUrl = URL.createObjectURL(response.data);
			});
		}
	}

	downloadMedia(): void {
		const downloadCommand: DownloadMediaDTO = {
			type: this.mediaType,
			mediaIds: [this.mediaItem.id],
			libraryId: 0,
			plexAccountId: 0,
		};

		this.$emit('download', downloadCommand);
	}
}
</script>
