<template>
	<!--	Server Data Tab Content	-->
	<q-markup-table wrap-cells>
		<tbody v-if="plexServer">
			<!-- Machine Identifier -->
			<tr>
				<td style="width: 30%">{{ t('components.server-dialog.tabs.server-data.machine-id') }}:</td>
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
			<!--	Check Server Action	-->
			<tr>
				<td>
					<BaseButton text-id="check-server-status" :loading="checkServerStatusLoading" @click="checkServer" />
				</td>
				<td>
					{{ checkServerStatusMessage }}
				</td>
			</tr>
		</tbody>
		<tbody v-else>
			<tr>
				<td>{{ t('general.error.invalid-server') }}</td>
			</tr>
		</tbody>
	</q-markup-table>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { PlexServerConnectionDTO, ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { ServerConnectionService, ServerService, SignalrService } from '@service';

const { t } = useI18n();
const checkServerStatusLoading = ref(false);
const hasSuccessServerStatus = ref(false);
const serverConnections = ref<PlexServerConnectionDTO[]>([]);
const serverStatus = ref<PlexServerStatusDTO | null>(null);
const progress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);

const props = withDefaults(
	defineProps<{
		plexServer: PlexServerDTO | null;
		isVisible: boolean;
	}>(),
	{
		plexServer: null,
		isVisible: false,
	},
);

const plexServerId = computed(() => props.plexServer?.id ?? -1);

const checkServerStatusMessage = computed(() => {
	if (get(checkServerStatusLoading)) {
		return progress.value.map((x) => x.message).join('\n');
	}

	return get(serverStatus)?.statusMessage ?? '';
});

function checkServer() {
	set(checkServerStatusLoading, true);
	useSubscription(
		ServerService.checkServerStatus(get(plexServerId)).subscribe((value) => {
			set(hasSuccessServerStatus, value?.isSuccessful ?? false);
			set(checkServerStatusLoading, false);
			set(serverStatus, value ?? null);
		}),
	);
}

function setup() {
	useSubscription(
		ServerConnectionService.getServerConnectionsByServerId(get(plexServerId)).subscribe((connections) => {
			set(serverConnections, connections);
		}),
	);
	useSubscription(
		ServerService.getServerStatus(get(plexServerId)).subscribe((value) => {
			set(hasSuccessServerStatus, value);
		}),
	);

	useSubscription(
		SignalrService.getServerConnectionProgressByPlexServerId(get(plexServerId)).subscribe((progressData) => {
			set(progress, progressData);
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
