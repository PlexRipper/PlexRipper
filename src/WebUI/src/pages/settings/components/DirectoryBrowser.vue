<template>
	<v-row justify="start">
		<v-col>
			<v-dialog v-model="isDialogOpen" scrollable max-width="900px">
				<!-- The path box -->
				<template v-slot:activator="{ on }">
					<v-btn color="primary" dark v-on="on">{{ buttonText }}</v-btn>
				</template>

				<v-card style="position: absolute; top: 200px; bottom:200px;  max-width: 900px; max-height:900px">
					<v-card-title>
						<v-row>
							<v-col cols="12">
								<label>Select path</label>
							</v-col>
							<v-col cols="12">
								<v-text-field v-model="path" placeholder="Start typing or select a path below" @input="setPath($event)" />
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
							@click:row="handleClick"
						>
							<template v-slot:item.type="{ item }">
								<v-icon>{{ getIcon(item.type) }}</v-icon>
							</template>
						</v-data-table>
					</v-card-text>
					<v-card-actions class="justify-end">
						<v-btn color="blue darken-1" text @click="cancel()">Cancel</v-btn>
						<v-btn color="blue darken-1" text @click="ok()">Ok</v-btn>
					</v-card-actions>
				</v-card>
			</v-dialog>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Vue, Component, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify';
import IPath from '@dto/settings/iPath.ts';
import Log from 'consola';

@Component
export default class DirectoryBrowser extends Vue {
	@Prop({ required: false, type: String, default: 'Add Folder' })
	readonly buttonText!: string;

	isDialogOpen: boolean = false;

	parentPath: string = '';
	path: string = '';

	headers: DataTableHeader[] = [
		{
			text: 'Type',
			value: 'type',
			width: 60,
		},
		{
			text: 'Name',
			value: 'name',
		},
	];

	items: IPath[] = [];

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

	setPath(path: string): void {
		Log.debug(path);
	}

	ok(): void {
		this.$emit('new-path', this.path);
		this.cancel();
	}

	cancel(): void {
		this.path = '';
		this.isDialogOpen = false;
		this.requestDirectories(this.path);
	}

	handleClick(dataRow: IPath): void {
		if (dataRow.path === '..') {
			this.requestDirectories(this.parentPath);
		} else {
			this.requestDirectories(dataRow.path);
		}
	}

	requestDirectories(path: string): void {
		this.$axios.get(`/filesystem/?path=${path}`).then((response) => {
			this.items = response.data.directories;

			// Don't add return row if in the root folder
			if (path !== '') {
				this.items.unshift({
					type: {
						id: 0,
						type: 'none',
					},
					name: '...',
					path: '..',
				} as IPath);
			}
			this.parentPath = response.data.parent;
			this.path = path;
		});
	}

	mounted(): void {
		this.requestDirectories('');
	}
}
</script>
