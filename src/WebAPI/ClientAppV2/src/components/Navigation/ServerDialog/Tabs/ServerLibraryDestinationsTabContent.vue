<template>
	<table class="section-table">
		<tbody>
			<!--	Download Destinations	-->
			<tr v-for="library in plexLibraries" :key="library.id">
				<td style="width: 50%">
					<q-media-type-icon :media-type="library.type" class="mx-3" />
					{{ library.title }}
				</td>
				<td>
					<q-select
						:value="library.defaultDestinationId"
						item-text="displayName"
						item-value="id"
						:items="getFolderPathOptions(library.type)"
						@change="updateDefaultDestination(library.id, $event)" />
				</td>
			</tr>
		</tbody>
	</table>
</template>

<script setup lang="ts">
import { FolderPathDTO, FolderType, PlexLibraryDTO, PlexMediaType } from '@dto/mainApi';
import { LibraryService } from '@service';

const props = defineProps<{ plexLibraries: PlexLibraryDTO[]; folderPaths: FolderPathDTO[] }>();

function getFolderPathOptions(type: PlexMediaType): FolderPathDTO[] {
	switch (type) {
		case PlexMediaType.Movie:
			return props.folderPaths.filter((x) => x.folderType === FolderType.MovieFolder);
		case PlexMediaType.TvShow:
			return props.folderPaths.filter((x) => x.folderType === FolderType.TvShowFolder);
		default:
			return props.folderPaths;
	}
}

function updateDefaultDestination(libraryId: number, folderPathId: number): void {
	LibraryService.updateDefaultDestination(libraryId, folderPathId);
}
</script>
