<template>
	<media-overview :library-id="libraryId" />
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class TvShowsDetail extends Vue {
	libraryId: number = +this.$route.params.id;

	mounted() {
		Log.info('route', this.$route);
	}
}
</script>
