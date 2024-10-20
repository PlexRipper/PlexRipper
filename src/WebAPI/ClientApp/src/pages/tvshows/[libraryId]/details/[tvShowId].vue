<template>
	<QPage>
		<!--	Overview bar	-->
		<MediaOverviewBar
			:server="libraryStore.getServerByLibraryId(libraryId)"
			:library="libraryStore.getLibrary(libraryId)"
			:detail-mode="true"
			@back="onBack" />
		<!--	Header	-->
		<QRow>
			<QCol cols="auto">
				<q-card class="q-ma-md media-info-container">
					<!--	Poster	-->
					<q-img
						:src="imageUrl"
						fit="fill"
						:width="`${thumbWidth}px`"
						:height="`${thumbHeight}px`"
						ratio="2/3">
						<!--	Placeholder	-->
						<template #loading>
							<!--	Show fallback image	-->
							<QRow
								align="center"
								justify="center"
								class="fill-height">
								<QCol cols="auto">
									<QMediaTypeIcon
										:size="100"
										class="mx-3"
										:media-type="mediaItemDetail?.type ?? PlexMediaType.Unknown" />
								</QCol>
								<QCol cols="12">
									<h4 class="text-center">
										{{ mediaItemDetail?.title ?? 'unknown' }}
									</h4>
								</QCol>
							</QRow>
						</template>
					</q-img>
				</q-card>
			</QCol>
			<QCol>
				<q-card
					class="q-ma-md media-info-container"
					:style="{ height: thumbHeight + 'px' }">
					<!-- Media info -->
					<q-card-section>
						<q-markup-table wrap-cells>
							<tbody>
								<tr class="q-tr--no-hover">
									<td
										colspan="2"
										class="media-info-column media-title">
										{{ mediaItemDetail?.title ?? 'unknown' }}
									</td>
								</tr>
								<tr>
									<td class="media-info-column">
										{{ t('components.details-overview.total-duration') }}
									</td>
									<td class="media-info-column">
										<QDuration :value="mediaItemDetail?.duration ?? -1" />
									</td>
								</tr>
								<tr>
									<td class="media-info-column">
										{{ t('components.details-overview.media-count-label') }}
									</td>
									<td class="media-info-column">
										{{ mediaCountFormatted }}
									</td>
								</tr>
								<tr>
									<td class="media-info-column">
										{{ t('components.details-overview.summary') }}
									</td>
									<td class="media-info-column">
										{{ mediaItemDetail?.summary ?? '' }}
									</td>
								</tr>
							</tbody>
						</q-markup-table>
					</q-card-section>
				</q-card>
			</QCol>
		</QRow>

		<!--	Media Table	-->
		<QRow no-gutters>
			<QCol>
				<MediaList
					use-q-table
					:media-item="mediaItemDetail"
					disable-intersection
					disable-highlight />
			</QCol>
		</QRow>
	</QPage>
</template>

<script setup lang="ts">
import Log from 'consola';
import { get, set } from '@vueuse/core';
import { forkJoin } from 'rxjs';
import { take } from 'rxjs/operators';
import { type PlexMediaDTO, PlexMediaType } from '@dto';
import { useRouter } from 'vue-router';
import {
	definePageMeta,
	useI18n,
	useLibraryStore,
	useMediaOverviewStore,
	useMediaStore,
} from '#imports';

definePageMeta({
	scrollToTop: false,
});

const route = useRoute();

const mediaStore = useMediaStore();
const mediaOverviewStore = useMediaOverviewStore();
const libraryStore = useLibraryStore();
const router = useRouter();

const { t } = useI18n();
const loading = ref(true);
const mediaItemDetail = ref<PlexMediaDTO | null>(null);
const thumbWidth = ref(180);
const thumbHeight = ref(270);
const defaultImage = ref(false);

const mediaCountFormatted = computed(() => {
	const item = get(mediaItemDetail);
	if (item) {
		switch (item.type) {
			case PlexMediaType.Movie:
				return t('components.details-overview.one-movie-count');
			case PlexMediaType.TvShow:
				return t('components.details-overview.media-count', {
					seasonCount: item.childCount,
					episodeCount: sum(item.children?.map((x) => x.childCount)),
				});
			default:
				throw new Error(`Library type ${item.type} is not supported in the media count`);
		}
	}

	return 'unknown media count';
});

const imageUrl = computed(() => {
	return get(mediaItemDetail)?.hasThumb
		? `${get(mediaItemDetail)?.fullThumbUrl}&width=${get(thumbWidth)}&height=${get(thumbHeight)}`
		: '';
});

const libraryId = computed(() => +route.params.libraryId);
const mediaId = computed(() => +route.params.tvShowId);

function onBack() {
	router.push({
		name: 'tvshows-libraryId',
		params: {
			libraryId: get(libraryId),
		},
	});
}

onMounted(() => {
	const type = PlexMediaType.TvShow;

	mediaOverviewStore.downloadButtonVisible = false;

	useSubscription(
		forkJoin({
			mediaDetail: mediaStore.getMediaDataDetailById(get(mediaId), type),
		})
			.pipe(take(1))
			.subscribe({
				next: ({ mediaDetail }) => {
					// Media detail
					set(mediaItemDetail, mediaDetail);
					mediaOverviewStore.lastMediaItemViewed = mediaDetail;
				},
				error: () => {
					set(defaultImage, true);
				},
				complete: () => {
					set(loading, false);
				},
			}),
	);
});

onBeforeUnmount(() => {
	set(mediaItemDetail, null);
	set(loading, true);
	mediaOverviewStore.downloadButtonVisible = false;
});
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
    .media-details-dialog {
      .q-dialog__inner {
        transition: none !important;
      }
    }
  }

  &.navigation-drawer-closed {
    .media-details-dialog {
      .q-dialog__inner {
        width: 100vw !important;
      }
    }
  }

  &.navigation-drawer-opened {
    .media-details-dialog {
      .q-dialog__inner {
        width: calc(100vw - $navigation-drawer-width);
      }
    }
  }
}
</style>
