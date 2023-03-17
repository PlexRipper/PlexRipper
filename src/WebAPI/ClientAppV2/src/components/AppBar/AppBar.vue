<template>
    <q-header elevated>
        <q-toolbar class="glossy">
            <q-toolbar-title>
                <q-btn flat round dense icon="menu" class="q-mr-sm" @click.stop="showNavigationDrawer"/>
                <q-btn to="/" outlined nuxt>
                    <logo :size="24" class="mr-3"/>
                    {{ $t('general.name-version', {version}) }}
                </q-btn>
            </q-toolbar-title>
        </q-toolbar>
        <q-toolbar class="glossy">
            <AppBarProgressBar/>
        </q-toolbar>
        <q-toolbar class="glossy">
            <q-btn icon="mdi-github" href="https://github.com/PlexRipper/PlexRipper" target="_blank"/>

            <!-- DarkMode toggle -->
            <DarkModeToggle/>

            <!-- Account Selector -->
            <AccountSelector/>


            <!-- Notifications Selector -->
            <NotificationButton @toggle="showNotificationsDrawer"/>
        </q-toolbar>
    </q-header>
</template>

<script setup lang="ts">
import {useSubscription} from '@vueuse/rxjs';
import {GlobalService} from '@service';

const version = ref('?');

const emit = defineEmits<{
    (e: 'show-navigation'): void,
    (e: 'show-notifications'): void,
}>()


function showNavigationDrawer(): void {
    emit('show-navigation')
}

function showNotificationsDrawer(): void {
    emit('show-notifications')
}


onMounted(() => {
    useSubscription(
        GlobalService.getConfigReady().subscribe((config) => {
            version.value = config.version;
        }),
    );
});


</script>
