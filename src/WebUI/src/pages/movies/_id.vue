<template>
	<media-overview :library-id="libraryId" />
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

import MediaOverview from '~/components/MediaOverview/MediaOverview.vue';

@Component<MoviesDetail>({
	components: {
		MediaOverview,
	},
})
export default class MoviesDetail extends Vue {
	libraryId: number = +this.$route.params.id;
}
</script>
