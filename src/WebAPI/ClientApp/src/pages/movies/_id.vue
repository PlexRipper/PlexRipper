<template>
	<media-overview :library-id="libraryId" />
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class MoviesDetail extends Vue {
	libraryId: number = +this.$route.params.id;
}
</script>
