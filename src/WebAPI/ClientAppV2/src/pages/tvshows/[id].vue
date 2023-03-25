<template>
	<media-overview :library-id="libraryId" />
</template>

<script setup lang="ts">
const route = useRoute();

const libraryId = computed(() => +route.params.moviesId);
</script>
