<template>
	<q-toolbar class="media-overview-bar">
		<!--	Title	-->
		<q-toolbar-title>
			<q-row justify="start" align="center">
				<Transition appear enter-active-class="animated fadeInLeft" leave-active-class="animated fadeOutLeft">
					<q-col v-if="detailMode" cols="auto">
						<q-btn flat icon="mdi-arrow-left" size="xl" @click="$emit('back')" />
					</q-col>
				</Transition>
				<q-col cols="auto">
					<q-list class="no-background">
						<q-item>
							<q-item-section avatar>
								<q-media-type-icon class="mx-3" :size="36" :media-type="library?.type ?? PlexMediaType.None" />
							</q-item-section>
							<q-item-section>
								<q-item-label>
									{{ server ? serverStore.getServerName(server.id) : $t('general.commands.unknown') }} -
									{{ library ? libraryStore.getLibraryName(library.id) : $t('general.commands.unknown') }}
								</q-item-label>
								<q-item-label v-if="library && !detailMode" caption>
									{{ libraryCountFormatted }} -
									<q-file-size :size="library.mediaSize" />
								</q-item-label>
							</q-item-section>
						</q-item>
					</q-list>
				</q-col>
			</q-row>
		</q-toolbar-title>

		<!--	Download button	-->
		<vertical-button
			v-if="mediaOverviewStore.showDownloadButton"
			icon="mdi-download"
			label="Download"
			:height="barHeight"
			:width="verticalButtonWidth"
			@click="download" />

		<!--	Selection Dialog Button	-->
		<vertical-button
			v-if="mediaOverviewStore.showSelectionButton"
			icon="mdi-select-marker"
			text-id="selection"
			:height="barHeight"
			:width="verticalButtonWidth"
			@click="$emit('selection-dialog')" />

		<!--	Refresh library button	-->
		<vertical-button
			v-if="!detailMode"
			icon="mdi-refresh"
			label="Refresh"
			:height="barHeight"
			cy="media-overview-refresh-library-btn"
			:width="verticalButtonWidth"
			@click="refreshLibrary" />

		<!--	View mode	-->
		<vertical-button
			v-if="!detailMode"
			icon="mdi-eye"
			label="View"
			:height="barHeight"
			:width="verticalButtonWidth"
			cy="change-view-mode-btn">
			<q-menu anchor="bottom left" self="top left" auto-close>
				<q-list>
					<q-item
						v-for="(viewOption, i) in viewOptions"
						:key="i"
						clickable
						style="min-width: 200px"
						:data-cy="`view-mode-${viewOption.viewMode.toLowerCase()}-btn`"
						@click="changeView(viewOption.viewMode)">
						<!-- View mode options -->
						<q-item-section avatar>
							<q-avatar>
								<q-icon v-if="isSelected(viewOption.viewMode)" name="mdi-check" />
							</q-avatar>
						</q-item-section>
						<!--	Is selected icon	-->
						<q-item-section> {{ viewOption.label }}</q-item-section>
					</q-item>
				</q-list>
			</q-menu>
		</vertical-button>
	</q-toolbar>
</template>

<script setup lang="ts">
import type { PlexLibraryDTO, PlexServerDTO } from '@dto/mainApi';
import { PlexMediaType, ViewMode } from '@dto/mainApi';
import { useLibraryStore, useMediaOverviewBarDownloadCommandBus, useMediaOverviewStore, useServerStore } from '#imports';

const libraryStore = useLibraryStore();
const serverStore = useServerStore();
const mediaOverviewStore = useMediaOverviewStore();
const downloadCommandBus = useMediaOverviewBarDownloadCommandBus();

interface IViewOptions {
	label: string;
	viewMode: ViewMode;
}

const props = defineProps<{
	server: PlexServerDTO | null;
	library: PlexLibraryDTO | null;
	detailMode?: boolean;
}>();

const emit = defineEmits<{
	(e: 'back'): void;
	(e: 'selection-dialog'): void;
	(e: 'refresh-library', libraryId: number): void;
	(e: 'view-change', viewMode: ViewMode): void;
}>();

const barHeight = ref(85);
const verticalButtonWidth = ref(120);

const refreshLibrary = () => {
	emit('refresh-library', props.library?.id ?? -1);
};

const download = () => {
	downloadCommandBus.emit('download');
};

const changeView = (viewMode: ViewMode) => {
	emit('view-change', viewMode);
};

const isSelected = (viewMode: ViewMode) => {
	return mediaOverviewStore.getMediaViewMode === viewMode;
};

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
	];
});
</script>
