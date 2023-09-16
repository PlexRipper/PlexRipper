import { defineStore, acceptHMRUpdate } from 'pinia';
import { switchMap, tap } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { FolderPathDTO, FolderType, PlexMediaType } from '@dto/mainApi';
import { createFolderPath, deleteFolderPath, getFolderPaths, updateFolderPath } from '@api/pathApi';
import { useI18n } from '#build/imports';
import IFolderPathGroup from '@interfaces/IFolderPathGroup';
import ISetupResult from '@interfaces/service/ISetupResult';

export const useFolderPathStore = defineStore('FolderPathStore', () => {
	const state = reactive<{ folderPaths: FolderPathDTO[] }>({
		folderPaths: [],
	});

	// Actions
	const actions = {
		setup(): Observable<ISetupResult> {
			return actions.refreshFolderPaths().pipe(switchMap(() => of({ name: useFolderPathStore.name, isSuccess: true })));
		},
		refreshFolderPaths() {
			return getFolderPaths().pipe(
				tap((result) => {
					if (result.isSuccess && result.value) {
						state.folderPaths = result.value ?? [];
					}
				}),
			);
		},
		createFolderPath(folderPath: FolderPathDTO) {
			return createFolderPath(folderPath).pipe(
				tap((result) => {
					if (result?.isSuccess && result.value) {
						state.folderPaths.push(result.value);
					}
				}),
			);
		},
		updateFolderPath(folderPath: FolderPathDTO) {
			const i = state.folderPaths.findIndex((x) => x.id === folderPath.id);
			if (i > -1) {
				state.folderPaths.splice(i, 1, folderPath);
			}
			return updateFolderPath(folderPath);
		},
		deleteFolderPath(folderPathId: number) {
			const i = state.folderPaths.findIndex((x) => x.id === folderPathId);
			if (i > -1) {
				state.folderPaths.splice(i, 1);
			}
			return deleteFolderPath(folderPathId);
		},
	};

	// Getters
	const getters = {
		getFolderPaths: computed((state): FolderPathDTO[] => state.folderPaths),
		getFolderPath: (id: number): FolderPathDTO | undefined => {
			return state.folderPaths.find((x) => x.id === id);
		},
		getFolderPathOptions: (type: PlexMediaType): FolderPathDTO[] => {
			if (type === PlexMediaType.Movie || type === PlexMediaType.TvShow) {
				return state.folderPaths.filter((x) => x.mediaType === type);
			}

			return state.folderPaths;
		},
		getFolderPathsGroups: (onlyDefaults: boolean) => {
			const { t } = useI18n();
			const folderPathGroups: IFolderPathGroup[] = [];
			// Default Paths
			folderPathGroups.push({
				header: t('components.folder-paths-overview.main.header'),
				// The first 3 folderPaths are always the default ones.
				paths: state.folderPaths.filter((x) => x.id === 1 || x.id === 2 || x.id === 3),
				mediaType: PlexMediaType.None,
				folderType: FolderType.None,
				IsFolderDeletable: false,
				isFolderNameEditable: false,
				isFolderAddable: false,
			});

			if (onlyDefaults) {
				return folderPathGroups;
			}

			// Movie Paths
			folderPathGroups.push({
				header: t('components.folder-paths-overview.movie.header'),
				paths: state.folderPaths.filter(
					(x) => x.folderType === FolderType.MovieFolder && !folderPathGroups[0].paths.some((y) => y.id === x.id),
				),
				mediaType: PlexMediaType.Movie,
				folderType: FolderType.MovieFolder,
				IsFolderDeletable: true,
				isFolderNameEditable: true,
				isFolderAddable: true,
			});

			// TvShow Paths
			folderPathGroups.push({
				header: t('components.folder-paths-overview.tv-show.header'),
				paths: state.folderPaths.filter(
					(x) => x.folderType === FolderType.TvShowFolder && !folderPathGroups[0].paths.some((y) => y.id === x.id),
				),
				mediaType: PlexMediaType.TvShow,
				folderType: FolderType.TvShowFolder,
				IsFolderDeletable: true,
				isFolderNameEditable: true,
				isFolderAddable: true,
			});

			return folderPathGroups;
		},
	};

	return {
		...toRefs(state),
		...actions,
		...getters,
	};
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useFolderPathStore, import.meta.hot));
}
