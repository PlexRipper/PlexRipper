<template>
	<v-row>
		<v-col>
			<h1>WORKING!</h1>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import Vue from 'vue';
import Component from 'vue-class-component';

@Component({
	meta: {
		allowNoRequiredScopes: true,
	},
})
export default class Home extends Vue {}
</script>
