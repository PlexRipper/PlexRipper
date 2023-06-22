<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
        page change (flashing white background) during transitions.	-->
	<q-layout view="hHh LpR lFf">
		<template v-if="!isLoading">
			<!--	Use for everything else	-->
			<template v-if="!isEmptyLayout">
				<app-bar @show-navigation="toggleNavigationsDrawer" @show-notifications="toggleNotificationsDrawer" />
				<NavigationDrawer :show-drawer="showNavigationDrawerState" />
				<NotificationsDrawer :show-drawer="showNotificationsDrawerState" @cleared="toggleNotificationsDrawer" />
			</template>
			<!--	page-load-completed is only visible once the page is done loading. This is used for Cypress E2E	-->
			<q-page-container data-cy="page-load-completed">
				<slot />
			</q-page-container>
		</template>
		<!--	Dialogs	-->
		<help-dialog :name="helpDialogName" />
		<alert-dialog v-for="(alertItem, i) in alerts" :key="i" :name="`alert-dialog-${alertItem.id}`" :alert="alertItem" />
		<CheckServerConnectionsDialog />
		<!--	Background	-->
		<Background :hide-background="isNoBackground" />
	</q-layout>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import { storeToRefs } from 'pinia';
import { AlertService, HelpService } from '@service';
import IAlert from '@interfaces/IAlert';
import PageLoadOverlay from '@components/General/PageLoadOverlay.vue';
import globalService from '@service/globalService';
import { useOpenControlDialog } from '#imports';
import { useSettingsStore } from '~/store';

const $q = useQuasar();
const route = useRoute();
const settingsStore = useSettingsStore();
const { languageSettings } = storeToRefs(settingsStore);
const { setLocale } = useI18n();
const isLoading = ref(true);

const alerts = ref<IAlert[]>([]);
const showNavigationDrawerState = ref(true);
const showNotificationsDrawerState = ref(false);
const helpDialogName = 'helpDialog';

const isEmptyLayout = computed((): boolean => {
	return route.fullPath.includes('setup');
});

const isNoBackground = computed((): boolean => {
	if (isLoading.value) {
		return true;
	}
	return isEmptyLayout.value;
});

function toggleNavigationsDrawer() {
	set(showNavigationDrawerState, !get(showNavigationDrawerState));
}

function toggleNotificationsDrawer() {
	set(showNotificationsDrawerState, !get(showNotificationsDrawerState));
}

watch(
	() => languageSettings.value.language,
	(value) => setLocale(value),
);

onMounted(() => {
	$q.loading.show({
		spinner: PageLoadOverlay,
	});
	useSubscription(
		globalService.getPageSetupReady().subscribe({
			next: () => {
				Log.debug('Loading has finished, displaying page now');
				set(isLoading, false);
				$q.loading.hide();
			},
			error: (err) => {
				Log.error('Error while loading page', err);
				$q.loading.hide();
			},
		}),
	);

	useSubscription(
		HelpService.getHelpDialog().subscribe((newHelpId) => {
			if (newHelpId) {
				useOpenControlDialog(helpDialogName, newHelpId);
			}
		}),
	);

	useSubscription(
		AlertService.getAlerts().subscribe((newAlerts) => {
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
