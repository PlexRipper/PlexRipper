<template>
    <!--	Instead of multiple layouts we merge into one default layout to prevent full
        page change (flashing white background) during transitions.	-->
    <v-app>
        <page-load-overlay v-if="isLoading" :value="isLoading"/>
        <template v-else>
            <help-dialog :id="helpId" :show="helpDialogState" @close="helpDialogState = false"/>
            <alert-dialog v-for="(alertItem, i) in alerts" :key="i" :alert="alertItem" @close="closeAlert"/>
            <CheckServerConnectionsProgress/>
            <!--	Use for setup-layout	-->
            <template v-if="isSetupPage">
                <vue-scroll>
                    <v-main class="no-background">
                        <nuxt/>
                    </v-main>
                </vue-scroll>
            </template>
            <!--	Use for everything else	-->
            <template v-else>
                <app-bar @show-navigation="toggleNavigationsDrawer" @show-notifications="toggleNotificationsDrawer"/>
                <navigation-drawer :show-drawer="showNavigationDrawerState"/>
                <notifications-drawer :show-drawer="showNotificationsDrawerState" @cleared="toggleNotificationsDrawer"/>
                <v-main>
                    <nuxt/>
                </v-main>
                <footer/>
            </template>
        </template>
        <Background :hide-background="isNoBackground"/>
    </v-app>
</template>

<script setup lang="ts">
import Log from 'consola';
import {useSubscription} from '@vueuse/rxjs';
import {AlertService, HelpService} from '@service';
import IAlert from '@interfaces/IAlert';
import NotificationsDrawer from '@overviews/NotificationsDrawer.vue';
import PageLoadOverlay from '@components/General/PageLoadOverlay.vue';
import globalService from '@service/globalService';
import CheckServerConnectionsProgress from '@components/Progress/CheckServerConnectionsProgress.vue';

const route = useRoute();

const isLoading = ref(true);
const helpDialogState = ref(false);
const helpId = ref('');
const alerts = ref<IAlert[]>([]);
const showNavigationDrawerState = ref(true);
const showNotificationsDrawerState = ref(false);


const isSetupPage = computed(() => {
    return route.fullPath.includes('setup');
});

const isNoBackground = computed(() => {
    if (isLoading) {
        return true;
    }
    return isSetupPage;
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
    useSubscription(
        globalService.getPageSetupReady().subscribe(() => {
            Log.debug('Loading has finished, displaying page now');
            isLoading.value = false;
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
