import { cloneDeep, isEqual, isNumber, orderBy, sortBy, uniqueId } from 'lodash-es';
import { format } from 'date-fns';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { reactive, computed, toRefs } from 'vue';
import { get } from '@vueuse/core';
import {
	type PlexMediaMetadataDTO,
	type PlexMediaSlimDTO,
	type PlexMediaStatisticsDTO,
	PlexMediaType,
	VideoQuality,
	ViewMode,
} from '@dto';
import type { IMediaOverviewSort } from '@composables/event-bus';
import { MediaSortField, SortDirection } from '@enums';
import { StoreNames, type IMetaDataMediaFilter, type ISelection } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';
import { map, tap } from 'rxjs/operators';
import { defer, forkJoin, type Observable, of } from 'rxjs';
import { useLibraryStore, useSettingsStore } from '@store';
import { getHighestQualityRank, getVideoQualityColor, translateVideoQuality, getHighestQuality } from '@composables';
import { useSubscription } from '@vueuse/rxjs';

interface IMediaOverviewStoreState {
	libraryId: number;
	items: Readonly<PlexMediaSlimDTO[]>;
	sortedItems: Readonly<PlexMediaSlimDTO[]>;
	itemsLength: number;
	sortedState: IMediaOverviewSort[];
	scrollDict: Record<string, number>;
	scrollAlphabet: string[];
	selection: ISelection;
	downloadButtonVisible: boolean;
	mediaType: PlexMediaType;
	filterQuery: string;
	lastMediaItemViewed: PlexMediaSlimDTO | null;
	loading: boolean;
	isDetailView: boolean;
	allMovieCount: number;
	allTvShowCount: number;
	allSeasonCount: number;
	allEpisodeCount: number;
	allFileSize: number;
	metadata: IMetaDataMediaFilter;
	metadataList: PlexMediaMetadataDTO;
}

