<template>
	<!--	Loading screen	-->
	<template v-if="isLoading">
		<q-row justify="center" class="mx-0">
			<q-col cols="auto" align-self="center">
				<q-circular-progress size="70px" indeterminate />
				<h1 v-if="isRefreshing">
					{{
						$t('components.media-overview.is-refreshing', {
							library: library.value ? library.value.title : $t('general.commands.unknown'),
							server: server.value ? server.value.name : $t('general.commands.unknown'),
						})
					}}
				</h1>
				<h1 v-else>{{ $t('components.media-overview.retrieving-library') }}</h1>
				<!-- Library progress bar -->
				<q-linear-progress :value="getPercentage" height="20" stripe color="deep-orange">
					<div class="absolute-full flex flex-center">
						<q-badge color="white" text-color="accent" :label="`${getPercentage}%`" />
					</div>
				</q-linear-progress>
			</q-col>
		</q-row>
	</template>
	<!-- Header -->
	<template v-else-if="server && library">
		<q-row v-show="showMediaOverview" no-gutters>
			<q-col>
				<!--	Overview bar	-->
				<media-overview-bar
					:server="server"
					:library="library"
					:view-mode="viewMode"
					:has-selected="selected.length > 0"
					:hide-download-button="!isTableView"
					@view-change="changeView"
					@refresh-library="refreshLibrary"
					@download="processDownloadCommand([])" />
				<!--	Data table display	-->
				<template v-if="isTableView">
					<!--					<MediaTable-->
					<!--						ref="overviewMediaTable"-->
					<!--						:items="items"-->
					<!--						:active-account-id="activeAccountId"-->
					<!--						:library-id="libraryId"-->
					<!--						:media-type="mediaType"-->
					<!--						@download="processDownloadCommand"-->
					<!--						@selected="selected = $event"-->
					<!--						@request-media="requestMedia" />-->
				</template>

				<!-- Poster display-->
				<template v-if="isPosterView">
					<poster-table
						:items="items"
						:active-account-id="activeAccountId"
						:media-type="mediaType"
						@download="processDownloadCommand"
						@open-details="openDetails" />
				</template>
			</q-col>
		</q-row>
		<!--	Overlay with details of the media	-->
		<DetailsOverview
			v-show="!showMediaOverview"
			ref="detailsOverview"
			:media-type="mediaType"
			:media-item="detailItem"
			:library="library"
			:server="server"
			:active-account-id="activeAccountId"
			@close="closeDetailsOverview"
			@download="processDownloadCommand" />
	</template>
	<template v-else>
		<h1>{{ $t('components.media-overview.no-data') }}</h1>
	</template>
	<!--	Download confirmation dialog	-->
	<!--	<DownloadConfirmation ref="downloadConfirmationRef" :items="items" @download="sendDownloadCommand" />-->
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, defineProps, computed, watch } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { useRoute, useRouter } from 'vue-router';
import type { DisplaySettingsDTO, DownloadMediaDTO, PlexMediaDTO, PlexServerDTO } from '@dto/mainApi';
import { LibraryProgress, PlexLibraryDTO, PlexMediaType, ViewMode } from '@dto/mainApi';
import { DownloadService, LibraryService, SettingsService, SignalrService } from '@service';
import { getTvShow } from '@api/mediaApi';
import { DetailsOverview } from '#components';

const activeAccountId = ref(0);
const movieViewMode = ref(ViewMode.Poster);
const tvShowViewMode = ref(ViewMode.Poster);
const selected = ref<string[]>([]);
const isRefreshing = ref(false);

const server = ref<PlexServerDTO | null>(null);
const library = ref<PlexLibraryDTO | null>(null);

const libraryProgress = ref<LibraryProgress | null>(null);
const downloadPreviewType = ref(PlexMediaType.None);
const items = ref<PlexMediaDTO[]>([]);
const detailItem = ref<PlexMediaDTO | null>(null);

// const downloadConfirmationRef = ref<InstanceType<typeof DownloadConfirmation> | null>(null);
const detailsOverview = ref<InstanceType<typeof DetailsOverview> | null>(null);
// const overviewMediaTableRef = ref<InstanceType<typeof MediaTable> | null>(null);

const props = defineProps<{
	libraryId: number;
}>();

const router = useRouter();
const route = useRoute();
const mediaType = computed(() => library.value?.type ?? PlexMediaType.Unknown);
const getPercentage = computed(() => libraryProgress.value?.percentage ?? -1);
const isLoading = computed(() => isRefreshing.value || !(server.value && library.value));
const viewMode = computed(() => {
	switch (mediaType.value) {
		case PlexMediaType.Movie:
			return movieViewMode.value;
		case PlexMediaType.TvShow:
			return tvShowViewMode.value;
		default:
			return ViewMode.Poster;
	}
});

const showMediaOverview = computed(() => !(detailItem.value ?? false));
const isPosterView = computed(() => viewMode.value === ViewMode.Poster);
const isTableView = computed(() => viewMode.value === ViewMode.Table);

const changeView = (viewMode: ViewMode) => {
	let type: keyof DisplaySettingsDTO | null = null;

	switch (mediaType.value) {
		case PlexMediaType.Movie:
			movieViewMode.value = viewMode;
			type = 'movieViewMode';
			break;
		case PlexMediaType.TvShow:
			tvShowViewMode.value = viewMode;
			type = 'tvShowViewMode';
			break;
		default:
			Log.error('Could not set view mode for type' + mediaType.value);
	}
	if (type) {
		useSubscription(SettingsService.updateDisplaySettings(type, viewMode).subscribe());
	}
};

