<template>
	<div>
		<h4>{{ t('components.server-dialog.tabs.server-connections.section-header') }}</h4>
	</div>
	<q-list>
		<template
			v-for="(connection, index) in serverConnectionStore.getServerConnectionsByServerId(plexServer?.id)"
			:key="index"
		>
			<q-item>
				<!-- Radio Button -->
				<q-item-section
					avatar
					tag="label"
				>
					<q-radio
						v-model="preferredConnectionId"
						:val="connection.id"
						color="red"
					/>
				</q-item-section>
				<!-- Connection Icon -->
				<q-item-section
					avatar
					tag="label"
				>
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
						@click="checkPlexConnection(connection.id)"
					/>
				</q-item-section>
			</q-item>
			<CheckServerStatusProgressDisplay
				v-if="isLoading(connection.id)"
				:key="`progress-${index}`"
				:plex-server="plexServer"
				:progress="getProgress(connection.id)"
			/>
		</template>
	</q-list>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { PlexServerDTO, ServerConnectionCheckStatusProgressDTO } from '@dto';
import { useServerConnectionStore, useSignalrStore } from '~/store';

const { t } = useI18n();

const serverStore = useServerStore();
const signalrStore = useSignalrStore();
const serverConnectionStore = useServerConnectionStore();

const loading = ref<number[]>([]);
const progress = ref<ServerConnectionCheckStatusProgressDTO[]>([]);

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
	return get(progress).find((x) => x.plexServerConnectionId === plexServerConnectionId) ?? null;
}

function isLoading(plexServerConnectionId: number): boolean {
	return get(loading).includes(plexServerConnectionId);
}

function checkPlexConnection(plexServerConnectionId: number) {
	get(loading).push(plexServerConnectionId);
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
			.subscribe((progressData) => set(progress, progressData)),
	);
}

onMounted(() => {
	setup();
});

onUnmounted(() => {
	set(progress, []);
});
</script>