export const useMediaOverviewStore = defineStore(StoreNames.MediaOverviewStore, () => {
	const defaultState: IMediaOverviewStoreState = {
		libraryId: 0,
		items: [],
		sortedItems: [],
		itemsLength: 0,
		sortedState: [],
		scrollDict: { '#': 0 },
		scrollAlphabet: [],
		selection: { keys: [], allSelected: false, indexKey: 0 },
		downloadButtonVisible: false,
		mediaType: PlexMediaType.None,
		filterQuery: '',
		lastMediaItemViewed: null,
		loading: false,
		isDetailView: false,
		allMovieCount: 0,
		allTvShowCount: 0,
		allSeasonCount: 0,
		allEpisodeCount: 0,
		allFileSize: 0,
		metadata: {
			countryId: 0,
			roleId: 0,
			genreId: 0,
			quality: VideoQuality.None,
		},
		metadataList: {
			mediaCount: 0,
			roleCount: 0,
			countryCount: 0,
			genreCount: 0,
			qualityCount: 0,
			roles: [],
			countries: [],
			genres: [],
			qualities: [],
		},
	};

	const state = reactive<IMediaOverviewStoreState>(cloneDeep(defaultState));

	const settingsStore = useSettingsStore();
	const libraryStore = useLibraryStore();

	const actions = {
		refreshMetaData() {
			return plexLibraryApi.getLibraryMediaMetadata(state.libraryId, { mediaType: state.mediaType }).pipe(tap((result) => {
				if (result.isSuccess && result.value) {
					return state.metadataList = result.value;
				}
			}));
		},
		refreshAllLibraryMediaByType(page: number = 0, size: number = 100): Observable<PlexMediaStatisticsDTO | null> {
			return plexMediaApi.getAllMediaByTypeEndpoint({
				mediaType: state.mediaType,
				page,
				size,
				filterOwnedMedia: settingsStore.generalSettings.hideMediaFromOwnedServers,
				filterOfflineMedia: settingsStore.generalSettings.hideMediaFromOfflineServers,
				...state.metadata,
			}).pipe(map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => {
				if (isSuccess && value) {
					return value;
				}
				return null;
			}));
		},
		refreshLibraryMedia(page: number = 0, size: number = 100): Observable<PlexMediaStatisticsDTO | null> {
			return plexLibraryApi.getPlexLibraryMediaEndpoint(state.libraryId, {
				page,
				size,
				filterOfflineMedia: false,
				filterOwnedMedia: false,
				...state.metadata,
			}).pipe(map(({ isSuccess, value }): PlexMediaStatisticsDTO | null => {
				if (isSuccess && value) {
					return value;
				}
				return null;
			}));
		},
		requestMedia(): Observable<PlexMediaStatisticsDTO | null> {
			if (state.loading) {
				return of(null);
			}

			const page = 0;
			const size = 0;

			state.loading = true;

			return forkJoin([
				actions.refreshMetaData(),
				defer(() =>
					state.libraryId > 0
						? libraryStore.refreshLibrary(state.libraryId)
						: of(null),
				),
				defer(() =>
					state.libraryId === 0
						? actions.refreshAllLibraryMediaByType(page, size)
						: actions.refreshLibraryMedia(page, size),
				).pipe(
					tap((data) => {
						actions.setMedia(data, state.mediaType);
					}),
				),
			]).pipe(
				map(([_, __, media]) => media),
				tap(() => state.loading = false),
			);
		},
		setMedia(data: PlexMediaStatisticsDTO | null, mediaType: PlexMediaType) {
			if (data) {
				state.items = Object.freeze(data.mediaList);
				state.itemsLength = data.mediaCount;
				state.mediaType = mediaType;

				state.allMovieCount = data.movieCount;
				state.allTvShowCount = data.tvShowCount;
				state.allSeasonCount = data.seasonCount;
				state.allEpisodeCount = data.episodeCount;
				state.allFileSize = data.mediaSize;
			} else {
				state.items = Object.freeze([]);
				state.itemsLength = 0;
				state.mediaType = mediaType;

				state.allMovieCount = 0;
				state.allTvShowCount = 0;
				state.allSeasonCount = 0;
				state.allEpisodeCount = 0;
				state.allFileSize = 0;
			}
			state.filterQuery = '';
		},
		setMetaData({
			countryId,
			roleId,
			genreId,
			quality,
		}: Partial<IMetaDataMediaFilter>): Observable<PlexMediaStatisticsDTO | null> {
			if (isNumber(countryId)) {
				state.metadata.countryId = countryId;
			}

			if (isNumber(roleId)) {
				state.metadata.roleId = roleId;
			}

			if (isNumber(genreId)) {
				state.metadata.genreId = genreId;
			}

			if (quality) {
				state.metadata.quality = quality;
			}

			return actions.requestMedia();
		},
		unsetMetaData(key: keyof IMetaDataMediaFilter): Observable<PlexMediaStatisticsDTO | null> {
			switch (key) {
				case 'countryId':
				case 'roleId':
				case 'genreId':
					state.metadata[key] = 0;
					break;
				case 'quality':
					state.metadata[key] = VideoQuality.None;
					break;
			}

			return actions.requestMedia();
		},
		clearMetaDataFilter() {
			state.metadata = {
				countryId: 0,
				roleId: 0,
				genreId: 0,
				quality: VideoQuality.None,
			};
		},
		changeAllMediaOverviewType(mediaType: PlexMediaType) {
			state.mediaType = mediaType;
			settingsStore.displaySettings.allOverviewViewMode = mediaType;
			useSubscription(actions.requestMedia().subscribe());
		},
		setFirstLetterIndex() {
			state.scrollDict = {};
			const items = get(getters.getMediaItems);
			const activeSort = state.sortedState[0] ?? null;
			const field = activeSort?.field ?? null;
			const direction = activeSort?.sort ?? SortDirection.Asc;

			if (field === MediaSortField.Year) {
				// Group by individual year in the order they appear in the sorted list
				const seen = new Set<string>();
				for (let i = 0; i < items.length; i++) {
					const year = String(items[i]!.year ?? '#');
					if (!seen.has(year)) {
						seen.add(year);
						state.scrollDict[year] = i;
					}
				}
			} else if (field === MediaSortField.Quality) {
				// Group by quality label in the order they appear in the sorted list
				const seen = new Set<string>();
				for (let i = 0; i < items.length; i++) {
					const label = translateVideoQuality(getHighestQuality(items[i]!));
					if (!seen.has(label)) {
						seen.add(label);
						state.scrollDict[label] = i;
					}
				}
			} else if (field === MediaSortField.Duration) {
				// Group by 10-minute buckets (e.g. "0–10 min", "10–20 min")
				// Only buckets that contain actual items are included, in sorted order
				const seen = new Set<string>();
				for (let i = 0; i < items.length; i++) {
					const seconds = items[i]!.duration ?? 0;
					const bucketStart = Math.floor(seconds / 600) * 10;
					const label = `${bucketStart}–${bucketStart + 10} min`;
					if (!seen.has(label)) {
						seen.add(label);
						state.scrollDict[label] = i;
					}
				}
			} else if (field === MediaSortField.AddedAt || field === MediaSortField.UpdatedAt) {
				// Group by month + year (e.g. "Apr 2025")
				const seen = new Set<string>();
				for (let i = 0; i < items.length; i++) {
					const raw = items[i]![field as 'addedAt' | 'updatedAt'];
					if (!raw) {
						continue;
					}
					const label = format(new Date(raw), 'MMM yyyy');
					if (!seen.has(label)) {
						seen.add(label);
						state.scrollDict[label] = i;
					}
				}
			} else if (field === MediaSortField.MediaSize) {
				// Group by 1 GB buckets (e.g. "0–1 GB", "1–2 GB")
				// Only buckets present in the data are included, in sorted order
				const GB = 1_073_741_824;
				const seen = new Set<string>();
				for (let i = 0; i < items.length; i++) {
					const bytes = items[i]!.mediaSize ?? 0;
					const bucketStart = Math.floor(bytes / GB);
					const label = `${bucketStart}–${bucketStart + 1} GB`;
					if (!seen.has(label)) {
						seen.add(label);
						state.scrollDict[label] = i;
					}
				}
			} else {
				// Title / default (sortIndex) navigation — scan items in their current order
				// and record the first occurrence of each initial letter.
				// Only letters that actually have matching items are included.
				const seen = new Set<string>();
				for (let i = 0; i < items.length; i++) {
					const char = items[i]!.title[0]?.toUpperCase() ?? '#';
					const key = /[A-Z]/.test(char) ? char : '#';
					if (!seen.has(key)) {
						seen.add(key);
						state.scrollDict[key] = i;
					}
				}
			}

			// Keys are inserted in sorted-list order; reverse for desc so the nav
			// visually matches the top-to-bottom order of the poster grid.
			const keys = Object.keys(state.scrollDict);
			state.scrollAlphabet = direction === SortDirection.Desc ? keys.reverse() : keys;
		},
		setSelection(selection: ISelection) {
			state.selection = selection;
		},
		getMediaIndex(mediaId: number): number {
			return state.items.findIndex((x) => x.id === mediaId);
		},
		setSelectionRange(min: number, max: number) {
			actions.setSelection({
				indexKey: state.selection.indexKey,
				keys: get(getters.getMediaItems)
					.filter((x) => x.sortIndex >= min && x.sortIndex <= max)
					.map((x) => x.id),
				allSelected: false,
			} as ISelection);
		},
		setRootSelected(value: boolean) {
			actions.setSelection({
				indexKey: state.selection?.indexKey ?? 0,
				keys: value ? state.items.map((x) => x.id) : [],
				allSelected: value,
			} as ISelection);
		},
		clearSort() {
			state.sortedState = [];
			state.sortedItems = [];
		},
		clearFilter() {
			state.filterQuery = '';
		},
		sortMedia(event: IMediaOverviewSort) {
			// Single-sort: the new event fully replaces any existing sort
			const newSortedState: IMediaOverviewSort[] = event.sort !== SortDirection.NoSort ? [event] : [];

			// Prevent unnecessary sorting
			if (isEqual(state.sortedState, newSortedState)) {
				return;
			}

			if (newSortedState.length === 0) {
				state.sortedItems = [];
				state.sortedState = [];
				return;
			}

			const direction = event.sort as SortDirection.Asc | SortDirection.Desc;

			// Quality is not a direct field on PlexMediaSlimDTO; sort by derived rank
			if (event.field === MediaSortField.Quality) {
				state.sortedItems = Object.freeze(
					orderBy(state.items, [(item) => getHighestQualityRank(item)], [direction]),
				);
			} else {
				state.sortedItems = Object.freeze(
					orderBy(
						state.items,
						[event.field as keyof PlexMediaSlimDTO],
						[direction],
					),
				);
			}
			state.sortedState = newSortedState;
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {
		hasSelectedMedia: computed((): boolean => {
			return state.selection.keys.length > 0;
		}),
		getActiveSort: computed((): IMediaOverviewSort | null => {
			return state.sortedState.length > 0 ? state.sortedState[0] ?? null : null;
		}),
		hasNoSearchResults: computed((): boolean => {
			return state.filterQuery != '' && get(getters.getMediaItems).length === 0;
		}),
		hasNoFilterResults: computed((): boolean => {
			return state.metadataList.mediaCount > 0 && get(getters.getMediaItems).length === 0;
		}),
		allMediaMode: computed(() => state.libraryId === 0),
		library: computed(() => libraryStore.getLibrary(state.libraryId)),
		getMediaItems: computed((): Readonly<PlexMediaSlimDTO[]> => {
			if (!state.items) {
				return [];
			}
			// Currently sorting
			const query = state.filterQuery.toLowerCase();
			if (state.sortedState.length > 0) {
				if (state.filterQuery != '') {
					return state.sortedItems.filter((x) => x.searchTitle.includes(query));
				}
				return state.sortedItems;
			} else {
				if (state.filterQuery != '') {
					return state.items.filter((x) => x.searchTitle.includes(query));
				}
				return state.items;
			}
		}),
		getMediaViewMode: computed((): ViewMode => {
			switch (state.mediaType) {
				case PlexMediaType.Movie:
					return settingsStore.displaySettings.movieViewMode;
				case PlexMediaType.TvShow:
					return settingsStore.displaySettings.tvShowViewMode;
				default:
					return ViewMode.Poster;
			}
		}),
		showSelectionButton: computed((): boolean => {
			return get(getters.getMediaViewMode) === ViewMode.Table && !state.isDetailView;
		}),
		showDownloadButton: computed((): boolean => {
			return state.downloadButtonVisible || (get(getters.hasSelectedMedia) && get(getters.getMediaViewMode) === ViewMode.Table);
		}),
		isRootSelected: computed((): boolean | null => {
			if (state.selection?.keys.length === state.itemsLength) {
				return true;
			}

			if (state.selection?.keys.length === 0) {
				return false;
			}

			return null;
		}),
		getGenres: computed(() => sortBy(state.metadataList.genres, (x) => x.name)),
		getRoles: computed(() => sortBy(state.metadataList.roles, (x) => x.name)),
		getCountries: computed(() => sortBy(state.metadataList.countries, (x) => x.name)),
		getQualities: computed(() => state.metadataList.qualities),
		getFilterChips: computed(() => {
			const result: { text: string; key: keyof IMetaDataMediaFilter; color?: string; id: string }[] = [];

			if (state.metadata.countryId > 0) {
				result.push({
					text: state.metadataList.countries.find((x) => x.id === state.metadata.countryId)?.name ?? '',
					key: 'countryId',
					id: uniqueId(),
				});
			}

			if (state.metadata.roleId > 0) {
				result.push({
					text: state.metadataList.roles.find((x) => x.id === state.metadata.roleId)?.name ?? '',
					key: 'roleId',
					id: uniqueId(),
				});
			}

			if (state.metadata.genreId > 0) {
				result.push({
					text: state.metadataList.genres.find((x) => x.id === state.metadata.genreId)?.name ?? '',
					key: 'genreId',
					id: uniqueId(),
				});
			}

			if (state.metadata.quality != VideoQuality.None) {
				const quality = state.metadataList.qualities.find((x) => x.quality === state.metadata.quality)?.quality;
				result.push({
					text: translateVideoQuality(quality),
					key: 'quality',
					color: getVideoQualityColor(quality),
					id: uniqueId(),
				});
			}

			return result;
		}),
	};

	watch(getters.getMediaItems, () => actions.setFirstLetterIndex());

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
