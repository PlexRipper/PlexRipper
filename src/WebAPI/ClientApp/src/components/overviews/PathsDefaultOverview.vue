<template>
	<v-container fluid>
		<!--	Show warning when no allowed to edit	-->
		<template v-if="!allowEditing">
			<v-row>
				<v-col>
					<v-alert border="bottom" colored-border type="warning" elevation="2">
						{{ $t('general.alerts.disabled-paths') }}
					</v-alert>
				</v-col>
			</v-row>
		</template>
		<!--	Default FolderPaths	-->
		<v-row v-for="folderPath in getFolderPaths" :key="folderPath.id" no-gutters>
			<v-col cols="3">
				<help-icon :help-id="toTranslation(folderPath.type)" />
			</v-col>
			<v-col>
				<p-text-field
					append-icon="mdi-folder-open"
					:value="folderPath.directory"
					readonly
					:disabled="!allowEditing"
					@click:append="openDirectoryBrowser(folderPath)"
				/>
			</v-col>
			<!--	Is Valid Icon -->
			<v-col cols="2">
				<valid-icon :valid="folderPath.isValid" :text="$t('general.alerts.invalid-directory')" />
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
import Log from 'consola';
import { Vue, Component } from 'vue-property-decorator';
import { FolderPathDTO } from '@dto/mainApi';
import { updateFolderPath } from '@api/pathApi';
import { kebabCase } from 'lodash';
import ButtonType from '@enums/buttonType';
import { DownloadService, FolderPathService } from '@service';

@Component
export default class PathsDefaultOverview extends Vue {
	folderPaths: FolderPathDTO[] = [];

	isDirectoryBrowserOpen: boolean = false;

	selectedFolderPath: FolderPathDTO | null = null;

	addBtn: ButtonType = ButtonType.Add;
	deleteBtn: ButtonType = ButtonType.Delete;

	allowEditing: boolean = true;

	get getFolderPaths(): FolderPathDTO[] {
		// The first 3 folderPaths are always the default ones.
		return this.folderPaths.filter((x) => x.id === 1 || x.id === 2 || x.id === 3);
	}

	openDirectoryBrowser(path: FolderPathDTO): void {
		this.selectedFolderPath = path;
		this.isDirectoryBrowserOpen = true;
	}

	confirmDirectoryBrowser(path: FolderPathDTO): void {
		this.selectedFolderPath = path;
		this.isDirectoryBrowserOpen = false;

		this.$subscribeTo(updateFolderPath(path), (data) => {
			if (data.isSuccess && data.value) {
				Log.debug(`Successfully updated folder path ${path.displayName}`, data.value);
				const i = this.folderPaths.findIndex((x) => x.id === data.value?.id);
				if (i > -1) {
					this.folderPaths.splice(i, 1, data.value);
				}
			}
		});
	}

	cancelDirectoryBrowser(): void {
		this.isDirectoryBrowserOpen = false;
	}

	toTranslation(type: string): string {
		return `help.settings.paths.${kebabCase(type)}`;
	}

	created(): void {
		this.$subscribeTo(FolderPathService.getFolderPaths(), (data) => {
			this.folderPaths = data ?? [];
		});

		// Ensure there are no active downloads before being allowed to change.
		this.$subscribeTo(DownloadService.getActiveDownloadList(), (data) => {
			Log.debug('data', data);
			this.allowEditing = data?.length === 0 ?? false;
		});
	}
}
</script>
