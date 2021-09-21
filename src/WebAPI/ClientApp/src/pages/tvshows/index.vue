<template>
	<page>
		<h1>TvShow Overview</h1>
		<p>Select a library from the left-hand side</p>
	</page>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import LoadingSpinner from '@components/LoadingSpinner.vue';

@Component({
	components: {
		LoadingSpinner,
	},
})
export default class TvShowOverview extends Vue {}
</script>
