<template>
	<q-item>
		<!-- Radio Button -->
		<q-item-section
			avatar
			tag="label">
			<q-radio
				v-model="preferredConnectionId"
				:val="connection.id"
				color="red" />
		</q-item-section>
		<!-- Connection Icon -->
		<q-item-section
			avatar
			tag="label">
			<QConnectionIcon :type="connection.type" />
		</q-item-section>
		<!-- Connection Status -->
		<q-item-section side>
			<QStatus :value="connection.latestConnectionStatus?.isSuccessful ?? false" />
		</q-item-section>
		<!-- Connection Url -->
		<q-item-section tag="label">
			<span class="ml-2">{{ connection.url }}</span>
		</q-item-section>
		<q-space />
		<q-item-section
			v-if="connection.isCustom"
			side>
			<EditIconButton
				@click="dialogStore.openAddConnectionDialog({ plexServerId, plexServerConnectionId: connection.id })" />
		</q-item-section>

		<q-item-section side>
			<CheckConnectionButton
				:loading="loading"
				:cy="`check-connection-btn-${connection.id}`"
				@click="checkPlexConnection(connection.id)" />
		</q-item-section>
	</q-item>
	<!-- Progress Text -->
	<CheckServerStatusProgressDisplay
		v-if="progressData"
		:progress="progressData" />
</template>

<script setup lang="ts">
import type { PlexServerConnectionDTO, ServerConnectionCheckStatusProgressDTO } from '@dto';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { useDialogStore, useServerConnectionStore, useServerStore, useSignalrStore } from '@store';
import { map } from 'rxjs/operators';

const serverStore = useServerStore();
const signalrStore = useSignalrStore();
const dialogStore = useDialogStore();
const serverConnectionStore = useServerConnectionStore();

const progressData = ref<ServerConnectionCheckStatusProgressDTO | null>(null);

const props = defineProps<{
	connection: PlexServerConnectionDTO;
}>();

const plexServerId = computed<number>(() => props.connection.plexServerId);

const loading = computed(() => serverConnectionStore.getConnectionLoading(props.connection.id));
const preferredConnectionId = computed<number>({
	get: () => serverStore.getServer(get(plexServerId))?.preferredConnectionId ?? -1,
	set: (value) => {
		useSubscription(serverConnectionStore.setPreferredPlexServerConnection(get(plexServerId), value).subscribe());
	},
});

function checkPlexConnection(plexServerConnectionId: number) {
	useSubscription(
		serverConnectionStore.checkServerConnection(plexServerConnectionId).subscribe(),
	);
}
onMounted(() => useSubscription(
	signalrStore
		.getServerConnectionProgressByPlexServerId(get(plexServerId))
		.pipe(map((x) => x.find((y) => y.plexServerConnectionId === props.connection.id)))
		.subscribe((data) => {
			set(progressData, data ?? null);
		}),
));
</script>
