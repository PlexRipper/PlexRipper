<template>
	<q-page>
		<media-overview :library-id="libraryId" />
	</q-page>
</template>

<script setup lang="ts">
const route = useRoute();

const libraryId = computed(() => +route.params.moviesId);
</script>