const resetProgress = (isRefreshingValue: boolean) => {
	isRefreshing.value = isRefreshingValue;

	libraryProgress.value = {
		id: props.libraryId,
		percentage: 0,
		received: 0,
		total: 0,
		isRefreshing: isRefreshingValue,
		isComplete: false,
		timeStamp: '',
	};
};

const processDownloadCommand = (downloadMediaCommand: DownloadMediaDTO[]) => {
	// TODO: Fix this
	// if (downloadMediaCommand.length > 0) {
	// 	downloadConfirmationRef.value?.openDialog(downloadMediaCommand);
	// 	return;
	// }
	// if (overviewMediaTableRef.value) {
	// 	downloadConfirmationRef.value?.openDialog(overviewMediaTableRef.value.createDownloadCommands());
	// } else {
	// 	Log.error('overviewMediaTableRef was invalid', overviewMediaTableRef.value);
	// }
};

const sendDownloadCommand = (downloadMediaCommand: DownloadMediaDTO[]) => {
	DownloadService.downloadMedia(downloadMediaCommand);
};

const openDetails = (mediaId: number) => {
	if (!router.currentRoute.value.path.includes('details')) {
		router.push({
			path: props.libraryId + '/details/' + mediaId,
		});
	}
	detailsOverview.value?.openDetails();

	const item = items.value.find((x) => x.id === mediaId);
	if (item?.children?.length === 0) {
		requestMedia({
			item,
			resolve: () => {
				detailItem.value = items.value.find((x) => x.id === mediaId) ?? null;
			},
		});
	}
};

const closeDetailsOverview = () => {
	Log.debug('Close Details Overview');
	router.push({
		path: '/tvshows/' + props.libraryId,
	});
	resetDetailsOverview();
};

const resetDetailsOverview = () => {
	detailItem.value = null;
};

const refreshLibrary = () => {
	isRefreshing.value = true;
	resetProgress(true);
	LibraryService.refreshLibrary(props.libraryId).subscribe((data) => {
		setLibrary(data);
		isRefreshing.value = false;
	});
};

const setLibrary = (data: PlexLibraryDTO | null) => {
	if (data) {
		library.value = data;
		switch (mediaType.value) {
			case PlexMediaType.Movie:
				items.value = library.value?.movies ?? [];
				break;
			case PlexMediaType.TvShow:
				items.value = library.value?.tvShows ?? [];
				break;
		}
	}
};

const requestMedia = (numberPromise: { item: PlexMediaDTO; resolve?: Function }) => {
	if (mediaType.value === PlexMediaType.TvShow) {
		getTvShow(numberPromise.item.id).subscribe((response) => {
			if (response.isSuccess) {
				const itemsIndex = items.value.findIndex((x) => x.id === numberPromise.item.id);
				// This is a fix to prevent episodes from acting like it has additional children and that it can be requested
				response.value?.children?.forEach((season) => {
					season.children?.forEach((episode) => {
						// @ts-ignore:
						episode.children = undefined;
					});
				});
				items.value[itemsIndex] = response.value ?? numberPromise.item;
				if (numberPromise.resolve) {
					numberPromise.resolve();
				}
			}
		});
	} else {
		Log.error('Request media could not be executed for ' + mediaType.value);
	}
};

watch(
	() => route.path,
	(newPath: string, oldPath: string) => {
		if (oldPath.includes('details') && !newPath.includes('details')) {
			resetDetailsOverview();
		}
	},
);

watch(isLoading, (val: boolean) => {
	if (!val) {
		if (detailsOverview.value) {
			if (+route.params.mediaid) {
				openDetails(+route.params.mediaid);
			}
		} else {
			nextTick(() => {
				if (+route.params.mediaid) {
					openDetails(+route.params.mediaid);
				}
			});
		}
	}
});

onMounted(() => {
	resetProgress(false);
	isRefreshing.value = false;

	// Get Active account id
	useSubscription(SettingsService.getActiveAccountId().subscribe((id) => (activeAccountId.value = id)));

	// Get display settings
	useSubscription(
		SettingsService.getMovieViewMode().subscribe((value) => {
			movieViewMode.value = value;
		}),
	);

	useSubscription(
		SettingsService.getTvShowViewMode().subscribe((value) => {
			tvShowViewMode.value = value;
		}),
	);

	// Setup progress bar
	useSubscription(
		SignalrService.getLibraryProgress(props.libraryId).subscribe((data) => {
			if (data) {
				libraryProgress.value = data;
				isRefreshing.value = data.isRefreshing;
				if (data.isComplete) {
					refreshLibrary();
				}
			}
		}),
	);

	// Get server
	useSubscription(
		LibraryService.getServerByLibraryId(props.libraryId).subscribe((data) => {
			if (data) {
				server.value = data;
				return;
			}
			Log.error(`MediaOverview => Server was invalid for library id ${props.libraryId}:`, data);
		}),
	);

	// Retrieve library data
	useSubscription(LibraryService.getLibrary(props.libraryId).subscribe((data) => setLibrary(data)));
});
</script>
