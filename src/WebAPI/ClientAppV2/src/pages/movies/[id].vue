<template>
	<q-page>
		<media-overview :library-id="+$route.params.id" :media-type="PlexMediaType.Movie" />
	</q-page>
</template>
<script setup lang="ts">
import { PlexMediaType } from '@dto/mainApi';
</script>
