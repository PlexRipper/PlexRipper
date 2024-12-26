<template>
	<q-list class="no-background">
		<q-item
			v-ripple
			:clickable="allMediaMode">
			<q-item-section avatar>
				<QMediaTypeIcon
					:media-type="mediaType"
					:size="36"
					class="mx-3" />
			</q-item-section>
			<q-item-section>
				<q-item-label v-if="server && library">
					{{ server ? serverStore.getServerName(server.id) : $t('general.commands.unknown') }}
					{{ $t('general.delimiter.dash') }}
					{{ library ? libraryStore.getLibraryName(library.id) : $t('general.commands.unknown') }}
				</q-item-label>
				<q-item-label v-else>
					{{ mediaTypeToAllText(mediaType) }}
				</q-item-label>
				<q-item-label
					v-if="!mediaOverviewStore.loading"
					caption>
					{{ mediaMetaData }}
				</q-item-label>
			</q-item-section>
			<q-menu
				v-if="allMediaMode"
				anchor="bottom left"
				auto-close
				self="top left">
				<q-list>
					<q-item
						v-for="(type, i) in [PlexMediaType.Movie, PlexMediaType.TvShow].filter(x => x !== mediaType)"
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
import { useLibraryStore, useServerStore, useLocalizationStore, useMediaOverviewStore, useI18n } from '#imports';

const libraryStore = useLibraryStore();
const serverStore = useServerStore();
const localizationStore = useLocalizationStore();
const mediaOverviewStore = useMediaOverviewStore();

const props = withDefaults(defineProps<{
	mediaType: PlexMediaType;
	libraryId?: number;
	allMediaMode: boolean;
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
		return formatted({
			movieCount: 0,
			tvShowCount: 1,
			seasonCount: props.mediaDetailItem.childCount,
			episodeCount: props.mediaDetailItem.grandChildCount,
			fileSize: props.mediaDetailItem.mediaSize,
		});
	}

	return formatted({
		movieCount: mediaOverviewStore.allMovieCount,
		tvShowCount: mediaOverviewStore.allTvShowCount,
		seasonCount: mediaOverviewStore.allSeasonCount,
		episodeCount: mediaOverviewStore.allEpisodeCount,
		fileSize: mediaOverviewStore.allFileSize,
	});
});

function formatted({ movieCount, tvShowCount, seasonCount, episodeCount, fileSize }: {
	movieCount: number;
	tvShowCount: number;
	seasonCount: number;
	episodeCount: number;
	fileSize: number;
}): string {
	switch (props.mediaType) {
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
			return `Media type ${props.mediaType} is not supported in the media count`;
	}
}

function toFileSize(size: number): string {
	if (!size) {
		return '-';
	}
	return prettyBytes(size, { locale: localizationStore.getLanguageLocale?.bcp47Code });
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
