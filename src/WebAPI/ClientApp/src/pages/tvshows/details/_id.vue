<template>
	<span> {{ mediaId }} </span>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

import MediaOverview from '@mediaOverview/MediaOverview.vue';

@Component({
	components: {
		MediaOverview,
	},
})
export default class TvShowsDetail extends Vue {
	mediaId: number = +this.$route.params.id;
}
</script>
