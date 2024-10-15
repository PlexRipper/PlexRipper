<template>
	<QPage>
		<QRow>
			<QCol>
				<QAlert type="error">
					<QText
						align="center"
						size="h4">
						{{ $t('pages.unknown.index.header', {
							libraryName: libraryStore.getLibraryName(libraryId),
						}) }}
					</QText>
				</QAlert>
			</QCol>
		</QRow>
	</QPage>
</template>

<script lang="ts" setup>
import { useRoute } from 'vue-router';
import { useLibraryStore } from '#imports';

const route = useRoute();
const libraryStore = useLibraryStore();

const libraryId = +route.params.libraryId;
</script>
