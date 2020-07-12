<template>
	<v-app>
		<navigation-drawer />
		<app-bar />
		<v-main>
			<nuxt />
		</v-main>
		<v-footer app>
			<span>&copy; {{ currentYear }} </span>
		</v-footer>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import NavigationDrawer from '@/components/NavigationDrawer.vue';
import AppBar from '@/components/AppBar.vue';
import StartUp from '@/plugins/startup';

@Component({
	components: {
		NavigationDrawer,
		AppBar,
	},
})
export default class Default extends Vue {
	get currentYear(): number {
		return new Date().getFullYear();
	}

	async created(): Promise<void> {
		this.$vuetify.theme.dark = true;
		await StartUp();
	}
}
</script>

<style lang="scss">
@import '@/assets/scss/style.scss';
</style>
