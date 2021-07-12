<template>
	<v-container fluid>
		<!--	Default FolderPaths	-->
		<v-row v-for="folderPath in folderPaths" :key="folderPath.id" no-gutters>
			<v-col cols="3">
				<help-icon :help-id="toTranslation(folderPath.type)" />
			</v-col>
			<v-col>
				<p-text-field
					append-icon="mdi-folder-open"
					:value="folderPath.directory"
					readonly
					@click:append="openDirectoryBrowser(folderPath)"
				/>
			</v-col>
			<!--	Is Valid Icon -->
			<v-col cols="2">
				<valid-icon :valid="folderPath.isValid" :text="$t('general.alerts.invalid-directory')" />
			</v-col>
		</v-row>
		<!--	Custom FolderPaths	-->
		<v-row v-for="folderPath in customFolderPaths" :key="folderPath.id" no-gutters>
			<v-col cols="3">
				<editable-text :value="folderPath.displayName" @save="saveDisplayName(folderPath.id, $event)" />
			</v-col>
			<v-col cols>
				<p-text-field
					append-icon="mdi-folder-open"
					:value="folderPath.directory"
					readonly
					@click:append="openDirectoryBrowser(folderPath)"
				/>
			</v-col>
			<!--	Is Valid Icon -->
			<v-col cols="2">
				<valid-icon :valid="folderPath.isValid" :text="$t('general.alerts.invalid-directory')" />
				<p-btn :button-type="deleteBtn" no-text :height="50" @click="deleteFolderPath(folderPath.id)" />
			</v-col>
		</v-row>
		<!--	Add Path Button	-->
		<v-row justify="center">
			<v-col cols="4">
				<p-btn :button-type="addBtn" block :height="50" @click="addFolderPath" />
			</v-col>
		</v-row>
		<!--	Directory Browser	-->
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
import Log from 'consola';
import { Vue, Component } from 'vue-property-decorator';
import { FolderPathDTO } from '@dto/mainApi';
import { getFolderPaths, updateFolderPath } from '@api/pathApi';
import ValidIcon from '@components/General/ValidIcon.vue';
import HelpIcon from '@components/Help/HelpIcon.vue';
import { kebabCase } from 'lodash';
import EditableText from '@components/Form/EditableText.vue';
import ButtonType from '@enums/buttonType';
import DirectoryBrowser from '../General/DirectoryBrowser.vue';

@Component({
	components: {
		DirectoryBrowser,
		EditableText,
		ValidIcon,
		HelpIcon,
	},
})
export default class PathsOverview extends Vue {
	folderPaths: FolderPathDTO[] = [];
	customFolderPaths: FolderPathDTO[] = [];

	isDirectoryBrowserOpen: boolean = false;

	selectedFolderPath: FolderPathDTO | null = null;

	addBtn: ButtonType = ButtonType.Add;
	deleteBtn: ButtonType = ButtonType.Delete;

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

	addFolderPath(): void {
		this.customFolderPaths.push({
			id: this.folderPaths.length + this.customFolderPaths.length,
			displayName: 'Custom Path',
			directory: '',
			type: 'CustomFolder',
			isValid: false,
		});
	}

	deleteFolderPath(id: number): void {
		const folderPathIndex = this.customFolderPaths.findIndex((x) => x.id === id);
		if (folderPathIndex > -1) {
			this.customFolderPaths.splice(folderPathIndex, 1);
		}
	}

	saveDisplayName(id: number, value: string): void {
		const folderPathIndex = this.customFolderPaths.findIndex((x) => x.id === id);
		if (folderPathIndex > -1) {
			const folderPath = { ...this.customFolderPaths[folderPathIndex], displayName: value };
			this.customFolderPaths.splice(folderPathIndex, 1, folderPath);
		}
	}

	created(): void {
		this.$subscribeTo(getFolderPaths(), (data) => {
			if (data.isSuccess && data.value) {
				this.folderPaths = data.value;
			}
		});

		this.customFolderPaths.push({
			id: this.folderPaths.length + this.customFolderPaths.length - 1,
			displayName: 'Test',
			directory: 'D:\\PlexDownloadFolder\\',
			type: 'CustomFolder',
			isValid: true,
		});
	}
}
</script>
