<template>
	<q-list class="no-background">
		<q-item
			v-ripple
			:clickable="mediaOverviewStore.allMediaMode">
			<q-item-section avatar>
				<QMediaTypeIcon
					:media-type="mediaOverviewStore.getMediaType"
					:size="36" />
			</q-item-section>
			<q-item-section>
				<q-item-label v-if="server && library">
					<span :class="{ 'inaccessible-item-text': !accountStore.getHasAccountServerAccess(server.id) }">
						{{ serverStore.getServerName(server.id) }}
					</span>
					{{ $t('general.delimiter.dash') }}
					<span :class="{ 'inaccessible-item-text': !accountStore.getHasAccountLibraryAccess(library.id) }">
						{{ libraryStore.getLibraryName(library.id) }}
					</span>
				</q-item-label>
				<q-item-label v-else>
					{{ mediaTypeToAllText(mediaOverviewStore.getMediaType) }}
				</q-item-label>
				<q-item-label
					v-if="!mediaOverviewStore.loading && hasMedia"
					caption>
					{{ formatted(mediaMetaData) }}
				</q-item-label>
			</q-item-section>
			<q-menu
				v-if="mediaOverviewStore.allMediaMode"
				anchor="bottom left"
				auto-close
				self="top left">
				<q-list>
					<q-item
						v-for="(type, i) in [PlexMediaType.Movie, PlexMediaType.TvShow].filter(x => x !== mediaOverviewStore.getMediaType)"
						:key="i"
						v-ripple
						clickable
						@click="mediaOverviewStore.changeAllMediaOverviewType(type)">
						<q-item-section avatar>
							<QMediaTypeIcon
								:media-type="type"
								:size="36"
								class="q-mr-md" />
						</q-item-section>
						<q-item-section>
							<QText
								size="h5"
								:value="mediaTypeToAllText(type)" />
						</q-item-section>
					</q-item>
				</q-list>
			</q-menu>
		</q-item>
	</q-list>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { type PlexMediaDTO, PlexMediaType } from '@dto';
import prettyBytes from 'pretty-bytes';
import {
	useAccountStore,
	useLibraryStore,
	useServerStore,
	useLocalizationStore,
	useMediaOverviewStore,
	useI18n,
} from '#imports';

const accountStore = useAccountStore();
const libraryStore = useLibraryStore();
const serverStore = useServerStore();
const localizationStore = useLocalizationStore();
const mediaOverviewStore = useMediaOverviewStore();

const props = withDefaults(defineProps<{
	libraryId?: number;
	detailMode?: boolean;
	mediaDetailItem?: PlexMediaDTO | null;

}>(), {
	libraryId: 0,
	detailMode: false,
});

const server = computed(() => serverStore.getServer(get(library)?.plexServerId ?? -1));
const library = computed(() => libraryStore.getLibrary(props.libraryId));

const { t } = useI18n();

const mediaMetaData = computed(() => {
	if (props.mediaDetailItem) {
		const movieFileCount = props.mediaDetailItem.mediaData.length || props.mediaDetailItem.qualities.length || 1;
		return {
			movieCount: props.mediaDetailItem.type === PlexMediaType.Movie ? movieFileCount : 0,
			tvShowCount: props.mediaDetailItem.type === PlexMediaType.TvShow ? 1 : 0,
			seasonCount: props.mediaDetailItem.childCount,
			episodeCount: props.mediaDetailItem.grandChildCount,
			fileSize: props.mediaDetailItem.mediaSize,
		};
	}

	return {
		movieCount: mediaOverviewStore.allMovieCount,
		tvShowCount: mediaOverviewStore.allTvShowCount,
		seasonCount: mediaOverviewStore.allSeasonCount,
		episodeCount: mediaOverviewStore.allEpisodeCount,
		fileSize: mediaOverviewStore.allFileSize,
	};
});

const hasMedia = computed(() => !!props.mediaDetailItem || mediaOverviewStore.totalCount > 0);

function formatted({ movieCount, tvShowCount, seasonCount, episodeCount, fileSize }: {
	movieCount: number;
	tvShowCount: number;
	seasonCount: number;
	episodeCount: number;
	fileSize: number;
}): string {
	switch (mediaOverviewStore.getMediaType) {
		case PlexMediaType.Movie:
			return t('components.media-overview-bar-header.movies-metadata', {
				movieCount,
				fileSize: toFileSize(fileSize),
			});
		case PlexMediaType.TvShow:
			return t('components.media-overview-bar-header.tv-shows-metadata', {
				tvShowCount,
				seasonCount,
				episodeCount,
				fileSize: toFileSize(fileSize),
			});
		default:
			return `Media type ${mediaOverviewStore.getMediaType} is not supported in the media count`;
	}
}

function toFileSize(size: number): string {
	if (size === 0) {
		return '-';
	}
	return prettyBytes(size, { locale: localizationStore.getLanguageLocale?.bcp47Code || 'en-US' });
}

function mediaTypeToAllText(mediaType: PlexMediaType): string {
	switch (mediaType) {
		case PlexMediaType.Movie:
			return t('components.media-overview-bar.all-media-mode.movies');
		case PlexMediaType.TvShow:
			return t('components.media-overview-bar.all-media-mode.tv-shows');
		default:
			return t('general.error.unknown');
	}
}
</script>

<style lang="scss">
.inaccessible-item-text {
  text-decoration: line-through;
  opacity: 0.62;
}
</style>
