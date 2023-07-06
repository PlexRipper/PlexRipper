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
import { tap } from 'rxjs/operators';
import PageLoadOverlay from '@components/General/PageLoadOverlay.vue';
import globalService from '@service/globalService';
import { useSettingsStore, useHelpStore, useAlertStore } from '#imports';
import IAlert from '@interfaces/IAlert';

const $q = useQuasar();
const route = useRoute();
const settingsStore = useSettingsStore();
const helpStore = useHelpStore();
const alertStore = useAlertStore();
const { languageSettings } = storeToRefs(settingsStore);
const i18n = useI18n();
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

/**
 * This is done because I18n doesn't work inside a Pinia store
 * Source: https://stackoverflow.com/a/76477511/8205497
 */
watch(
	() => languageSettings.value.language,
	(value) => {
		i18n.setLocale(value);
		settingsStore.setI18nObject(i18n);
	},
);

onMounted(() => {
	$q.loading.show({
		spinner: PageLoadOverlay,
	});
	useSubscription(
		globalService
			.getPageSetupReady()
			.pipe(
				tap(() => {
					settingsStore.setI18nObject(i18n);
				}),
			)
			.subscribe({
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
