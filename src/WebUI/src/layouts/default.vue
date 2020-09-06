<template>
	<v-app>
		<navigation-drawer />
		<app-bar />

		<v-main>
			<nuxt />
		</v-main>
		<v-footer app>
			<span>&copy; {{ currentYear }} - {{ getServerStatus }}</span>
		</v-footer>
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import NavigationDrawer from '@/components/NavigationDrawer.vue';
import AppBar from '@/components/AppBar.vue';
import HealthService from '@service/healthService';

@Component({
	components: {
		NavigationDrawer,
		AppBar,
	},
})
export default class Default extends Vue {
	status: boolean = false;

	get currentYear(): number {
		return new Date().getFullYear();
	}

	get getServerStatus(): string {
		return this.status ? 'Server is online!' : 'Server is offline';
	}

	created(): void {
		this.$vuetify.theme.dark = true;
		HealthService.getServerStatus().subscribe((status) => {
			this.status = status;
		});
	}
}
</script>

<style lang="scss">
@import '@/assets/scss/style.scss';
</style>
