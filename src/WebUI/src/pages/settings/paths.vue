<template>
	<v-container>
		<v-row v-for="(path, i) in paths" :key="i">
			<v-col cols="2">
				<v-subheader>{{ path.type }}</v-subheader>
			</v-col>
			<v-col cols="10">
				<v-text-field
					append-icon="mdi-folder-open"
					name="input-10-2"
					:value="path.directory"
					class="input-group--focused"
					solo
					readonly
					@click:append="openDirectoryBrowser(path)"
				></v-text-field>
			</v-col>
		</v-row>
		<v-row v-if="selectedPath">
			<v-col>
				<directory-browser
					:open="isDirectoryBrowserOpen"
					:path="selectedPath"
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
import DirectoryBrowser from './components/DirectoryBrowser.vue';

@Component({
	components: {
		DirectoryBrowser,
	},
})
export default class SettingsPaths extends Vue {
	moduleName: string = 'Settings Paths';

	paths: FolderPathDTO[] = [];

	isDirectoryBrowserOpen: boolean = false;

	selectedPath: FolderPathDTO | null = null;

	openDirectoryBrowser(path: FolderPathDTO): void {
		this.selectedPath = path;
		this.isDirectoryBrowserOpen = true;
	}

	confirmDirectoryBrowser(path: FolderPathDTO): void {
		this.selectedPath = path;
		Log.debug(path);
		this.isDirectoryBrowserOpen = false;

		updateFolderPath(path).subscribe((data) => {
			Log.debug(`Succesfully updated folderpath ${path.displayName}`, data);
		});
	}

	cancelDirectoryBrowser(): void {
		this.isDirectoryBrowserOpen = false;
	}

	created(): void {
		getFolderPaths().subscribe((data) => {
			this.paths = data;
			Log.debug(this.paths);
		});
	}
}
</script>
