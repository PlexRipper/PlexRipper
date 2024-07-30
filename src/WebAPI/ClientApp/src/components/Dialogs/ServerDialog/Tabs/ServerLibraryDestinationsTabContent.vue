<template>
	<!--	Download Destinations	-->
	<q-markup-table
		separator="horizontal"
		flat
	>
		<template v-if="plexLibraries.length">
			<q-tr
				v-for="library in plexLibraries"
				:key="library.id"
				style="margin: 4px 0"
			>
				<q-td>
					<QMediaTypeIcon
						:media-type="library.type"
						class="mx-3"
					/>
					{{ library.title }}
				</q-td>
				<q-td>
					<q-select
						:model-value="getDefaultDestination(library.id)"
						option-label="displayName"
						option-value="id"
						:options="folderPathStore.getFolderPathOptions(library.type)"
						@update:model-value="libraryStore.updateDefaultDestination(library.id, $event.id)"
					/>
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
import type { PlexLibraryDTO, PlexServerDTO } from '@dto';
import { useFolderPathStore } from '@store';

const folderPathStore = useFolderPathStore();
const libraryStore = useLibraryStore();
const props = defineProps<{
	plexServer: PlexServerDTO | null;
	plexLibraries: PlexLibraryDTO[];
}>();

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
		displayName: folderPathStore.getFolderPath(library.defaultDestinationId)?.displayName ?? 'Not set',
	};
}
</script>
