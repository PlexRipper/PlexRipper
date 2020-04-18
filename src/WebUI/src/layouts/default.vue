<template>
	<v-app>
		<v-content class="vContentOverride">
			<v-container fluid>
				<nuxt />
			</v-container>
		</v-content>
		<v-footer app>
			<span>&copy; {{ currentYear }} </span>
		</v-footer>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Default extends Vue {
	get currentYear(): number {
		return new Date().getFullYear();
	}
}
</script>

<style lang="scss">
@import '@/assets/scss/style.scss';
</style>
