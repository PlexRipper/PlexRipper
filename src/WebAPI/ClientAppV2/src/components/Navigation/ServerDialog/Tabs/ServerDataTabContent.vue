<template>
	<!--	Server Data Tab Content	-->
	<table class="section-table">
		<tbody>
		<!-- Machine Identifier -->
		<tr>
			<td style="width: 25%">{{ $t('components.server-dialog.tabs.server-data.machine-id') }}:</td>
			<td>{{ plexServer.machineIdentifier }}</td>
		</tr>
		<!-- Device -->
		<tr>
			<td>{{ $t('components.server-dialog.tabs.server-data.device') }}:</td>
			<td>{{ plexServer.device }}</td>
		</tr>
		<!-- Platform and platform version -->
		<tr>
			<td>{{ $t('components.server-dialog.tabs.server-data.platform') }}:</td>
			<td>{{ plexServer.platform }} ({{ plexServer.platformVersion }})</td>
		</tr>
		<!-- Product and version -->
		<tr>
			<td>{{ $t('components.server-dialog.tabs.server-data.plex-version') }}:</td>
			<td>{{ plexServer.product }} ({{ plexServer.productVersion }})</td>
		</tr>
		<!-- Created On -->
		<tr>
			<td>{{ $t('components.server-dialog.tabs.server-data.created-on') }}:</td>
			<td>
				<q-date-time short-date :text="plexServer.createdAt"/>
			</td>
		</tr>
		<tr>
			<td>{{ $t('components.server-dialog.tabs.server-data.last-updated-on') }}:</td>
			<td>
				<q-date-time short-date :text="plexServer.lastSeenAt"/>
			</td>
		</tr>
		<tr>
			<td>{{ $t('components.server-dialog.tabs.server-data.current-status') }}:</td>
			<td>
				<q-status pulse :value="hasSuccessServerStatus"/>
			</td>
		</tr>
		<!-- Server Status -->
		<tr v-if="serverConnections.length">
			<td>{{ $t('components.server-dialog.tabs.server-data.last-checked-on') }}:</td>
			<td>
				<q-date-time short-date :text="getLastStatusCheck"/>
			</td>
		</tr>
		</tbody>
	</table>
	<!--	Check Server Action	-->
	<q-card-actions align="right">
		<BaseButton text-id="check-server-status" :loading="checkServerStatusLoading" @click="checkServer"/>
	</q-card-actions>
</template>

<script setup lang="ts">
import {useSubscription} from '@vueuse/rxjs';
import type {PlexServerDTO} from '@dto/mainApi';
import {PlexServerConnectionDTO} from '@dto/mainApi';
import {ServerConnectionService, ServerService} from '@service';
import Log from "consola";

const checkServerStatusLoading = ref(false);
const hasSuccessServerStatus = ref(false);
const serverConnections = ref<PlexServerConnectionDTO[]>([]);

const props = withDefaults(defineProps<{
	plexServer: PlexServerDTO;
	isVisible: boolean;
}>(), {
	isVisible: false,

});


const getLastStatusCheck = computed(() => {
	const y = serverConnections.value
		.map((x) => x.latestConnectionStatus)
		.sort((a, b) => Date.parse(b.lastChecked) - Date.parse(a.lastChecked));
	return y[0].lastChecked;
});

const checkServer = () => {
	checkServerStatusLoading.value = true;
	useSubscription(
		ServerService.checkServer(props.plexServer.id).subscribe(() => {
			checkServerStatusLoading.value = false;
		}),
	);
}


const setup = () => {
	useSubscription(
		ServerConnectionService.getServerConnectionsByServerId(props.plexServer.id).subscribe((connections) => {
			serverConnections.value = connections;
		}),
	);
	useSubscription(
		ServerService.getServerStatus(props.plexServer.id).subscribe((value) => {
			hasSuccessServerStatus.value = value;
		}),
	);
}

onMounted(() => {
	Log.info('ServerDataTabContent', 'onMounted');
	setup();
})

onUnmounted(() => {
	Log.info('ServerDataTabContent', 'onUnmounted');
	checkServerStatusLoading.value = false;
})


</script>
