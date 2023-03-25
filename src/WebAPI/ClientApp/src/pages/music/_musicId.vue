<template>
	<page-container>
		<v-row>
			<v-col>
				<h1>{{ $t('pages.music._musicId.header') }}</h1>
			</v-col>
		</v-row>
	</page-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Music extends Vue {}
</script>
