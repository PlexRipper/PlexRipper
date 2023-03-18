<template>
	<q-option-group
		v-model="preferredConnectionId"
		:options="serverConnections"
	>
		<!--		@change="setPreferredPlexServerConnection($event)"-->
		<template v-slot:label="connection">
			<q-row>
				<q-col>
					<q-radio color="red" :value="connection.id">

						<q-icon :name="getConnectionIcon(connection.local)">
							<q-tooltip anchor="top middle" self="center middle">
						<span>{{
								connection.local
									? $t('general.tooltip.local-connection')
									: $t('general.tooltip.public-connection')
							}}</span>
							</q-tooltip>
						</q-icon>
						<!-- Connection Url -->
						<span class="ml-2">{{ connection.url }}</span>
					</q-radio>
				</q-col>
				<!-- Connection Status -->
				<q-col>
					<q-item-label>
						<q-status :value="connection.latestConnectionStatus.isSuccessful ?? false"/>
					</q-item-label>
				</q-col>
				<q-col>
					<CheckConnectionButton
						:loading="isLoading(connection.id)"
						@click="checkPlexConnection(connection.id)"
					/>
				</q-col>
				<q-col v-if="getProgress(connection.id)">
					<td colspan="3">
						<!--						<CheckServerStatusProgressDisplay :plex-server="plexServer" :progress="getProgress(connection.id)"/>-->
					</td>
				</q-col>
			</q-row>
		</template>
	</q-option-group>
</template>

<script setup lang="ts">
import {useSubscription} from '@vueuse/rxjs';
import type {PlexServerConnectionDTO, PlexServerDTO} from '@dto/mainApi';
import {ServerConnectionCheckStatusProgressDTO} from '@dto/mainApi';
import {ServerConnectionService, ServerService, SignalrService} from '@service';
import Log from "consola";

const serverConnections = ref<PlexServerConnectionDTO[]>([])
const loading = ref<number[]>([])
const progress = ref<ServerConnectionCheckStatusProgressDTO[]>([])
const preferredConnectionId = ref<number>(0)

const props = defineProps<{
	plexServer: PlexServerDTO;
	isVisible: boolean;
}>();

const getConnectionIcon = (local: boolean): string => {
	return local ? 'mdi-lan-connect' : 'mdi-earth';
}

const getProgress = (plexServerConnectionId: number): ServerConnectionCheckStatusProgressDTO | null => {
	return progress.value.find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null;
}

const isLoading = (plexServerConnectionId: number): boolean => {
	return loading.value.includes(plexServerConnectionId);
}

const checkPlexConnection = (plexServerConnectionId: number) => {
	loading.value.push(plexServerConnectionId)
	useSubscription(
		ServerConnectionService.checkServerConnection(plexServerConnectionId).subscribe(() => {
			loading.value = loading.value.filter((x) => x !== plexServerConnectionId);
		}),
	);
}

const setPreferredPlexServerConnection = (value: number) => {
	preferredConnectionId.value = value;
	useSubscription(ServerService.setPreferredPlexServerConnection(props.plexServer.id, value).subscribe());
}

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
}

onMounted(() => {
	Log.info('ServerConnectionsTabContent', 'onMounted');
	setup();
})

onUnmounted(() => {
	Log.info('ServerConnectionsTabContent', 'onUnmounted');
	progress.value = [];
})

</script>
