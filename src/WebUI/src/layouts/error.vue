<template>
	<v-app dark>
		<h1>{{ error }}</h1>
		<NuxtLink to="/"> Home page </NuxtLink>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class ErrorLayout extends Vue {
	@Prop()
	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	error!: any;

	layout(): string {
		return 'error';
	}
}
</script>
