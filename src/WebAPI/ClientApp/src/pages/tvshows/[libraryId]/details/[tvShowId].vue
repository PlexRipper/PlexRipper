<template>
	<QPage>
		<template v-if="!loading && mediaItemDetail">
			<!--	Overview bar	-->
			<MediaOverviewBar
				:media-type="mediaItemDetail.type"
				:library-id="libraryId"
				:media-detail-item="mediaItemDetail"
				detail-mode
				@action="onAction" />
			<QScroll class="page-content-minus-media-overview-bar">
				<!--	Header	-->
				<QRow>
					<QCol
						cols="auto">
						<!--	Poster	-->
						<MediaPosterImage
							class="q-ma-md"
							:thumb-width="thumbWidth"
							:thumb-height="thumbHeight"
							:actions="false"
							:media-item="mediaItemDetail" />
					</QCol>
					<QCol>
						<q-card
							class="media-info-container"
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
			</QScroll>
		</template>

		<!-- Download confirmation dialog	-->
		<DownloadConfirmation @download="downloadStore.downloadMedia($event)" />

		<QLoadingOverlay :loading="loading" />
	</QPage>
</template>

<script setup lang="ts">
import { get, set } from '@vueuse/core';
import { forkJoin } from 'rxjs';
import { take } from 'rxjs/operators';
import { type PlexMediaDTO, PlexMediaType } from '@dto';
import { useRouter } from 'vue-router';
import type { IMediaOverviewBarActions } from '@interfaces';
import {
	definePageMeta, listenMediaOverviewDownloadCommand, useDialogStore, useDownloadStore,
	useI18n,
	useMediaOverviewStore,
	useMediaStore, useSettingsStore,
	useSubscription,
} from '#imports';

definePageMeta({
	scrollToTop: false,
});

const route = useRoute();

const mediaStore = useMediaStore();
const dialogStore = useDialogStore();
const mediaOverviewStore = useMediaOverviewStore();
const downloadStore = useDownloadStore();
const settingsStore = useSettingsStore();

const router = useRouter();

const { t } = useI18n();
const loading = ref(true);
const mediaItemDetail = ref<PlexMediaDTO | null>(null);
const thumbWidth = ref(200);
const thumbHeight = ref(300);
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

const libraryId = computed(() => +route.params.libraryId);
const mediaId = computed(() => +route.params.tvShowId);

function onAction(event: IMediaOverviewBarActions) {
	if (event === 'back') {
		router.go(-1);
	}
}

listenMediaOverviewDownloadCommand((command) => {
	const type: PlexMediaType = command[0].type;
	if (settingsStore.isConfirmationEnabled(type)) {
		dialogStore.openMediaConfirmationDownloadDialog(command);
	} else {
		downloadStore.downloadMedia(command);
	}
});

onMounted(() => {
	const type = PlexMediaType.TvShow;

	mediaOverviewStore.downloadButtonVisible = false;
	mediaOverviewStore.isDetailView = true;

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
@use '@/assets/scss/_mixins.scss';

.media-info-container {
  @extend .background-sm;
  box-shadow: 0px 3px 1px -2px rgba(0, 0, 0, 0.2), 0px 2px 2px 0px rgba(0, 0, 0, 0.14),
  0px 1px 5px 0px rgba(0, 0, 0, 0.12);

  margin-right: 1rem;

  .media-title {
    font-size: 30px;
    font-weight: bold;
  }

  .media-info-column {
    min-width: 150px;
    text-align: left;
    white-space: pre-wrap;
  }
}
</style>
