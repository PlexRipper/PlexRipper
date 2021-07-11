<template>
	<v-container fluid>
		<v-row v-for="(folderPath, i) in folderPaths" :key="i" no-gutters>
			<v-col cols="3">
				<help-icon :help-id="toTranslation(folderPath.type)" />
			</v-col>
			<v-col>
				<v-text-field
					append-icon="mdi-folder-open"
					:value="folderPath.directory"
					:height="50"
					dense
					outlined
					solo
					readonly
					@click:append="openDirectoryBrowser(folderPath)"
				></v-text-field>
			</v-col>
			<v-col cols="1">
				<valid-icon :valid="folderPath.isValid" text="Directory is not a valid path!" />
			</v-col>
		</v-row>
		<v-row v-if="selectedFolderPath">
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
import { Vue, Component } from 'vue-property-decorator';
import { FolderPathDTO } from '@dto/mainApi';
import Log from 'consola';
import { getFolderPaths, updateFolderPath } from '@api/pathApi';
import ValidIcon from '@components/General/ValidIcon.vue';
import HelpIcon from '@components/Help/HelpIcon.vue';
import { kebabCase } from 'lodash';
import DirectoryBrowser from '../General/DirectoryBrowser.vue';

@Component({
	components: {
		DirectoryBrowser,
		ValidIcon,
		HelpIcon,
	},
})
export default class PathsOverview extends Vue {
	folderPaths: FolderPathDTO[] = [];

	isDirectoryBrowserOpen: boolean = false;

	selectedFolderPath: FolderPathDTO | null = null;

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
		this.$subscribeTo(getFolderPaths(), (data) => {
			if (data.isSuccess && data.value) {
				this.folderPaths = data.value;
			}
		});
	}
}
</script>
