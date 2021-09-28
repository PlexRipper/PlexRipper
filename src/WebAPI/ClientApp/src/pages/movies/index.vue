<template>
	<page-container>
		<h1>{{ $t('pages.movies.index.header') }}</h1>
		<p>{{ $t('pages.movies.index.sub-header') }}</p>
	</page-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class MoviesOverview extends Vue {}
</script>
