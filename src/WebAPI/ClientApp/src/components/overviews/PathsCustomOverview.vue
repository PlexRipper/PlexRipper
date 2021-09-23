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
					<h2>No paths are set.</h2>
				</v-col>
			</v-row>
		</template>
		<!--	Add Path Button	-->
		<v-row justify="center">
			<v-col cols="1">
				<p-btn :button-type="addBtn" block :disabled="!allowEditing" :height="60" icon-size="40px" @click="addFolderPath" />
			</v-col>
		</v-row>
		<!--	Directory Browser	-->
		<v-row v-if="selectedFolderPath && allowEditing">
			<v-col>
				<directory-browser
					:open="isDirectoryBrowserOpen"
					:path="selectedFolderPath"
					@confirm="confirmDirectoryBrowser"
					@cancel="cancelDirectoryBrowser"
				/>
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Vue, Component, Prop } from 'vue-property-decorator';
import { FolderPathDTO, FolderType } from '@dto/mainApi';
import ButtonType from '@enums/buttonType';
import { DownloadService, FolderPathService } from '@service';

@Component
export default class PathsCustomOverview extends Vue {
	@Prop({ required: true, type: String })
	readonly folderType!: FolderType;

	folderPaths: FolderPathDTO[] = [];

	isDirectoryBrowserOpen: boolean = false;

	selectedFolderPath: FolderPathDTO | null = null;

	addBtn: ButtonType = ButtonType.Add;
	deleteBtn: ButtonType = ButtonType.Delete;

	allowEditing: boolean = true;

	get getFolderPaths(): FolderPathDTO[] {
		return this.folderPaths.filter((x) => x.folderType === this.folderType && x.id >= 4);
	}

	openDirectoryBrowser(path: FolderPathDTO): void {
		this.selectedFolderPath = path;
		this.isDirectoryBrowserOpen = true;
	}

	confirmDirectoryBrowser(path: FolderPathDTO): void {
		this.isDirectoryBrowserOpen = false;
		this.selectedFolderPath = null;
		const i = this.folderPaths.findIndex((x) => x.id === path.id);
		if (i > -1) {
			const folderPath = { ...this.folderPaths[i], directory: path.directory };
			FolderPathService.updateFolderPath(folderPath);
		}
	}

	cancelDirectoryBrowser(): void {
		this.isDirectoryBrowserOpen = false;
	}

	addFolderPath(): void {
		FolderPathService.createFolderPath({
			id: 0,
			displayName: `${this.folderType.replace('Folder', ' Folder')} Path`,
			directory: '',
			folderType: this.folderType,
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
		this.$subscribeTo(FolderPathService.getFolderPaths(), (data) => {
			this.folderPaths = data ?? [];
		});

		// Ensure there are no active downloads before being allowed to change.
		this.$subscribeTo(DownloadService.getActiveDownloadList(), (data) => {
			this.allowEditing = data?.length === 0 ?? false;
		});
	}
}
</script>
