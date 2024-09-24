<template>
	<QText size="h5">
		{{ $t('components.server-dialog.tabs.server-connections.section-header') }}
	</QText>
	<q-list>
		<template
			v-for="(connection, index) in serverConnectionStore.getServerConnectionsByServerId(plexServer?.id)"
			:key="index">
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
					<QConnectionIcon :local="connection.local" />
				</q-item-section>
				<!-- Connection Url -->
				<q-item-section tag="label">
					<span class="ml-2">{{ connection.url }}</span>
				</q-item-section>
				<q-space />
				<!-- Connection Status -->
				<q-item-section side>
					<QStatus :value="connection.latestConnectionStatus?.isSuccessful ?? false" />
				</q-item-section>
				<q-item-section side>
					<CheckConnectionButton
						:loading="isLoading(connection.id)"
						:cy="`check-connection-btn-${index}`"
						@click="checkPlexConnection(connection.id)" />
				</q-item-section>
			</q-item>
			<CheckServerStatusProgressDisplay
				v-if="getProgress(connection.id)"
				:key="`progress-${index}`"
				:plex-server="plexServer"
				:progress="getProgress(connection.id)" />
		</template>
	</q-list>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { PlexServerDTO, ServerConnectionCheckStatusProgressDTO } from '@dto';
import { useServerConnectionStore, useSignalrStore } from '~/store';

const serverStore = useServerStore();
const signalrStore = useSignalrStore();
const serverConnectionStore = useServerConnectionStore();

const loading = ref<number[]>([]);
const progressList = ref<ServerConnectionCheckStatusProgressDTO[]>([]);

const props = defineProps<{
	plexServerId: number;
	isVisible: boolean;
}>();

const plexServer = computed<PlexServerDTO | null>(() => serverStore.getServer(props.plexServerId));

const preferredConnectionId = computed<number>({
	get: () => get(plexServer)?.preferredConnectionId ?? -1,
	set: (value) => {
		useSubscription(serverConnectionStore.setPreferredPlexServerConnection(props.plexServerId, value).subscribe());
	},
});

function getProgress(plexServerConnectionId: number): ServerConnectionCheckStatusProgressDTO | null {
	return get(progressList).find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null;
}

function isLoading(plexServerConnectionId: number): boolean {
	return get(loading).includes(plexServerConnectionId);
}

function checkPlexConnection(plexServerConnectionId: number) {
	get(loading).push(plexServerConnectionId);

	const index = get(progressList).findIndex((x) => x.plexServerConnectionId === plexServerConnectionId);
	if (index) {
		get(progressList).splice(index, 1);
	}

	useSubscription(
		serverConnectionStore.checkServerConnection(plexServerConnectionId).subscribe(() => {
			set(
				loading,
				get(loading).filter((x) => x !== plexServerConnectionId),
			);
		}),
	);
}

function setup() {
	useSubscription(
		signalrStore
			.getServerConnectionProgressByPlexServerId(props.plexServerId)
			.subscribe((progressData) => set(progressList, progressData)),
	);
}

onMounted(() => {
	setup();
});

onUnmounted(() => {
	set(progressList, []);
});
</script>
