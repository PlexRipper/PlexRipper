<template>
	<v-row no-gutters style="width: 100%">
		<v-col>
			<slot></slot>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class PSection extends Vue {}
</script>
