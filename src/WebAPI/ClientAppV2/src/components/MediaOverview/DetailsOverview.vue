<template>
	<q-page v-show="isOpen">
		<template v-if="mediaItem">
			<!--	Details Overview Bar	-->
			<q-row class="mx-0">
				<media-overview-bar
					detail-mode
					:library="library"
					:server="server"
					:media-item="mediaItem"
					:has-selected="selected.length > 0"
					@download="downloadSelectedMedia"
					@back="close" />
			</q-row>

			<!--	Header	-->
			<q-row style="max-height: 250px; height: 250px">
				<!--	Poster	-->
				<q-col cols="auto">
					<q-card :max-width="thumbWidth" :width="thumbWidth">
						<q-img :src="imageUrl" :width="thumbWidth" :height="thumbHeight">
							<!--	Placeholder	-->
							<template #placeholder>
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
			<q-row no-gutters>
				<q-col>
					<!--					<MediaTable-->
					<!--						ref="detailMediaTableRef"-->
					<!--						:items="[mediaItem]"-->
					<!--						:media-type="mediaType"-->
					<!--						:library-id="library.id"-->
					<!--						hide-navigation-->
					<!--						detail-mode-->
					<!--						@download="downloadMedia"-->
					<!--						@selected="selected = $event" />-->
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
	</q-page>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, watch } from 'vue';
// import MediaTable from '@mediaOverview/MediaTable/MediaTable.vue';
import { DownloadMediaDTO, PlexLibraryDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import { MediaService } from '@service';
import ITreeViewItem from '@class/ITreeViewItem';

const props = defineProps<{
	mediaType: PlexMediaType;
	mediaItem: ITreeViewItem | null;
	library: PlexLibraryDTO | null;
	server: PlexServerDTO | null;
	activeAccountId: number;
}>();

// const detailMediaTableRef = ref<InstanceType<typeof MediaTable> | null>(null);

const emit = defineEmits<{
	(e: 'download', download: DownloadMediaDTO[] | DownloadMediaDTO): void;
	(e: 'close'): void;
}>();

const thumbWidth = ref(150);
const thumbHeight = ref(200);
const isOpen = ref(false);
const mediaId = ref(0);
const defaultImage = ref(false);
const imageUrl = ref('');
const selected = ref<string[]>([]);
const downloadMediaCommand = ref<DownloadMediaDTO[]>([]);

const close = () => {
	isOpen.value = false;
	emit('close');
};

watch(
	() => props.mediaItem,
	(newMediaItem: ITreeViewItem | null) => {
		if (newMediaItem) {
			MediaService.getThumbnail(newMediaItem.id, props.mediaType, thumbWidth.value, thumbHeight.value).subscribe((data) => {
				if (!data) {
					defaultImage.value = true;
					return;
				}
				imageUrl.value = data;
			});
		}
	},
);

const openDetails = () => {
	isOpen.value = true;
};

const downloadSelectedMedia = () => {
	if (props.mediaType === PlexMediaType.TvShow) {
		// emit('download', detailMediaTableRef.value?.createDownloadCommands());
	}
	if (props.mediaType === PlexMediaType.Movie) {
		emit('download', {
			mediaIds: [props.mediaItem?.id ?? 0],
			type: props.mediaType,
			plexAccountId: props.activeAccountId,
		});
	}
};

const downloadMedia = (download: DownloadMediaDTO[] | DownloadMediaDTO) => {
	emit('download', download);
};

defineExpose({
	openDetails,
});
</script>
