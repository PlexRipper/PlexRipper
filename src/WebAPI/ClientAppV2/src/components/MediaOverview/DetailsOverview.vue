<template>
	<template v-if="mediaItem">
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
									<q-media-type-icon :size="100" class="mx-3" :media-type="mediaItem.type" />
								</q-col>
								<q-col cols="12">
									<h4 class="text-center">{{ mediaItem.title }}</h4>
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
									<td colspan="2" class="media-info-column media-title">{{ mediaItem.title }}</td>
								</tr>
								<tr>
									<td class="media-info-column">{{ $t('components.details-overview.total-duration') }}</td>
									<td class="media-info-column">
										<q-duration :value="mediaItem.duration" />
									</td>
								</tr>
								<tr>
									<td class="media-info-column">{{ $t('components.details-overview.media-count-label') }}</td>
									<td class="media-info-column">{{ mediaCountFormatted }}</td>
								</tr>
								<tr>
									<td class="media-info-column">{{ $t('components.details-overview.summary') }}</td>
									<td class="media-info-column">{{ mediaItem.summary }}</td>
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
	</template>

	<!--	Loading	-->
	<q-loading-overlay :loading="!mediaItem" />
</template>

<script setup lang="ts">
import { computed, defineEmits, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import Log from 'consola';
import sum from 'lodash-es/sum';
import { PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { MediaList } from '#components';
import { MediaService } from '@service';
import { useMediaOverviewBarBus } from '#imports';

const emit = defineEmits<{
	(e: 'media-item', mediaItem: PlexMediaDTO | null): void;
	(e: 'close'): void;
}>();

const t = useI18n().t;
const mediaItem = ref<PlexMediaDTO | null>(null);
const thumbWidth = ref(180);
const thumbHeight = ref(270);
const defaultImage = ref(false);
const imageUrl = ref('');

const mediaOverViewBarBus = useMediaOverviewBarBus();

const openDetails = (mediaId: number, mediaType: PlexMediaType) => {
	mediaOverViewBarBus.emit({ downloadButtonVisible: false });
	useSubscription(
		MediaService.getMediaDataById(mediaId, mediaType).subscribe((data) => {
			mediaItem.value = data;
			emit('media-item', data);
		}),
	);

	useSubscription(
		MediaService.getThumbnail(mediaId, mediaType, thumbWidth.value, thumbHeight.value).subscribe({
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
};

const closeDetails = () => {
	mediaItem.value = null;
	emit('media-item', null);
};

const mediaCountFormatted = computed(() => {
	if (mediaItem.value) {
		const item = mediaItem.value;
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

defineExpose({
	openDetails,
	closeDetails,
});
</script>
