<template>
	<q-card-dialog :name="name" class="media-details-dialog" maximized seamless @opened="openDetails" @closed="closeDetails">
		<template #default>
			<q-col>
				<!--	Header	-->
				<q-row>
					<q-col cols="auto">
						<q-card class="q-ma-md media-info-container">
							<!--	Poster	-->
							<q-img :src="imageUrl" fit="fill" :width="`${thumbWidth}px`" :height="`${thumbHeight}px`" ratio="2/3">
								<!--	Placeholder	-->
								<template #loading>
									<!--	Show fallback image	-->
									<q-row align="center" justify="center" class="fill-height">
										<q-col cols="auto">
											<q-media-type-icon
												:size="100"
												class="mx-3"
												:media-type="mediaItem?.type ?? PlexMediaType.Unknown" />
										</q-col>
										<q-col cols="12">
											<h4 class="text-center">{{ mediaItem?.title ?? 'unknown' }}</h4>
										</q-col>
									</q-row>
								</template>
							</q-img>
						</q-card>
					</q-col>
					<q-col>
						<q-card class="q-ma-md media-info-container" :style="{ height: thumbHeight + 'px' }">
							<!-- Media info-->
							<q-card-section>
								<q-markup-table wrap-cells>
									<tbody>
										<tr class="q-tr--no-hover">
											<td colspan="2" class="media-info-column media-title">
												{{ mediaItem?.title ?? 'unknown' }}
											</td>
										</tr>
										<tr>
											<td class="media-info-column">
												{{ $t('components.details-overview.total-duration') }}
											</td>
											<td class="media-info-column">
												<q-duration :value="mediaItem?.duration ?? -1" />
											</td>
										</tr>
										<tr>
											<td class="media-info-column">
												{{ $t('components.details-overview.media-count-label') }}
											</td>
											<td class="media-info-column">{{ mediaCountFormatted }}</td>
										</tr>
										<tr>
											<td class="media-info-column">{{ $t('components.details-overview.summary') }}</td>
											<td class="media-info-column">{{ mediaItem?.summary ?? '' }}</td>
										</tr>
									</tbody>
								</q-markup-table>
							</q-card-section>
						</q-card>
					</q-col>
				</q-row>

				<!--	Media Table	-->
				<q-row no-gutters>
					<q-col>
						<MediaList :media-item="mediaItem" />
					</q-col>
				</q-row>
				<!--	Loading overlay	-->
				<QLoadingOverlay :loading="loading" />
			</q-col>
		</template>
	</q-card-dialog>
</template>

<script setup lang="ts">
import { computed, defineEmits, defineProps, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import Log from 'consola';
import sum from 'lodash-es/sum';
import { get, set } from '@vueuse/core';
import { PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { MediaList } from '#components';
import { MediaService } from '@service';
import { useI18n, useMediaOverviewBarBus } from '#imports';

const emit = defineEmits<{
	(e: 'media-item', mediaItem: PlexMediaDTO | null): void;
	(e: 'close'): void;
}>();

defineProps<{
	name: string;
}>();

const t = useI18n().t;
const loading = ref(false);
const mediaItem = ref<PlexMediaDTO | null>(null);
const thumbWidth = ref(180);
const thumbHeight = ref(270);
const defaultImage = ref(false);
const imageUrl = ref('');

const mediaCountFormatted = computed(() => {
	const item = get(mediaItem);
	if (item) {
		switch (item.type) {
			case PlexMediaType.Movie:
				return `1 Movie`;
			case PlexMediaType.TvShow:
				return t('components.details-overview.media-count', {
					seasonCount: item.childCount,
					episodeCount: sum(item.children?.map((x) => x.childCount)),
				});
			default:
				return `Library type ${item.type} is not supported in the media count`;
		}
	}

	return 'unknown media count';
});

function openDetails({ mediaId, type }: { mediaId: number; type: PlexMediaType }) {
	set(loading, true);

	useMediaOverviewBarBus().emit({ downloadButtonVisible: false });
	useSubscription(
		MediaService.getMediaDataById(mediaId, type).subscribe((data) => {
			mediaItem.value = data;
			emit('media-item', data);
			set(loading, false);
		}),
	);

	useSubscription(
		MediaService.getThumbnail(mediaId, type, thumbWidth.value, thumbHeight.value).subscribe({
			next: (data) => {
				Log.info('data', data);
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

function closeDetails() {
	set(mediaItem, null);
}
</script>
<style lang="scss">
@import '@/assets/scss/variables.scss';

.media-details-dialog {
	.q-dialog__inner {
		height: calc(100vh - $app-bar-height - $media-overview-bar-height);
		transition: all 0.12s ease;

		top: auto !important;
		left: auto !important;
		bottom: 0 !important;
		right: 0 !important;
	}
}

body {
	// Disable transitions animation when manually resizing the window
	&.window-resizing {
		.q-dialog__inner {
			transition: none !important;
		}
	}

	&.navigation-drawer-closed {
		.q-dialog__inner {
			width: 100vw !important;
		}
	}

	&.navigation-drawer-opened {
		.q-dialog__inner {
			width: calc(100vw - $navigation-drawer-width);
		}
	}
}
</style>
