<template>
	<media-overview :library-id="+$route.params.libraryId" :media-type="PlexMediaType.TvShow" />
</template>
<script setup lang="ts">
import { PlexMediaType } from '@dto/mainApi';
</script>
