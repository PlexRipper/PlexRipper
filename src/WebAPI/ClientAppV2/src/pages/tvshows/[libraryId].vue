<template>
	<q-page>
		<media-overview :library-id="+$route.params.libraryId" :media-type="PlexMediaType.TvShow" />
	</q-page>
</template>
<script setup lang="ts">
import { PlexMediaType } from '@dto/mainApi';
</script>
