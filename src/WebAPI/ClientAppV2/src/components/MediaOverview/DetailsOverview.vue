<template>
	<template v-if="mediaItem">
		<!--	Header	-->

		<q-card square flat>
			<q-card-section horizontal>
				<!--	Poster	-->
				<q-img :src="imageUrl" :width="`${thumbWidth}px`" :height="`${thumbHeight}px`">
					<!--	Placeholder	-->
					<template #loading>
						<!--	Show fallback image	-->
						<template v-if="defaultImage">
							<q-row align="center" justify="center" class="fill-height">
								<q-col cols="auto">
									<q-media-type-icon :size="100" class="mx-3" media-type="mediaType" />
								</q-col>
								<q-col cols="12">
									<h4 class="text-center">{{ mediaItem.title }}</h4>
								</q-col>
							</q-row>
						</template>
						<!--	Show  image	-->
						<template v-else>
							<q-row class="fill-height ma-0" align="center" justify="center">
								<q-col cols="12">
									<h4 class="text-center">{{ mediaItem.title }}</h4>
								</q-col>
								<q-col cols="auto">
									<loading-spinner color="grey lighten-5" />
								</q-col>
							</q-row>
						</template>
					</template>
				</q-img>
				<!-- Media info-->
				<q-card-section>
					<q-card-title>
						{{ mediaItem.title }}
					</q-card-title>
					<q-markup-table>
						<q-tr>
							<q-td>{{ $t('components.details-overview.duration') }}</q-td>
							<q-td>
								<q-duration :value="mediaItem.duration" />
							</q-td>
						</q-tr>
						<q-tr>
							<q-td>{{ $t('components.details-overview.database-id') }}</q-td>
							<q-td>{{ mediaItem.id }}</q-td>
						</q-tr>
					</q-markup-table>
				</q-card-section>
			</q-card-section>
		</q-card>

		<!--	Media Table	-->
		<q-row v-if="mediaItem" no-gutters>
			<q-col>
				<MediaList :media-item="mediaItem" />
			</q-col>
		</q-row>
	</template>

	<!--	Loading	-->
	<q-loading-overlay :loading="!mediaItem" />
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, watch } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import Log from 'consola';
import { DownloadMediaDTO, PlexMediaDTO, PlexMediaType } from '@dto/mainApi';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';
import { MediaList } from '#components';
import { MediaService } from '@service';

const props = defineProps<{
	mediaType: PlexMediaType;
}>();

const emit = defineEmits<{
	(e: 'download', download: DownloadMediaDTO[] | DownloadMediaDTO): void;
	(e: 'media-item', mediaItem: PlexMediaDTO | null): void;
	(e: 'close'): void;
}>();

const mediaItem = ref<PlexMediaDTO | null>(null);
const thumbWidth = ref(150);
const thumbHeight = ref(200);
const defaultImage = ref(false);
const imageUrl = ref('');
const selected = ref<string[]>([]);
const downloadMediaCommand = ref<DownloadMediaDTO[]>([]);
const mediaTableColumns = getMediaTableColumns();

// watch(
// 	() => props.mediaItem,
// 	(newMediaItem: ITreeViewItem | null) => {
// 		if (newMediaItem) {
// 			MediaService.getThumbnail(newMediaItem.id, props.mediaType, thumbWidth.value, thumbHeight.value).subscribe((data) => {
// 				if (!data) {
// 					defaultImage.value = true;
// 					return;
// 				}
// 				imageUrl.value = data;
// 			});
// 		}
// 	},
// );

const openDetails = (mediaId: number) => {
	useSubscription(
		MediaService.getMediaDataById(mediaId, props.mediaType).subscribe((data) => {
			mediaItem.value = data;
			emit('media-item', data);
		}),
	);

	useSubscription(
		MediaService.getThumbnail(mediaId, props.mediaType, thumbWidth.value, thumbHeight.value).subscribe({
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

// const downloadSelectedMedia = () => {
// 	if (props.mediaType === PlexMediaType.TvShow) {
// 		// emit('download', detailMediaTableRef.value?.createDownloadCommands());
// 	}
// 	if (props.mediaType === PlexMediaType.Movie) {
// 		emit('download', {
// 			mediaIds: [props.mediaItem?.id ?? 0],
// 			type: props.mediaType,
// 			plexAccountId: props.activeAccountId,
// 		});
// 	}
// };

defineExpose({
	openDetails,
	closeDetails,
});
</script>
