<template>
	<v-container fluid>
		<!--	Custom FolderPaths	-->
		<template v-if="getFolderPaths.length > 0">
			<v-row v-for="folderPath in getFolderPaths" :key="folderPath.id" no-gutters>
				<v-col cols="3">
					<editable-text
						:value="folderPath.displayName"
						:disabled="!allowEditing"
						@save="saveDisplayName(folderPath.id, $event)"
					/>
				</v-col>
				<v-col cols>
					<p-text-field
						append-icon="mdi-folder-open"
						:value="folderPath.directory"
						readonly
						:disabled="!allowEditing"
						@click:append="openDirectoryBrowser(folderPath)"
					/>
				</v-col>
				<v-col cols="2">
					<!--	Is Valid Icon -->
					<valid-icon :valid="folderPath.isValid" :text="$t('general.alerts.invalid-directory')" />
					<!--	Delete Button -->
					<p-btn
						:button-type="deleteBtn"
						no-text
						:disabled="!allowEditing"
						:height="50"
						@click="deleteFolderPath(folderPath.id)"
					/>
				</v-col>
			</v-row>
		</template>
		<!--	No custom FolderPaths	Warning-->
		<template v-else>
			<v-row justify="center">
				<v-col cols="auto">
					<h2>{{ $t('components.paths-custom-overview.no-paths') }}</h2>
				</v-col>
			</v-row>
		</template>
		<!--	Add Path Button	-->
		<v-row justify="center">
			<v-col cols="1">
				<p-btn
					:button-type="addBtn"
					block
					:disabled="!allowEditing"
					:height="60"
					icon-size="40px"
					@click="addFolderPath"
				/>
			</v-col>
		</v-row>
		<!--	Directory Browser	-->
		<v-row>
			<v-col>
				<directory-browser ref="directoryBrowser" @confirm="confirmDirectoryBrowser" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Prop, Ref, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { FolderPathDTO, FolderType, PlexMediaType } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import { DownloadService, FolderPathService } from '@service';
import DirectoryBrowser from '@components/General/DirectoryBrowser.vue';

@Component
export default class PathsCustomOverview extends Vue {
	@Prop({ required: true, type: String })
	readonly folderType!: FolderType;

	folderPaths: FolderPathDTO[] = [];

	addBtn: ButtonType = ButtonType.Add;
	deleteBtn: ButtonType = ButtonType.Delete;

	allowEditing: boolean = true;

	@Ref('directoryBrowser')
	readonly directoryBrowserRef!: DirectoryBrowser;

	get getFolderPaths(): FolderPathDTO[] {
		return this.folderPaths.filter((x) => x.folderType === this.folderType && x.id >= 4);
	}

	get getMediaType(): PlexMediaType {
		switch (this.folderType) {
			case FolderType.TvShowFolder:
				return PlexMediaType.TvShow;

			case FolderType.MovieFolder:
				return PlexMediaType.Movie;

			case FolderType.MusicFolder:
				return PlexMediaType.Music;

			case FolderType.PhotosFolder:
				return PlexMediaType.Photos;

			case FolderType.GamesVideosFolder:
				return PlexMediaType.Games;
			default:
				Log.error(`PathsCustomOverview.vue => Failed to convert ${this.folderType} to PlexMediaType`);
				return PlexMediaType.Unknown;
		}
	}

	openDirectoryBrowser(path: FolderPathDTO): void {
		this.directoryBrowserRef.open(path);
	}

	confirmDirectoryBrowser(path: FolderPathDTO): void {
		const i = this.folderPaths.findIndex((x) => x.id === path.id);
		if (i > -1) {
			const folderPath = { ...this.folderPaths[i], directory: path.directory };
			FolderPathService.updateFolderPath(folderPath);
		}
	}

	addFolderPath(): void {
		FolderPathService.createFolderPath({
			id: 0,
			displayName: `${this.folderType.replace('Folder', ' Folder')} Path`,
			directory: '',
			folderType: this.folderType,
			mediaType: this.getMediaType,
			isValid: false,
		});
	}

	deleteFolderPath(id: number): void {
		FolderPathService.deleteFolderPath(id);
	}

	saveDisplayName(id: number, value: string): void {
		const folderPathIndex = this.folderPaths.findIndex((x) => x.id === id);
		if (folderPathIndex > -1) {
			const folderPath = { ...this.folderPaths[folderPathIndex], displayName: value };
			FolderPathService.updateFolderPath(folderPath);
		}
	}

	mounted(): void {
		useSubscription(
			FolderPathService.getFolderPaths().subscribe((data) => {
				this.folderPaths = data ?? [];
			}),
		);

		// Ensure there are no active downloads before being allowed to change.
		useSubscription(
			DownloadService.getActiveDownloadList().subscribe((data) => {
				this.allowEditing = data?.length === 0 ?? false;
			}),
		);
	}
}
</script>
