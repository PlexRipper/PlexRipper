<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
				page change (flashing white background) during transitions.	-->
	<v-app :class="[isSetupPage ? 'no-background' : 'background']">
		<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />
		<!--	Use for setup-layout	-->
		<template v-if="isSetupPage">
			<perfect-scrollbar style="height: 100vh">
				<v-main class="no-background">
					<nuxt />
				</v-main>
			</perfect-scrollbar>
		</template>
		<!--	Use for everything else	-->
		<template v-else>
			<navigation-drawer />
			<app-bar />
			<v-main class="no-background">
				<nuxt />
			</v-main>
			<footer />
		</template>
		<background />
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import NavigationDrawer from '@components/Navigation/NavigationDrawer.vue';
import AppBar from '@components/AppBar/AppBar.vue';
import HelpService from '@service/helpService';
import HelpDialog from '@components/Help/HelpDialog.vue';
import Footer from '@components/Footer/Footer.vue';

@Component({
	loading: false,
	components: {
		NavigationDrawer,
		AppBar,
		HelpDialog,
		Footer,
	},
})
export default class Default extends Vue {
	helpDialogState: boolean = false;
	helpId: string = '';

	get isSetupPage(): boolean {
		return this.$route.fullPath === '/setup';
	}

	created(): void {
		this.$subscribeTo(HelpService.getHelpDialog(), (helpId) => {
			this.helpId = helpId;
			this.helpDialogState = true;
		});
	}
}
</script>
