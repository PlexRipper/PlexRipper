import Log from 'consola';
import { cloneDeep, isNumber, orderBy, sortBy, uniqueId } from 'lodash-es';
import { format } from 'date-fns';
import { acceptHMRUpdate, defineStore } from 'pinia';
import { computed, reactive, toRefs } from 'vue';
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
import { type IMetaDataMediaFilter, type ISelection, type ISortOption, StoreNames } from '@interfaces';
import { plexLibraryApi, plexMediaApi } from '@api';
import { map, tap } from 'rxjs/operators';
import { defer, forkJoin, type Observable, of } from 'rxjs';
import { useLibraryStore, useSettingsStore } from '@store';
import { getHighestQuality, getHighestQualityRank, getVideoQualityColor, translateVideoQuality } from '@composables';
import { useSubscription } from '@vueuse/rxjs';
import { useI18n } from 'vue-i18n';

interface IMediaOverviewStoreState {
	libraryId: number;
	items: Readonly<PlexMediaSlimDTO[]>;
	sortedItems: Readonly<PlexMediaSlimDTO[]>;
	itemsLength: number;
	sortedState: IMediaOverviewSort;
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
		sortedState: { field: MediaSortField.Title, sort: SortDirection.Asc },
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
						actions.sortMedia(state.sortedState);
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
		setMediaIndexNavigationOptions() {
			const items = get(getters.getMediaItems);
			const activeSort = get(getters.getActiveSort);
			const field = activeSort.field;
			const direction = activeSort.sort;

			const GB = 1_073_741_824;

			const keySelector: (item: PlexMediaSlimDTO) => string | null = (() => {
				switch (field) {
					case MediaSortField.Year:
						return (it) => String(it.year ?? '#');

					case MediaSortField.Quality:
						return (it) => translateVideoQuality(getHighestQuality(it));

					case MediaSortField.Duration:
						return (it) => {
							const seconds = it.duration ?? 0;
							const start = Math.floor(seconds / 600) * 10; // 10-min buckets
							return `${start}–${start + 10} min`;
						};

					case MediaSortField.AddedAt:
						return (it) => {
							const raw = it.addedAt;
							return raw ? format(new Date(raw), 'MMM yyyy') : null;
						};

					case MediaSortField.UpdatedAt:
						return (it) => {
							const raw = it.updatedAt;
							return raw ? format(new Date(raw), 'MMM yyyy') : null;
						};

					case MediaSortField.MediaSize:
						return (it) => {
							const bytes = it.mediaSize ?? 0;
							const start = Math.floor(bytes / GB);
							return `${start}–${start + 1} GB`;
						};

					default:
						return (it) => {
							const first = (it.title ?? '').trim().charAt(0).toUpperCase();
							return /^[A-Z]$/.test(first) ? first : '#';
						};
				}
			})();

			const indexByKey = new Map<string, number>();
			for (let i = 0; i < items.length; i++) {
				const key = keySelector(items[i]!);
				if (key == null) continue; // skip items without a usable key (e.g. no date)
				if (!indexByKey.has(key)) indexByKey.set(key, i);
			}

			const keys = Array.from(indexByKey.keys());

			// Title nav should be alphabetical, not insertion order
			const isTitle = field === MediaSortField.Title; // this is 'sortIndex' in your enum

			function sortAlphaKeys(keys: string[], direction: SortDirection) {
				const hash = keys.includes('#') ? ['#'] : [];
				const letters = keys.filter((k) => k !== '#').sort((a, b) => a.localeCompare(b));
				if (direction === SortDirection.Desc) letters.reverse();
				return [...hash, ...letters];
			}

			state.scrollAlphabet = isTitle
				? sortAlphaKeys(keys, direction)
				: direction === SortDirection.Desc
					? [...keys].reverse()
					: keys;

			state.scrollDict = Object.fromEntries(indexByKey);
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
			state.sortedState = { field: MediaSortField.Title, sort: SortDirection.Asc };
			state.sortedItems = [];
		},
		clearFilter() {
			state.filterQuery = '';
		},
		toggleSortMedia(field: MediaSortField) {
			if (state.sortedState.field === field) {
				state.sortedState.sort = state.sortedState.sort === SortDirection.Asc ? SortDirection.Desc : SortDirection.Asc;
			} else {
				state.sortedState = { field, sort: SortDirection.Asc };
			}

			actions.sortMedia(state.sortedState);
		},
		sortMedia(event: IMediaOverviewSort) {
			Log.debug('Sorting media with event', event);

			if (event.sort === SortDirection.NoSort) {
				state.sortedItems = [];
				state.sortedState = event;
				return;
			}

			const order = event.sort === SortDirection.Asc ? 'asc' : 'desc';

			// Quality is not a direct field on PlexMediaSlimDTO; sort by derived rank
			if (event.field === MediaSortField.Quality) {
				state.sortedItems = Object.freeze(orderBy(state.items, [(item) => getHighestQualityRank(item)], [order]));
			} else {
				state.sortedItems = Object.freeze(orderBy(state.items, [event.field as keyof PlexMediaSlimDTO], [order]));
			}

			state.sortedState = event;
		},
		$reset() {
			Object.assign(state, cloneDeep(defaultState));
		},
	};

	const getters = {
		hasSelectedMedia: computed((): boolean => {
			return state.selection.keys.length > 0;
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
			const query = state.filterQuery.toLowerCase();
			if (get(getters.getIsSorted)) {
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
		getActiveSort: computed((): IMediaOverviewSort => {
			return state.sortedState;
		}),
		getIsSorted: computed((): boolean => {
			if (state.sortedState.sort === SortDirection.NoSort) {
				return false;
			}
			return !(state.sortedState.field === MediaSortField.Title && state.sortedState.sort === SortDirection.Asc);
		}),
		getSortOptions: computed((): ISortOption[] => {
			const { t } = useI18n();
			const options: ISortOption[] = [
				{ field: MediaSortField.Title, label: t('general.sort.title'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Year, label: t('general.sort.year'), direction: SortDirection.NoSort },
				{ field: MediaSortField.AddedAt, label: t('general.sort.added-at'), direction: SortDirection.NoSort },
				{ field: MediaSortField.UpdatedAt, label: t('general.sort.updated-at'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Duration, label: t('general.sort.duration'), direction: SortDirection.NoSort },
				{ field: MediaSortField.MediaSize, label: t('general.sort.size'), direction: SortDirection.NoSort },
				{ field: MediaSortField.Quality, label: t('general.sort.quality'), direction: SortDirection.NoSort },
			];

			for (const option of options) {
				if (option.field === state.sortedState.field) {
					option.direction = state.sortedState.sort;
				}
			}

			return options;
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

	watch(getters.getMediaItems, () => actions.setMediaIndexNavigationOptions());

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useMediaOverviewStore, import.meta.hot));
}
