<template>
	<!--	Download Destinations	-->
	<q-markup-table separator="horizontal" flat>
		<template v-if="plexLibraries.length">
			<q-tr v-for="library in plexLibraries" :key="library.id" style="margin: 4px 0">
				<q-td>
					<q-media-type-icon :media-type="library.type" class="mx-3" />
					{{ library.title }}
				</q-td>
				<q-td>
					<q-select
						:model-value="getDefaultDestination(library.id)"
						option-label="displayName"
						option-value="id"
						:options="getFolderPathOptions(library.type)"
						@update:model-value="updateDefaultDestination(library.id, $event.id)" />
				</q-td>
			</q-tr>
		</template>
		<template v-else>
			<q-tr>
				<q-td>
					<h3>
						{{ $t('components.server-library-destinations-tab-content.no-libraries') }}
					</h3>
				</q-td>
			</q-tr>
		</template>
	</q-markup-table>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { FolderPathDTO, PlexLibraryDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import { FolderPathService, LibraryService } from '@service';
import { ref } from '#imports';

const props = defineProps<{
	plexServer: PlexServerDTO | null;
	plexLibraries: PlexLibraryDTO[];
}>();

const folderPaths = ref<FolderPathDTO[]>([]);

function getFolderPathOptions(type: PlexMediaType): FolderPathDTO[] {
	if (type === PlexMediaType.Movie || type === PlexMediaType.TvShow) {
		return get(folderPaths).filter((x) => x.mediaType === type);
	}

	return get(folderPaths);
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
		displayName: get(folderPaths).find((x) => x.id === library.defaultDestinationId)?.displayName ?? 'Not set',
	};
}

function updateDefaultDestination(libraryId: number, folderPathId: number): void {
	LibraryService.updateDefaultDestination(libraryId, folderPathId);
}

onMounted(() => {
	useSubscription(
		FolderPathService.getFolderPaths().subscribe((folderPathsData) => {
			set(folderPaths, folderPathsData);
		}),
	);
});
</script>
