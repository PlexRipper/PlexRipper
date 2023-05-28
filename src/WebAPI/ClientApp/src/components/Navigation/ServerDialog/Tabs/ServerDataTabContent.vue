<template>
	<!--	Server Data Tab Content	-->
	<table class="section-table">
		<tbody v-if="plexServer">
			<!-- Machine Identifier -->
			<tr>
				<td style="width: 25%">{{ t('components.server-dialog.tabs.server-data.machine-id') }}:</td>
				<td>{{ plexServer.machineIdentifier }}</td>
			</tr>
			<!-- Device -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.device') }}:</td>
				<td>{{ plexServer.device }}</td>
			</tr>
			<!-- Platform and platform version -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.platform') }}:</td>
				<td>{{ plexServer.platform }} ({{ plexServer.platformVersion }})</td>
			</tr>
			<!-- Product and version -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.plex-version') }}:</td>
				<td>{{ plexServer.product }} ({{ plexServer.productVersion }})</td>
			</tr>
			<!-- Created On -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.created-on') }}:</td>
				<td>
					<q-date-time short-date :text="plexServer.createdAt" />
				</td>
			</tr>
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.last-seen-at') }}:</td>
				<td>
					<q-date-time short-date :text="plexServer.lastSeenAt" />
				</td>
			</tr>
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.current-status') }}:</td>
				<td>
					<q-status pulse :value="hasSuccessServerStatus" />
				</td>
			</tr>
		</tbody>
		<tbody>
			<tr>
				<td>{{ t('general.error.invalid-server') }}</td>
			</tr>
		</tbody>
	</table>
	<!--	Check Server Action	-->
	<q-card-actions align="right">
		<BaseButton text-id="check-server-status" :loading="checkServerStatusLoading" @click="checkServer" />
	</q-card-actions>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { set } from '@vueuse/core';
import type { PlexServerDTO } from '@dto/mainApi';
import { PlexServerConnectionDTO } from '@dto/mainApi';
import { ServerConnectionService, ServerService } from '@service';

const { t } = useI18n();
const checkServerStatusLoading = ref(false);
const hasSuccessServerStatus = ref(false);
const serverConnections = ref<PlexServerConnectionDTO[]>([]);

const props = withDefaults(
	defineProps<{
		plexServer: PlexServerDTO | null;
		isVisible: boolean;
	}>(),
	{
		isVisible: false,
	},
);

function checkServer() {
	set(checkServerStatusLoading, true);
	useSubscription(
		ServerService.checkServerStatus(props.plexServer?.id ?? -1).subscribe((value) => {
			set(hasSuccessServerStatus, value);
			set(checkServerStatusLoading, false);
		}),
	);
}

function setup() {
	useSubscription(
		ServerConnectionService.getServerConnectionsByServerId(props.plexServer?.id ?? -1).subscribe((connections) => {
			set(serverConnections, connections);
		}),
	);
	useSubscription(
		ServerService.getServerStatus(props.plexServer?.id ?? -1).subscribe((value) => {
			set(hasSuccessServerStatus, value);
		}),
	);
}

onMounted(() => {
	Log.info('ServerDataTabContent', 'onMounted');
	setup();
});

onUnmounted(() => {
	Log.info('ServerDataTabContent', 'onUnmounted');
	set(checkServerStatusLoading, false);
});
</script>
