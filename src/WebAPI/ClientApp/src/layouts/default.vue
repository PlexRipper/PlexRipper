<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
				page change (flashing white background) during transitions.	-->
	<v-app :class="[isSetupPage ? 'no-background' : 'background']">
		<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />
		<alert-dialog v-for="(alertItem, i) in alerts" :key="i" :alert="alertItem" @close="closeAlert" />
		<!--	Use for setup-layout	-->
		<template v-if="isSetupPage">
			<vue-scroll>
				<v-main class="no-background">
					<nuxt />
				</v-main>
			</vue-scroll>
		</template>
		<!--	Use for everything else	-->
		<template v-else>
			<navigation-drawer :show-drawer="drawerState" />
			<app-bar @show="drawerState = $event" />
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
import { HelpService, AlertService } from '@service';
import IAlert from '@interfaces/IAlert';

@Component({
	loading: false,
})
export default class Default extends Vue {
	helpDialogState: boolean = false;
	helpId: string = '';
	alerts: IAlert[] = [];
	drawerState: Boolean = true;
	get isSetupPage(): boolean {
		return this.$route.fullPath === '/setup';
	}

	closeAlert(alert: IAlert): void {
		AlertService.removeAlert(alert.id);
	}

	mounted(): void {
		this.$subscribeTo(HelpService.getHelpDialog(), (helpId) => {
			if (helpId) {
				this.helpId = helpId;
				this.helpDialogState = true;
			}
		});

		this.$subscribeTo(AlertService.getAlerts(), (alerts) => {
			if (alerts) {
				this.alerts = alerts;
			}
		});
	}
}
</script>
