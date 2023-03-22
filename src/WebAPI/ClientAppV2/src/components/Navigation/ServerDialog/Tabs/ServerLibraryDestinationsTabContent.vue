<template>
	<!--	Download Destinations	-->
	<q-markup-table separator="horizontal" flat>
		<q-tr v-for="library in plexLibraries" :key="library.id" style="margin: 4px 0">
			<q-td>
				<q-media-type-icon :media-type="library.type" class="mx-3" />
				{{ library.title }}
			</q-td>
			<q-td>
				<p-select
					:model-value="getDefaultDestination(library.id)"
					option-label="displayName"
					option-value="id"
					dense
					:options="getFolderPathOptions(library.type)"
					@update:model-value="updateDefaultDestination(library.id, $event.id)" />
			</q-td>
		</q-tr>
	</q-markup-table>
</template>

<script setup lang="ts">
import { defineProps } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { FolderPathDTO, FolderType, PlexLibraryDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import { FolderPathService, LibraryService } from '@service';
import { ref } from '#imports';

const props = defineProps<{
	plexServer: PlexServerDTO;
	plexLibraries: PlexLibraryDTO[];
}>();

const folderPaths = ref<FolderPathDTO[]>([]);

function getFolderPathOptions(type: PlexMediaType): FolderPathDTO[] {
	switch (type) {
		case PlexMediaType.Movie:
			return folderPaths.value.filter((x) => x.folderType === FolderType.MovieFolder);
		case PlexMediaType.TvShow:
			return folderPaths.value.filter((x) => x.folderType === FolderType.TvShowFolder);
		default:
			return folderPaths.value;
	}
}

function getDefaultDestination(libraryId: number): { id: number; displayName: string } {
	const library = props.plexLibraries.find((x) => x.id === libraryId);
	if (!library) {
		return {
			id: 0,
			displayName: 'Library not found',
		};
	}

	return {
		id: library.defaultDestinationId,
		displayName: folderPaths.value.find((x) => x.id === library.defaultDestinationId)?.displayName ?? 'Not set',
	};
}

function updateDefaultDestination(libraryId: number, folderPathId: number): void {
	LibraryService.updateDefaultDestination(libraryId, folderPathId);
}

onMounted(() => {
	useSubscription(
		FolderPathService.getFolderPaths().subscribe((folderPathsData) => {
			folderPaths.value = folderPathsData;
		}),
	);
});
</script>
