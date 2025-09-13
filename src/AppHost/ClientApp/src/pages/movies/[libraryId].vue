<template>
	<QPage>
		<MediaOverview

			:library-id="libraryId"
			:media-type="PlexMediaType.Movie" />
	</QPage>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import { PlexMediaType } from '@dto';

const route = useRoute();

const libraryId = +route.params.libraryId;
</script>
