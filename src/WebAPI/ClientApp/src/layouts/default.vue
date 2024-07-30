<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
        page change (flashing white background) during transitions.	-->
	<q-layout view="hHh LpR lFf">
		<!--	Use for everything else	-->
		<template v-if="!isEmptyLayout">
			<AppBar
				@show-navigation="toggleNavigationsDrawer"
				@show-notifications="toggleNotificationsDrawer"
			/>
			<NavigationDrawer :show-drawer="showNavigationDrawerState" />
			<NotificationsDrawer
				:show-drawer="showNotificationsDrawerState"
				@cleared="toggleNotificationsDrawer"
			/>
		</template>
		<!--	page-load-completed is only visible once the page is done loading. This is used for Cypress E2E	-->
		<q-page-container data-cy="page-load-completed">
			<slot />
		</q-page-container>
		<!--	Dialogs	-->
		<HelpDialog :name="helpDialogName" />
		<AlertDialog
			v-for="(alertItem, i) in alerts"
			:key="i"
			:name="`alert-dialog-${alertItem.id}`"
			:alert="alertItem"
		/>
		<CheckServerConnectionsDialog />
		<!--	Background	-->
		<Background :hide-background="isEmptyLayout" />
	</q-layout>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type IAlert from '@interfaces/IAlert';
import { useHelpStore, useAlertStore, useGlobalStore, useRoute } from '#imports';

const route = useRoute();
const helpStore = useHelpStore();
const alertStore = useAlertStore();

const alerts = ref<IAlert[]>([]);
const showNavigationDrawerState = ref(true);
const showNotificationsDrawerState = ref(false);
const helpDialogName = 'helpDialog';

const isEmptyLayout = computed((): boolean => {
	return route.fullPath.includes('setup');
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
			next: () => {
				Log.debug('Loading has finished, displaying page now');
			},
			error: (err) => {
				Log.error('Error while loading page', err);
			},
		}),
	);

	useSubscription(
		helpStore.getHelpDialog.subscribe((newHelpId) => {
			if (newHelpId) {
				useOpenControlDialog(helpDialogName, newHelpId);
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
						useOpenControlDialog(`alert-dialog-${newAlert.id}`);
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
