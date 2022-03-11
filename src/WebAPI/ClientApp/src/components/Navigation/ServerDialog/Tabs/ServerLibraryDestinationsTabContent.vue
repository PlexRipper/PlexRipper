<template>
	<v-simple-table class="section-table">
		<tbody>
			<!--	Download Destinations	-->
			<tr v-for="library in plexLibraries" :key="library.id">
				<td style="width: 50%"><media-type-icon :media-type="library.type" class="mx-3" />{{ library.title }}</td>
				<td>
					<p-select
						:value="library.defaultDestinationId"
						item-text="displayName"
						item-value="id"
						:items="getFolderPathOptions(library.type)"
						@change="updateDefaultDestination(library.id, $event)"
					/>
				</td>
			</tr>
		</tbody>
	</v-simple-table>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { FolderPathDTO, FolderType, PlexLibraryDTO, PlexMediaType } from '@dto/mainApi';
import { LibraryService } from '@service';

@Component<ServerLibraryDestinationsTabContent>({
	components: {},
})
export default class ServerLibraryDestinationsTabContent extends Vue {
	@Prop({ required: true, type: Array as () => PlexLibraryDTO[] })
	readonly plexLibraries!: PlexLibraryDTO[];

	@Prop({ required: true, type: Array as () => FolderPathDTO[] })
	readonly folderPaths!: FolderPathDTO[];

	getFolderPathOptions(type: PlexMediaType): FolderPathDTO[] {
		switch (type) {
			case PlexMediaType.Movie:
				return this.folderPaths.filter((x) => x.folderType === FolderType.MovieFolder);
			case PlexMediaType.TvShow:
				return this.folderPaths.filter((x) => x.folderType === FolderType.TvShowFolder);
			default:
				return this.folderPaths;
		}
	}

	updateDefaultDestination(libraryId: number, folderPathId: number): void {
		LibraryService.updateDefaultDestination(libraryId, folderPathId);
	}
}
</script>
