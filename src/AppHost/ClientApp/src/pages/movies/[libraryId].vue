<template>
	<QPage>
		<MediaOverview :library-id="libraryId" />
	</QPage>
</template>

<script setup lang="ts">
import { useRoute } from 'vue-router';

const route = useRoute();

const libraryId = +route.params.libraryId!;
</script>
