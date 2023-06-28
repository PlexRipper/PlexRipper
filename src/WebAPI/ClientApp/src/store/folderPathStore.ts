import { defineStore, acceptHMRUpdate } from 'pinia';
import { FolderPathDTO, FolderType, PlexMediaType } from '@dto/mainApi';
import { createFolderPath, deleteFolderPath, getFolderPaths, updateFolderPath } from '@api/pathApi';
import { useI18n } from '#build/imports';
import IFolderPathGroup from '@interfaces/IFolderPathGroup';

export const useFolderPathStore = defineStore('FolderPathStore', {
	state: (): { folderPaths: FolderPathDTO[] } => ({
		folderPaths: [],
	}),
	actions: {
		setup() {
			this.refreshFolderPaths();
		},
		refreshFolderPaths() {
			getFolderPaths().subscribe((folderPaths) => {
				if (folderPaths.isSuccess) {
					this.folderPaths = folderPaths.value ?? [];
				}
			});
		},
		createFolderPath(folderPath: FolderPathDTO): void {
			createFolderPath(folderPath).subscribe((folderPath) => {
				if (folderPath?.isSuccess && folderPath.value) {
					this.folderPaths.push(folderPath.value);
				}
			});
		},
		updateFolderPath(folderPath: FolderPathDTO): void {
			const i = this.folderPaths.findIndex((x) => x.id === folderPath.id);
			if (i > -1) {
				this.folderPaths.splice(i, 1, folderPath);
			}
			updateFolderPath(folderPath).subscribe();
		},
		deleteFolderPath(folderPathId: number): void {
			const i = this.folderPaths.findIndex((x) => x.id === folderPathId);
			if (i > -1) {
				this.folderPaths.splice(i, 1);
			}
			deleteFolderPath(folderPathId).subscribe();
		},
	},
	getters: {
		getFolderPaths(state): FolderPathDTO[] {
			return state.folderPaths;
		},
		getFolderPath:
			(state) =>
			(id: number): FolderPathDTO | undefined => {
				return state.folderPaths.find((x) => x.id === id);
			},
		getFolderPathOptions:
			(state) =>
			(type: PlexMediaType): FolderPathDTO[] => {
				if (type === PlexMediaType.Movie || type === PlexMediaType.TvShow) {
					return state.folderPaths.filter((x) => x.mediaType === type);
				}

				return state.folderPaths;
			},
		getFolderPathsGroups: (state) => (onlyDefaults: boolean) => {
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
	},
});

if (import.meta.hot) {
	import.meta.hot.accept(acceptHMRUpdate(useFolderPathStore, import.meta.hot));
}
