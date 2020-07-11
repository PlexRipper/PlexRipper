<template>
	<v-row>
		<v-col cols="12">
			<v-data-table
				:headers="headers"
				:items="items"
				hide-default-footer
				fixed-header
				disable-sort
				disable-pagination
				disable-filtering
			>
				<template v-slot:item.type="{ item }">
					<v-icon>{{ getIcon(item.type) }}</v-icon>
				</template>
				<template v-slot:item.actions="{ item }">
					<p class="text-center ma-0 pointer" @click="deletePath(item)">
						<v-icon>
							mdi-delete
						</v-icon>
					</p>
				</template>
			</v-data-table>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Vue, Component, Prop } from 'vue-property-decorator';
import { DataTableHeader } from 'vuetify';
import IPath from '@dto/settings/iPath.ts';

@Component
export default class FolderTable extends Vue {
	@Prop({ required: true, type: Array as () => IPath[], default: [] })
	readonly items!: IPath[];

	headers: DataTableHeader[] = [
		{
			text: 'Path',
			value: 'path',
		},
		{
			text: 'Free Space',
			value: 'freespace',
			width: 100,
		},
		{
			text: 'Unmapped Folders',
			value: 'unmappedfolders',
			width: 150,
		},
		{
			text: '',
			value: 'actions',
			width: 100,
		},
	];

	deletePath(path: IPath): void {
		this.$emit('delete-path', path);
	}
}
</script>
