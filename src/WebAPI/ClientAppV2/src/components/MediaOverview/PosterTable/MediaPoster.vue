<template>
	<q-intersection transition="fade" class="q-ma-md media-poster" :threshold="0.1" @visibility="isVisibleChanged">
		<div class="highlight-border-box">
			<q-hover>
				<template #default="{ hover }">
					<q-card>
						<q-img
							loading="eager"
							:no-transition="!imageUrl"
							:src="imageUrl"
							fit="fill"
							no-spinner
							class="poster-image"
							:alt="mediaItem.title">
							<!--	Placeholder	-->
							<!--						<template #loading>-->
							<!--							&lt;!&ndash;	Show fallback image	&ndash;&gt;-->
							<!--							<template v-if="defaultImage">-->
							<!--								<q-row align="center" justify="center" class="fill-height">-->
							<!--									<q-col cols="auto">-->
							<!--										<q-media-type-icon class="mx-3" :size="100" :media-type="mediaType" />-->
							<!--									</q-col>-->
							<!--									<q-col cols="12">-->
							<!--										<h4 class="text-center">{{ mediaItem.title }}</h4>-->
							<!--									</q-col>-->
							<!--								</q-row>-->
							<!--							</template>-->
							<!--							&lt;!&ndash;	Show  image	&ndash;&gt;-->
							<!--							<template v-else>-->
							<!--								<q-row class="fill-height ma-0" align="center" justify="center">-->
							<!--									<q-col cols="12">-->
							<!--										<h4 class="text-center">{{ mediaItem.title }}</h4>-->
							<!--									</q-col>-->
							<!--									<q-col cols="auto">-->
							<!--										<q-circular-progress color="grey lighten-5" />-->
							<!--									</q-col>-->
							<!--								</q-row>-->
							<!--							</template>-->
							<!--						</template>-->
							<!--	Overlay	-->
							<div :class="['poster-overlay', hover ? 'on-hover' : '', 'white--text']">
								<q-row justify="center" align="end" style="height: 100%">
									<q-col cols="12" class="text-center">
										<span class="poster-overlay-title">
											{{ mediaItem.title }}
										</span>
									</q-col>
									<q-col cols="auto">
										<q-btn v-if="isMovieType" icon="mdi-download" size="xl" @click="downloadMedia()" />
										<q-btn v-if="isTvShowType" icon="mdi-magnify" size="xl" @click="openDetails()" />
									</q-col>
								</q-row>
							</div>
						</q-img>
						<!--	Poster bar	-->
						<q-row justify="center" no-gutters>
							<q-col cols="auto">
								<q-chip
									v-for="item in mediaItem.mediaData"
									:key="item.id"
									class="my-2"
									:color="getQualityColor(item.videoResolution)"
									size="md">
									{{ getQualityString(item.videoResolution) }}
								</q-chip>
							</q-col>
						</q-row>
					</q-card>
				</template>
			</q-hover>
			<!--	Highlight animation effect	-->
			<svg class="glow-container">
				<!--suppress HtmlUnknownAttribute -->
				<rect pathLength="100" height="5" width="5" stroke-linecap="round" class="glow-blur" />
				<!--suppress HtmlUnknownAttribute -->
				<rect pathLength="100" height="5" width="5" stroke-linecap="round" class="glow-line" />
			</svg>
		</div>
	</q-intersection>
</template>

<script setup lang="ts">
import { ref, computed, defineProps, defineEmits } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { MediaService } from '@service';

const props = defineProps<{
	mediaItem: PlexMediaDTO;
	mediaType: PlexMediaType;
}>();

const emit = defineEmits<{
	(e: 'open-details', mediaId: number): void;
	(e: 'download', downloadMediaCommands: DownloadMediaDTO[]): void;
}>();

const thumbWidth = ref(200);
const thumbHeight = ref(300);

const isVisible = ref(false);
const imageUrl = ref('');
const defaultImage = ref(false);

const isMovieType = computed(() => props.mediaType === PlexMediaType.Movie);
const isTvShowType = computed(() => props.mediaType === PlexMediaType.TvShow);

const openDetails = () => {
	emit('open-details', props.mediaItem.id);
};

const getQualityColor = (quality: string): string => {
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
};

const getQualityString = (quality: string): string => {
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
		case '2160':
			return '4k';
		default:
			return quality;
	}
};

const isVisibleChanged = (visibilty: boolean) => {
	isVisible.value = visibilty;

	if (!props.mediaItem.hasThumb) {
		defaultImage.value = true;
		return;
	}

	if (isVisible.value && !imageUrl.value) {
		useSubscription(
			MediaService.getThumbnail(props.mediaItem.id, props.mediaType, thumbWidth.value, thumbHeight.value).subscribe({
				next: (data) => {
					if (!data) {
						defaultImage.value = true;
						return;
					}
					imageUrl.value = data;
				},
				error: () => {
					defaultImage.value = true;
				},
			}),
		);
	}
};

const downloadMedia = () => {
	const downloadCommand: DownloadMediaDTO = {
		type: props.mediaType,
		mediaIds: [props.mediaItem.id],
		plexLibraryId: props.mediaItem.plexLibraryId,
		plexServerId: props.mediaItem.plexServerId,
	};

	emit('download', [downloadCommand]);
};
</script>

<style lang="scss">
.media-poster {
	width: 200px;
	height: 340px;

	.poster-image {
		height: 300px;
	}
}

.poster-overlay {
	align-items: center;
	justify-content: center;
	position: absolute;
	height: 100%;
	width: 100%;
	background-color: rgba(0, 0, 0, 0);
	opacity: 0;
	transition: opacity 0.2s ease-in-out;

	.poster-overlay-title {
		font-size: 1.5em;
		font-weight: bold;
	}

	&.on-hover {
		background-color: rgba(0, 0, 0, 0.8);
		opacity: 0.8;

		.q-btn {
			opacity: 1;
		}
	}
}
</style>
