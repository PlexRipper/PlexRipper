<template>
	<v-app>
		<v-content>
			<navigation-drawer />
			<nuxt />
		</v-content>
		<v-footer app>
			<span>&copy; {{ currentYear }} </span>
		</v-footer>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import NavigationDrawer from '@/components/NavigationDrawer.vue';
@Component({
	components: {
		NavigationDrawer,
	},
})
export default class Default extends Vue {
	get currentYear(): number {
		return new Date().getFullYear();
	}

	created(): void {
		this.$vuetify.theme.dark = true;
	}
}
</script>

<style lang="scss">
@import '@/assets/scss/style.scss';
</style>
