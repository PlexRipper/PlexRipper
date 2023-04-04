<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
        page change (flashing white background) during transitions.	-->
	<q-layout view="hHh lpR fFf">
		<template v-if="!isLoading">
			<help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false" />
			<!--            <alert-dialog v-for="(alertItem, i) in alerts" :key="i" :alert="alertItem" @close="closeAlert"/>-->
			<!--            <CheckServerConnectionsProgress/>-->

			<!--	Use for everything else	-->
			<template v-if="!isEmptyLayout">
				<app-bar @show-navigation="toggleNavigationsDrawer" @show-notifications="toggleNotificationsDrawer" />
				<NavigationDrawer :show-drawer="showNavigationDrawerState" />
				<NotificationsDrawer :show-drawer="showNotificationsDrawerState" @cleared="toggleNotificationsDrawer" />
			</template>
			<q-page-container class="page-container">
				<slot />
			</q-page-container>
		</template>
		<Background :hide-background="isNoBackground" />
	</q-layout>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, computed, onMounted } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import { AlertService, HelpService } from '@service';
import IAlert from '@interfaces/IAlert';
import PageLoadOverlay from '@components/General/PageLoadOverlay.vue';
import globalService from '@service/globalService';

const $q = useQuasar();
const route = useRoute();

const isLoading = ref(true);
const helpDialogState = ref(false);
const helpId = ref('');
const alerts = ref<IAlert[]>([]);
const showNavigationDrawerState = ref(true);
const showNotificationsDrawerState = ref(false);

const isEmptyLayout = computed((): boolean => {
	return route.fullPath.includes('setup');
});

const isNoBackground = computed((): boolean => {
	if (isLoading.value) {
		return true;
	}
	return isEmptyLayout.value;
});

function closeAlert(alert: IAlert): void {
	AlertService.removeAlert(alert.id);
}

function toggleNavigationsDrawer() {
	showNavigationDrawerState.value = !showNavigationDrawerState.value;
}

function toggleNotificationsDrawer() {
	Log.info('toggleNotificationsDrawer');
	showNotificationsDrawerState.value = !showNotificationsDrawerState.value;
}

onMounted(() => {
	$q.loading.show({
		spinner: PageLoadOverlay,
	});

	useSubscription(
		globalService.getPageSetupReady().subscribe(() => {
			Log.debug('Loading has finished, displaying page now');
			isLoading.value = false;
			$q.loading.hide();
		}),
	);

	useSubscription(
		HelpService.getHelpDialog().subscribe((newHelpId) => {
			if (newHelpId) {
				helpId.value = newHelpId;
				helpDialogState.value = true;
			}
		}),
	);

	useSubscription(
		AlertService.getAlerts().subscribe((newAlerts) => {
			if (newAlerts) {
				alerts.value = newAlerts;
			}
		}),
	);
});
</script>
