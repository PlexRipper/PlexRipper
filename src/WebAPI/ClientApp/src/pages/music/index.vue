<template>
	<page-container>
		<v-row>
			<v-col>
				<h1>{{ $t('pages.music.index.header') }}</h1>
			</v-col>
		</v-row>
	</page-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class MusicOverview extends Vue {}
</script>
