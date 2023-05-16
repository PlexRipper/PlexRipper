<template>
	<q-intersection transition="fade" class="q-ma-md media-poster" :threshold="0.1" @visibility="isVisibleChanged">
		<q-card flat class="media-poster--card highlight-border-box">
			<div>
				<q-hover>
					<template #default="{ hover }">
						<q-img
							v-if="!defaultImage"
							loading="eager"
							:src="imageUrl"
							fit="fill"
							no-spinner
							class="media-poster--image"
							:alt="mediaItem.title">
							<!--	Overlay	-->
							<div :class="['media-poster--overlay', hover ? 'on-hover' : '', 'white--text']">
								<q-row justify="center" align="end" style="height: 100%">
									<q-col cols="12" text-align="center">
										<span class="media-poster--title">
											{{ mediaItem.title }}
										</span>
									</q-col>
									<q-col cols="auto">
										<BaseButton
											v-if="mediaType === PlexMediaType.Movie"
											icon="mdi-download"
											size="xl"
											flat
											:outline="false"
											@click="downloadMedia()" />
										<BaseButton
											v-if="mediaType === PlexMediaType.TvShow"
											icon="mdi-magnify"
											:outline="false"
											size="xl"
											flat
											@click="sendMediaOverviewOpenDetailsCommand(mediaItem.id)" />
									</q-col>
								</q-row>
							</div>
						</q-img>
						<!--	Show fallback image	-->
						<template v-else>
							<q-row column align="center" justify="between" class="media-poster--fallback">
								<q-col>
									<q-media-type-icon class="mx-3" :size="90" :media-type="mediaType" />
								</q-col>
								<q-col text-align="center">
									<span class="media-poster--title">
										{{ mediaItem.title }}
									</span>
								</q-col>

								<q-col cols="auto">
									<BaseButton
										v-if="mediaType === PlexMediaType.Movie"
										icon="mdi-download"
										:outline="false"
										size="xl"
										flat
										@click="downloadMedia()" />
									<BaseButton
										v-if="mediaType === PlexMediaType.TvShow"
										icon="mdi-magnify"
										:outline="false"
										size="xl"
										flat
										@click="sendMediaOverviewOpenDetailsCommand(mediaItem.id)" />
								</q-col>
							</q-row>
						</template>
					</template>
				</q-hover>
			</div>
			<!--	Poster bar	-->
			<div v-if="qualities.length" class="media-poster--quality-bar">
				<q-chip v-for="(quality, j) in qualities" :key="j" :color="getQualityColor(quality.quality)" size="md">
					{{ quality.displayQuality }}
				</q-chip>
			</div>
			<QLoadingOverlay :loading="loading" />
			<!--	Highlight animation effect	-->
			<svg class="glow-container">
				<!--suppress HtmlUnknownAttribute -->
				<rect pathLength="100" height="5" width="5" stroke-linecap="round" class="glow-blur" />
				<!--suppress HtmlUnknownAttribute -->
				<rect pathLength="100" height="5" width="5" stroke-linecap="round" class="glow-line" />
			</svg>
		</q-card>
	</q-intersection>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { DownloadMediaDTO, PlexMediaSlimDTO, PlexMediaType } from '@dto/mainApi';
import { MediaService } from '@service';
import { sendMediaOverviewOpenDetailsCommand } from '@composables/event-bus';

const props = defineProps<{
	mediaItem: PlexMediaSlimDTO;
	index: number;
}>();

const emit = defineEmits<{
	(e: 'download', downloadMediaCommands: DownloadMediaDTO[]): void;
}>();

const thumbWidth = ref(200);
const thumbHeight = ref(300);

const isVisible = ref(false);
const imageUrl = ref('');
const defaultImage = ref(false);
const loading = ref(false);
const mediaType = computed(() => props.mediaItem?.type ?? PlexMediaType.Unknown);
const qualities = computed(() => props.mediaItem?.qualities ?? []);

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

function isVisibleChanged(visibility: boolean) {
	set(isVisible, visibility);
	// set(defaultImage, true);
	// return;

	if (!props.mediaItem.hasThumb) {
		set(defaultImage, true);
		return;
	}

	if (get(isVisible) && !get(imageUrl)) {
		useSubscription(
			MediaService.getThumbnail(props.mediaItem.id, get(mediaType), get(thumbWidth), get(thumbHeight)).subscribe({
				next: (data) => {
					if (!data) {
						set(defaultImage, true);
						return;
					}
					set(imageUrl, data);
				},
				error: () => {
					set(defaultImage, true);
				},
				complete: () => {
					set(loading, false);
				},
			}),
		);
	}
}

function downloadMedia() {
	const downloadCommand: DownloadMediaDTO = {
		type: get(mediaType),
		mediaIds: [props.mediaItem.id],
		plexLibraryId: props.mediaItem.plexLibraryId,
		plexServerId: props.mediaItem.plexServerId,
	};

	emit('download', [downloadCommand]);
}
</script>

<style lang="scss">
@import '@/assets/scss/_mixins.scss';

.media-poster {
	@extend .background-sm;

	width: 200px;
	height: 340px;

	&--card {
		width: 200px;
		height: 340px;
	}

	&--image,
	&--fallback {
		width: 200px;
		height: 300px;
	}

	&--fallback {
		& > div {
			margin: 16px 0;
		}
	}

	&--title {
		font-size: 1.5em;
		font-weight: bold;
		text-align: center;
	}

	&--overlay {
		@extend .background-xl;
		width: 100%;
		height: 100%;
		opacity: 0;
		transition: opacity 0.2s ease-in-out;

		&.on-hover {
			opacity: 0.8;

			.q-btn {
				opacity: 1;
			}
		}
	}

	&--quality-bar {
		height: 40px;
		display: flex;
		justify-content: center;
		align-items: center;
	}
}
</style>
