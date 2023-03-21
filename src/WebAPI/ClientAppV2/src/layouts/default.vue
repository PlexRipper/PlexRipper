<template>
	<!--	Instead of multiple layouts we merge into one default layout to prevent full
        page change (flashing white background) during transitions.	-->
	<q-layout view="hHh lpR fFf">
		<template v-if="!isLoading">
			<!--            <help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false"/>-->
			<!--            <alert-dialog v-for="(alertItem, i) in alerts" :key="i" :alert="alertItem" @close="closeAlert"/>-->
			<!--            <CheckServerConnectionsProgress/>-->
			<!--	Use for setup-layout	-->
			<template v-if="isSetupPage">
				<!--                <vue-scroll>-->
				<q-page-container>
					<router-view/>
				</q-page-container>
				<!--                </vue-scroll>-->
			</template>
			<!--	Use for everything else	-->
			<template v-else>
				<app-bar @show-navigation="toggleNavigationsDrawer" @show-notifications="toggleNotificationsDrawer"/>
				<NavigationDrawer :show-drawer="showNavigationDrawerState"/>
				<!--                <notifications-drawer :show-drawer="showNotificationsDrawerState" @cleared="toggleNotificationsDrawer"/>-->
				<q-page-container class="page-container">
					<router-view/>
				</q-page-container>
				<footer/>
			</template>
		</template>
		<Background :hide-background="isNoBackground"/>
	</q-layout>
</template>

<script setup lang="ts">
import Log from 'consola';
import {useSubscription} from '@vueuse/rxjs';
import NotificationsDrawer from '@overviews/NotificationsDrawer.vue';
import {AlertService, HelpService} from '@service';
import IAlert from '@interfaces/IAlert';
import PageLoadOverlay from '@components/General/PageLoadOverlay.vue';
import globalService from '@service/globalService';

const $q = useQuasar();
const route = useRoute();

Log.info('Default layout loaded', $q);

const isLoading = ref(true);
const helpDialogState = ref(false);
const helpId = ref('');
const alerts = ref<IAlert[]>([]);
const showNavigationDrawerState = ref(true);
const showNotificationsDrawerState = ref(false);

$q.dark.set(true);

const isSetupPage = computed((): boolean => {
	return route.fullPath.includes('setup');
});

const isNoBackground = computed((): boolean => {
	if (isLoading.value) {
		return true;
	}
	return isSetupPage.value;
});

function closeAlert(alert: IAlert): void {
	AlertService.removeAlert(alert.id);
}

function toggleNavigationsDrawer() {
	showNavigationDrawerState.value = !showNavigationDrawerState.value;
}

function toggleNotificationsDrawer() {
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

// TODO: Temp solution for this issue: https://github.com/Maiquu/nuxt-quasar/pull/7.
// see styles => @import 'assets/scss/style.scss';
// and re-enable in nuxt.config.ts
</script>

<style lang="scss">
body {
	&.body--dark {
		.page-container {
			background: $dark-theme-glass-background;
		}
	}

	&.body--light {
		.page-container {
			background: $light-background-color;
		}
	}
}
</style>
