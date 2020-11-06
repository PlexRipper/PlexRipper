<template>
	<v-app>
		<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />
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
import AppBar from '@components/AppBar/AppBar.vue';
import HealthService from '@service/healthService';
import HelpService from '@service/helpService';
import HelpDialog from '@components/Help/HelpDialog.vue';

@Component({
	components: {
		NavigationDrawer,
		AppBar,
		HelpDialog,
	},
})
export default class Default extends Vue {
	status: boolean = false;
	helpDialogState: boolean = false;
	helpId: string = '';
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
		HelpService.getHelpDialog().subscribe((helpId) => {
			this.helpId = helpId;
			this.helpDialogState = true;
		});
	}
}
</script>

<style lang="scss">
@import '@/assets/scss/style.scss';
</style>
