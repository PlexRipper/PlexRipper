<template>
	<page-container>
		<h1>{{ $t('pages.tvshows.index.header') }}</h1>
		<p>{{ $t('pages.tvshows.index.sub-header') }}</p>
	</page-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class TvShowOverview extends Vue {}
</script>
