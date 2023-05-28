<template>
	<q-page>
		<media-overview :library-id="libraryId" :media-id="mediaId" :media-type="PlexMediaType.TvShow" />
	</q-page>
</template>
<script setup lang="ts">
import { useRoute } from 'vue-router';
import { PlexMediaType } from '@dto/mainApi';
import { definePageMeta } from '#imports';

definePageMeta({
	scrollToTop: false,
});

const route = useRoute();

const mediaId = +route.params.tvShowId;
const libraryId = +route.params.libraryId;
</script>
