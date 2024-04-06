<template>
	<q-page>
		<media-overview :library-id="libraryId" :media-id="mediaId" :media-type="PlexMediaType.Movie" />
	</q-page>
</template>
<script setup lang="ts">
import { useRoute } from 'vue-router';
import { PlexMediaType } from '@dto';

const route = useRoute();

const mediaId = +route.params.tvShowId;
const libraryId = +route.params.libraryId;
</script>
