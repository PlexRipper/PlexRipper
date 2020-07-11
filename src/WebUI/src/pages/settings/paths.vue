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
import IPath from '@dto/settings/iPath.ts';
import Log from 'consola';
import { getFolderPaths } from '@api/pathApi';
import DirectoryBrowser from './components/DirectoryBrowser.vue';
import FolderTable from './components/FolderTable.vue';

@Component({
	components: {
		DirectoryBrowser,
		FolderTable,
	},
})
export default class SettingsPaths extends Vue {
	moduleName: string = 'Settings Paths';

	paths: IPath[] = [];

	isDirectoryBrowserOpen: boolean = false;

	selectedPath: IPath | null = null;

	openDirectoryBrowser(path: IPath): void {
		this.selectedPath = path;
		this.isDirectoryBrowserOpen = true;
	}

	confirmDirectoryBrowser(path: IPath): void {
		this.selectedPath = path;
		Log.debug(path);
		this.isDirectoryBrowserOpen = false;
	}

	cancelDirectoryBrowser(): void {
		this.isDirectoryBrowserOpen = false;
	}

	async created(): Promise<void> {
		this.paths = await getFolderPaths();
		Log.debug(this.paths);
	}
}
</script>
