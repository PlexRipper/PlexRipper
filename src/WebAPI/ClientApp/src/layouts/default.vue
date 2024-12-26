<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
        page change (flashing white background) during transitions.	-->
	<q-layout view="hHh LpR lFf">
		<!--	Use for everything else	-->
		<template v-if="!isEmptyLayout">
			<AppBar
				@show-navigation="toggleNavigationsDrawer"
				@show-notifications="toggleNotificationsDrawer" />
			<NavigationDrawer :show-drawer="showNavigationDrawerState" />
			<NotificationsDrawer
				:show-drawer="showNotificationsDrawerState"
				@cleared="toggleNotificationsDrawer" />
		</template>
		<!--	page-load-completed is only visible once the page is done loading. This is used for Cypress E2E	-->
		<q-page-container data-cy="page-load-completed">
			<slot />
		</q-page-container>
		<!--	Dialogs	-->
		<HelpDialog />
		<AlertDialog
			v-for="alertItem in alerts"
			:key="alertItem.id"
			:alert="alertItem" />
		<CheckServerConnectionsDialog />
		<FirstTimeSetupDialog />
		<DiscordInviteDialog />
		<SyncServerMediaDialog />
		<!--	Background	-->
		<Background :hide-background="isEmptyLayout" />
	</q-layout>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { IAlert } from '@interfaces';
import { DialogType } from '@enums';
import {
	useHelpStore,
	useAlertStore,
	useGlobalStore,
	useDialogStore,
	useSettingsStore,
	useRoute,
	nextTick,
} from '#imports';

const route = useRoute();
const helpStore = useHelpStore();
const alertStore = useAlertStore();
const dialogStore = useDialogStore();
const settingsStore = useSettingsStore();

const alerts = ref<IAlert[]>([]);
const showNavigationDrawerState = ref(true);
const showNotificationsDrawerState = ref(false);

const isEmptyLayout = computed((): boolean => {
	return route.fullPath.includes('setup') || route.fullPath.includes('login');
});

function toggleNavigationsDrawer() {
	set(showNavigationDrawerState, !get(showNavigationDrawerState));
}

function toggleNotificationsDrawer() {
	set(showNotificationsDrawerState, !get(showNotificationsDrawerState));
}

onMounted(() => {
	useSubscription(
		useGlobalStore().getPageSetupReady.subscribe({
			next: (ready) => {
				if (ready) {
					Log.debug('Loading has finished, displaying page now');
					setTimeout(() => {
						if (settingsStore.generalSettings.firstTimeSetup) {
							dialogStore.openDialog(DialogType.FirstTimeSetupDialog);
						} else if (!settingsStore.generalSettings.hasBeenInvitedToDiscord) {
							dialogStore.openDialog(DialogType.DiscordServerInviteDialog);
						}
					}, 1000);
				} else {
					// TODO: Display loading
				}
			},
			error: (err) => {
				Log.error('Error while loading page', err);
			},
		}),
	);

	useSubscription(
		helpStore.getHelpDialog.subscribe((newHelpObject) => {
			if (newHelpObject) {
				dialogStore.openHelpInfoDialog(newHelpObject);
			}
		}),
	);

	useSubscription(
		alertStore.getAlerts.subscribe((newAlerts) => {
			if (newAlerts) {
				set(alerts, newAlerts);
				// Allow the alert dialog to render first before opening it
				nextTick(() => {
					for (const newAlert of get(alerts)) {
						dialogStore.openAlertInfoDialog(newAlert);
					}
				});
			}
		}),
	);

	window.addEventListener('resize', () => {
		if (document.body.classList.contains('window-resizing')) {
			return;
		}
		document.body.classList.add('window-resizing');

		setTimeout(() => {
			document.body.classList.remove('window-resizing');
		}, 100);
	});
});
</script>
