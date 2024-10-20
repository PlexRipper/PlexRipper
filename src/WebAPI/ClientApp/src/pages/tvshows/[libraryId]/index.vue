<template>
	<QPage>
		<MediaOverview
			:library-id="libraryId"
			:media-type="PlexMediaType.TvShow" />
	</QPage>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';
import { PlexMediaType } from '@dto';
import { definePageMeta } from '#imports';

definePageMeta({
	scrollToTop: false,
});

const route = useRoute();

const libraryId = +route.params.libraryId;
</script>
