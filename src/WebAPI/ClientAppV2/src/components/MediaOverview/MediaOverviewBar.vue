<template>
	<q-toolbar class="media-overview-bar" outlined :height="barHeight">
		<!--	Title	-->
		<q-toolbar-title>
			<q-row justify="start" no-gutters>
				<q-col v-if="detailMode" cols="auto">
					<q-list two-line class="no-background">
						<q-item>
							<q-btn flat icon="mdi-arrow-left" size="xl" @click="$emit('back')" />
						</q-item>
					</q-list>
				</q-col>
				<q-col cols="auto">
					<q-list class="no-background">
						<q-item>
							<q-item-section avatar>
								<q-media-type-icon class="mx-3" :size="36" :media-type="library.type" />
							</q-item-section>
							<q-item-section>
								<q-item-label>
									{{ server ? server.name : '?' }} - {{ library ? library.title : '?' }}
								</q-item-label>
								<q-item-label v-if="library" caption>
									{{ detailMode ? mediaCountFormatted : libraryCountFormatted }} -
									<q-file-size :size="mediaSize" />
								</q-item-label>
							</q-item-section>
						</q-item>
					</q-list>
				</q-col>
			</q-row>
		</q-toolbar-title>

		<q-space />
		<!--	Download button	-->
		<vertical-button
			v-if="!hideDownloadButton"
			icon="mdi-download"
			label="Download"
			:height="barHeight"
			:width="verticalButtonWidth"
			:disabled="!hasSelected"
			@click="download" />

		<!--	Refresh library button	-->
		<vertical-button
			v-if="!detailMode"
			icon="mdi-refresh"
			label="Refresh"
			:height="barHeight"
			:width="verticalButtonWidth"
			@click="refreshLibrary" />

		<!--	View mode	-->
		<vertical-button icon="mdi-eye" label="View" :height="barHeight" :width="verticalButtonWidth">
			<q-menu anchor="bottom left" self="top left" auto-close>
				<q-item
					v-for="(viewOption, i) in viewOptions"
					:key="i"
					clickable
					style="min-width: 200px"
					@click="changeView(viewOption.viewMode)">
					<!-- View mode options -->
					<q-item-section avatar>
						<q-avatar>
							<q-icon v-if="isSelected(viewOption.viewMode)" :name="'mdi-check'" />
						</q-avatar>
					</q-item-section>
					<!--	Is selected icon	-->
					<q-item-section> {{ viewOption.label }}</q-item-section>
				</q-item>
			</q-menu>
		</vertical-button>
	</q-toolbar>
</template>

<script setup lang="ts">
import { defineProps, defineEmits, ref, computed } from 'vue';
import { sum } from 'lodash-es';
import type { PlexLibraryDTO, PlexMediaDTO, PlexServerDTO } from '@dto/mainApi';
import { PlexMediaType, ViewMode } from '@dto/mainApi';

interface IViewOptions {
	label: string;
	viewMode: ViewMode;
}

const props = defineProps<{
	server: PlexServerDTO | null;
	library: PlexLibraryDTO | null;
	viewMode: ViewMode;
	mediaItem?: PlexMediaDTO | null;
	hasSelected: boolean;
	detailMode?: boolean;
	hideDownloadButton: boolean;
}>();

const emit = defineEmits<{
	(e: 'back'): void;
	(e: 'download'): void;
	(e: 'refresh-library', libraryId: number): void;
	(e: 'view-change', viewMode: ViewMode): void;
}>();

const barHeight = ref(85);
const verticalButtonWidth = ref(120);

const refreshLibrary = () => {
	emit('refresh-library', props.library?.id ?? -1);
};

const download = () => {
	emit('download');
};

const changeView = (viewMode: ViewMode) => {
	emit('view-change', viewMode);
};

const isSelected = (viewMode: ViewMode) => {
	return props.viewMode === viewMode;
};

const mediaSize = computed(() => {
	return props.detailMode ? props.mediaItem?.mediaSize ?? 0 : props.library?.mediaSize ?? 0;
});

const libraryCountFormatted = computed(() => {
	if (props.library) {
		switch (props.library?.type) {
			case PlexMediaType.Movie:
				return `${props.library.count} Movies`;
			case PlexMediaType.TvShow:
				return `${props.library.count} TvShows - ${props.library.seasonCount} Seasons - ${props.library.episodeCount} Episodes`;
			default:
				return `Library type ${props.library?.type} is not supported in the media count`;
		}
	}
	return 'unknown media count';
});

const mediaCountFormatted = computed(() => {
	if (props.mediaItem) {
		switch (props.library?.type) {
			case PlexMediaType.Movie:
				return `1 Movie`;
			case PlexMediaType.TvShow:
				return `1 TvShow - ${props.mediaItem.children?.length} Seasons - ${sum(
					props.mediaItem.children?.map((x) => x.childCount),
				)} Episodes`;
			default:
				return `Library type ${props.library?.type} is not supported in the media count`;
		}
	}

	return 'unknown media count';
});

const viewOptions = computed((): IViewOptions[] => {
	return [
		{
			label: 'Poster View',
			viewMode: ViewMode.Poster,
		},
		{
			label: 'Table View',
			viewMode: ViewMode.Table,
		},
		// {
		// 	label: 'Overview',
		// 	viewMode: ViewMode.Overview,
		// },
	];
});
</script>
<style lang="scss">
.media-overview-bar {
	border: 2px solid red;
}
</style>
