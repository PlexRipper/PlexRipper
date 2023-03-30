<template>
	<q-page>
		<media-overview :library-id="+$route.params.libraryId" :media-type="PlexMediaType.Movie" />
	</q-page>
</template>
<script setup lang="ts">
import { PlexMediaType } from '@dto/mainApi';
</script>
