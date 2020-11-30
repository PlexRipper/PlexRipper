<template>
	<v-app :class="[hasBackgroundOverlay ? 'background' : 'no-background']">
		<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />
		<navigation-drawer v-if="showNavigationDrawer" />
		<app-bar v-if="showAppbar" />

		<v-main class="no-background">
			<nuxt />
		</v-main>
		<footer />
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
		return this.$route.fullPath !== '/setup';
	}

	get hasBackgroundOverlay(): boolean {
		return this.isSetupPage;
	}

	get showNavigationDrawer(): boolean {
		return this.isSetupPage;
	}

	get showAppbar(): boolean {
		return this.isSetupPage;
	}

	created(): void {
		HelpService.getHelpDialog().subscribe((helpId) => {
			this.helpId = helpId;
			this.helpDialogState = true;
		});
	}
}
</script>

<style lang="scss">
// This only needs to be included in the default layout, not in other layouts.
// Otherwise the styles get imported multiple times.
@import './src/assets/scss/style.scss';
</style>
