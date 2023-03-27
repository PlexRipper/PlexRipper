<template>
	<template v-if="mediaItem">
		<!--	Header	-->
		<q-row style="max-height: 250px; height: 250px">
			<!--	Poster	-->
			<q-col cols="auto">
				<q-card :max-width="thumbWidth" :width="thumbWidth">
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
				</q-card>
			</q-col>

			<q-col>
				<q-card>
					<q-card-title>
						{{ mediaItem.title }}
					</q-card-title>
					<q-card-section>
						<q-row no-gutters>
							<q-col cols="12">
								<span>
									{{ $t('components.details-overview.duration') }}
									<q-duration :value="mediaItem.duration" />
								</span>
							</q-col>
							<q-col cols="12">
								<span>{{ $t('components.details-overview.database-id') }} {{ mediaItem.id }}</span>
							</q-col>
						</q-row>
					</q-card-section>
				</q-card>
			</q-col>
		</q-row>

		<!--	Media Table	-->
		<Print>{{ mediaItem }}</Print>
		<q-row no-gutters>
			<q-col>
				<q-tree-view-table :columns="mediaTableColumns" :nodes="[mediaItem]" />
			</q-col>
		</q-row>
	</template>
	<!--	Loading	-->
	<template v-else>
		<q-row justify="center">
			<q-col cols="auto">
				<loading-spinner :size="60" />
			</q-col>
		</q-row>
	</template>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, watch } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadMediaDTO, PlexLibraryDTO, PlexMediaDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import { MediaService } from '@service';
import { getMediaTableColumns } from '~/composables/mediaTableColumns';

const props = defineProps<{
	mediaType: PlexMediaType;
	library: PlexLibraryDTO;
	server: PlexServerDTO;
	mediaItem: PlexMediaDTO | null;
}>();

// const detailMediaTableRef = ref<InstanceType<typeof MediaTable> | null>(null);

const emit = defineEmits<{
	(e: 'download', download: DownloadMediaDTO[] | DownloadMediaDTO): void;
	(e: 'close'): void;
}>();

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

const openDetails = (mediaId: number) => {};

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
});
</script>
