<template>
	<v-row justify="start">
		<v-col>
			<v-dialog :value="open" persistent scrollable max-width="900px">
				<v-card style="position: absolute; top: 200px; bottom:200px;  max-width: 900px; max-height:900px">
					<v-card-title>
						<v-row>
							<v-col cols="12">
								<label>Select {{ path.type }}</label>
							</v-col>
							<v-col cols="12">
								<v-text-field
									v-model="newDirectory"
									placeholder="Start typing or select a path below"
									@input="newDirectory = $event"
								/>
							</v-col>
						</v-row>
					</v-card-title>
					<v-divider />
					<v-card-text style="height: 300px;">
						<v-data-table
							:headers="headers"
							:items="items"
							hide-default-footer
							fixed-header
							disable-sort
							disable-pagination
							disable-filtering
							item-class="class"
							@click:row="directoryNavigate"
						>
							<template v-slot:item.type="{ item }">
								<v-icon>{{ getIcon(item.type) }}</v-icon>
							</template>
						</v-data-table>
					</v-card-text>
					<v-card-actions class="justify-end">
						<v-btn color="blue darken-1" text @click="cancel()">Cancel</v-btn>
						<v-btn color="blue darken-1" text @click="confirm()">Ok</v-btn>
					</v-card-actions>
				</v-card>
			</v-dialog>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Vue, Component, Prop, Watch } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify';
import IFolderPath from '@dto/settings/iFolderPath';
import { getDirectoryPath } from '@api/pathApi';
import { IDirectory } from '@dto/settings/paths/IFileSystem';

@Component
export default class DirectoryBrowser extends Vue {
	@Prop({ required: false, type: Object as () => IFolderPath })
	readonly path!: IFolderPath;

	@Prop({ required: true, type: Boolean })
	readonly open!: boolean;

	parentPath: string = '';
	newDirectory: string = '';

	items: IDirectory[] = [];

	headers: DataTableHeader[] = [
		{
			text: 'Type',
			value: 'type',
			width: 60,
			class: 'directory-row',
		},
		{
			text: 'Name',
			value: 'name',
			class: 'directory-row',
		},
	];

	getIcon(type: string): string {
		switch (type) {
			case 'drive':
				return 'mdi-folder';
			case 'folder':
				return 'mdi-folder';
			case 'file':
				return 'mdi-file';
			case 'return':
				return 'mdi-arrow-up-bold-outline';

			default:
				return 'crosshairs-question';
		}
	}

	@Watch('open', { immediate: true, deep: true })
	onDialogOpen(val: boolean): void {
		if (val) {
			this.newDirectory = this.path.directory;
			this.requestDirectories(this.newDirectory);
		}
	}

	confirm(): void {
		this.path.directory = this.newDirectory;
		this.$emit('confirm', this.path);
	}

	cancel(): void {
		this.$emit('cancel');
	}

	directoryNavigate(dataRow: IDirectory): void {
		if (dataRow.path === '..') {
			this.requestDirectories(this.parentPath);
		} else {
			this.requestDirectories(dataRow.path);
		}
	}

	requestDirectories(path: string): void {
		getDirectoryPath(path).subscribe((data) => {
			this.items = data.directories;

			// Don't add return row if in the root folder
			if (path !== '') {
				this.items.unshift({
					name: '...',
					path: '..',
					type: 0,
				});
			}
			this.parentPath = data.parent;
			this.newDirectory = path;
		});
	}
}
</script>
