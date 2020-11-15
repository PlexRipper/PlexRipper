<template>
	<v-row no-gutters style="width: 100%">
		<v-col cols="12">
			<slot></slot>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class PSection extends Vue {}
</script>
