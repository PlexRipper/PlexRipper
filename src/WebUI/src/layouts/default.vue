<template>
	<v-app>
		<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />
		<navigation-drawer />
		<app-bar />

		<v-main>
			<nuxt />
		</v-main>
		<footer />
	</v-app>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import NavigationDrawer from '@/components/NavigationDrawer.vue';
import AppBar from '@components/AppBar/AppBar.vue';
import HelpService from '@service/helpService';
import HelpDialog from '@components/Help/HelpDialog.vue';
import Footer from '@components/Footer/Footer.vue';

@Component({
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
