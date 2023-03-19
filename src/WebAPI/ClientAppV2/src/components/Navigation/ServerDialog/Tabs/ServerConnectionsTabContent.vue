<template>
	<div>
		<h4>{{ $t('components.server-dialog.tabs.server-connections.section-header') }}</h4>
	</div>
	<q-list>
		<template v-for="(connection, index) in serverConnections">
			<q-item v-ripple>
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
					<q-icon :name="getConnectionIcon(connection.local)" style="font-size: 2em">
						<q-tooltip anchor="top middle" self="center middle">
							<span>{{
								connection.local
									? $t('general.tooltip.local-connection')
									: $t('general.tooltip.public-connection')
							}}</span>
						</q-tooltip>
					</q-icon>
				</q-item-section>
				<!-- Connection Url -->
				<q-item-section tag="label">
					<span class="ml-2">{{ connection.url }}</span>
				</q-item-section>
				<q-space />
				<!-- Connection Status -->
				<q-item-section side>
					<q-status :value="connection.latestConnectionStatus.isSuccessful ?? false" />
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
import { defineProps, onMounted, ref } from 'vue';
import { useSubscription } from '@vueuse/rxjs';
import type { PlexServerConnectionDTO, PlexServerDTO } from '@dto/mainApi';
import { ServerConnectionCheckStatusProgressDTO } from '@dto/mainApi';
import { ServerConnectionService, ServerService, SignalrService } from '@service';

const serverConnections = ref<PlexServerConnectionDTO[]>([]);
const loading = ref<number[]>([]);
const progress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);
const preferredConnectionId = ref<number>(0);

const props = defineProps<{
	plexServer: PlexServerDTO;
	isVisible: boolean;
}>();

const getConnectionIcon = (local: boolean): string => {
	return local ? 'mdi-lan-connect' : 'mdi-earth';
};

function getProgress(plexServerConnectionId: number): ServerConnectionCheckStatusProgressDTO | null {
	return progress.value.find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null;
}

function isLoading(plexServerConnectionId: number): boolean {
	return loading.value.includes(plexServerConnectionId);
}

function checkPlexConnection(plexServerConnectionId: number) {
	loading.value.push(plexServerConnectionId);
	useSubscription(
		ServerConnectionService.checkServerConnection(plexServerConnectionId).subscribe(() => {
			loading.value = loading.value.filter((x) => x !== plexServerConnectionId);
		}),
	);
}

const setPreferredPlexServerConnection = (value: number) => {
	preferredConnectionId.value = value;
	useSubscription(ServerService.setPreferredPlexServerConnection(props.plexServer.id, value).subscribe());
};

const setup = () => {
	useSubscription(
		ServerConnectionService.getServerConnectionsByServerId(props.plexServer.id).subscribe((connections) => {
			serverConnections.value = connections;
		}),
	);
	useSubscription(
		SignalrService.getServerConnectionProgressByPlexServerId(props.plexServer.id).subscribe((progressData) => {
			progress.value = progressData;
		}),
	);
};

onMounted(() => {
	Log.info('ServerConnectionsTabContent', 'onMounted');
	setup();
});

onUnmounted(() => {
	Log.info('ServerConnectionsTabContent', 'onUnmounted');
	progress.value = [];
});
</script>
