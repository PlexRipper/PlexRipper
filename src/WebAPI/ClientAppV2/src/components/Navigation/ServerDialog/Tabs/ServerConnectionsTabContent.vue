<template>
	<div>
		<h4>{{ t('components.server-dialog.tabs.server-connections.section-header') }}</h4>
	</div>
	<q-list>
		<template v-for="(connection, index) in serverConnections" :key="index">
			<q-item>
				<!-- Radio Button -->
				<q-item-section avatar tag="label">
					<q-radio
						:model-value="preferredConnectionId"
						:val="connection.id"
						color="red"
						@update:model-value="setPreferredPlexServerConnection" />
				</q-item-section>
				<!-- Connection Icon -->
				<q-item-section avatar tag="label">
					<QConnectionIcon :local="connection.local" />
				</q-item-section>
				<!-- Connection Url -->
				<q-item-section tag="label">
					<span class="ml-2">{{ connection.url }}</span>
				</q-item-section>
				<q-space />
				<!-- Connection Status -->
				<q-item-section side>
					<q-status :value="connection.latestConnectionStatus?.isSuccessful ?? false" />
				</q-item-section>
				<q-item-section side>
					<CheckConnectionButton
						:loading="isLoading(connection.id)"
						:cy="`check-connection-btn-${index}`"
						@click="checkPlexConnection(connection.id)" />
				</q-item-section>
			</q-item>
			<CheckServerStatusProgressDisplay
				v-if="isLoading(connection.id)"
				:key="`progress-${index}`"
				:plex-server="plexServer"
				:progress="getProgress(connection.id)" />
		</template>
	</q-list>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { PlexServerConnectionDTO, PlexServerDTO } from '@dto/mainApi';
import { ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { ServerConnectionService, ServerService, SignalrService } from '@service';

const { t } = useI18n();
const serverConnections = ref<PlexServerConnectionDTO[]>([]);
const loading = ref<number[]>([]);
const progress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);
const preferredConnectionId = ref<number>(0);

const props = defineProps<{
	plexServer: PlexServerDTO | null;
	isVisible: boolean;
}>();

function getProgress(plexServerConnectionId: number): ServerConnectionCheckStatusProgressDTO | null {
	return get(progress).find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null;
}

function isLoading(plexServerConnectionId: number): boolean {
	return get(loading).includes(plexServerConnectionId);
}

function checkPlexConnection(plexServerConnectionId: number) {
	get(loading).push(plexServerConnectionId);
	useSubscription(
		ServerConnectionService.checkServerConnection(plexServerConnectionId).subscribe(() => {
			set(
				loading,
				get(loading).filter((x) => x !== plexServerConnectionId),
			);
		}),
	);
}

const setPreferredPlexServerConnection = (value: number) => {
	set(preferredConnectionId, value);
	useSubscription(ServerService.setPreferredPlexServerConnection(props.plexServer?.id ?? -1, value).subscribe());
};

const setup = () => {
	useSubscription(
		ServerConnectionService.getServerConnectionsByServerId(props.plexServer?.id ?? -1).subscribe((connections) => {
			set(serverConnections, connections);
		}),
	);
	useSubscription(
		SignalrService.getServerConnectionProgressByPlexServerId(props.plexServer?.id ?? -1).subscribe((progressData) => {
			set(progress, progressData);
		}),
	);
};

onMounted(() => {
	Log.info('ServerConnectionsTabContent', 'onMounted');
	setup();
});

onUnmounted(() => {
	Log.info('ServerConnectionsTabContent', 'onUnmounted');
	set(progress, []);
});
</script>
