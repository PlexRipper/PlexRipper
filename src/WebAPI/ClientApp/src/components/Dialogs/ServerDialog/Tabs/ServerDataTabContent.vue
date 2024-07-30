<template>
	<!--	Server Data Tab Content	-->
	<q-markup-table wrap-cells>
		<tbody v-if="plexServer">
			<!-- Machine Identifier -->
			<tr>
				<td style="width: 30%">
					{{ t('components.server-dialog.tabs.server-data.headers.machine-id') }}
				</td>
				<td>{{ plexServer.machineIdentifier }}</td>
			</tr>
			<!-- Device -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.headers.device') }}</td>
				<td>{{ plexServer.device }}</td>
			</tr>
			<!-- Platform and platform version -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.headers.platform') }}</td>
				<td>
					{{
						$t('components.server-dialog.tabs.server-data.values.platform-version', {
							platform: plexServer.platform,
							platformVersion: plexServer.platformVersion,
						})
					}}
				</td>
			</tr>
			<!-- Product and version -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.headers.plex-version') }}</td>
				<td>
					{{
						$t('components.server-dialog.tabs.server-data.values.product-version', {
							product: plexServer.product,
							productVersion: plexServer.productVersion,
						})
					}}
				</td>
			</tr>
			<!-- Created On -->
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.headers.created-on') }}</td>
				<td>
					<QDateTime
						short-date
						:text="plexServer.createdAt"
					/>
				</td>
			</tr>
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.headers.last-seen-at') }}</td>
				<td>
					<QDateTime
						short-date
						:text="plexServer.lastSeenAt"
					/>
				</td>
			</tr>
			<tr>
				<td>{{ t('components.server-dialog.tabs.server-data.headers.current-status') }}</td>
				<td>
					<QStatus :value="serverStore.getServerStatus(plexServer.id)" />
				</td>
			</tr>
			<!--	Check Server Action	-->
			<tr>
				<td>
					<BaseButton
						text-id="check-server-status"
						:loading="checkServerStatusLoading"
						@click="checkServer"
					/>
				</td>
				<td style="padding: 0">
					<q-markup-table
						v-if="hasCheckedServerStatus"
						wrap-cells
					>
						<tbody v-if="checkServerStatusLoading">
							<tr
								v-for="(progressItem, index) in progress"
								:key="index"
							>
								<td>
									<QStatus
										pulse
										:value="progressItem.connectionSuccessful"
									/>
								</td>
								<td>{{ progressItem.message }}</td>
							</tr>
						</tbody>
						<tbody v-else-if="progress.every((x) => x.completed)">
							<tr>
								<td>
									<QStatus
										pulse
										:value="progress.some((x) => x.connectionSuccessful)"
									/>
								</td>
								<td>
									{{ checkServerStatusMessage }}
								</td>
							</tr>
						</tbody>
					</q-markup-table>
				</td>
			</tr>
			<!--			<tr v-if="settingsStore.debugMode"> -->
			<!--				<td colspan="2"> -->
			<!--					<print> -->
			<!--						{{ progress }} -->
			<!--					</print> -->
			<!--				</td> -->
			<!--			</tr> -->
		</tbody>
		<tbody v-else>
			<tr>
				<td>{{ t('general.error.invalid-server') }}</td>
			</tr>
		</tbody>
	</q-markup-table>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { get, set } from '@vueuse/core';
import type { PlexServerDTO, ServerConnectionCheckStatusProgressDTO } from '@dto';
import { useSignalrStore, useI18n } from '#imports';

const { t } = useI18n();
const signalrStore = useSignalrStore();

const serverStore = useServerStore();
const serverConnectionStore = useServerConnectionStore();
const checkServerStatusLoading = ref(false);
const hasCheckedServerStatus = ref(false);
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

const plexServerId = computed(() => props?.plexServer?.id ?? -1);

const checkServerStatusMessage = computed(() => {
	if (get(progress).length === 0) {
		return '';
	}
	if (get(progress).some((x) => x.connectionSuccessful)) {
		return t('components.server-dialog.tabs.server-data.at-least-one-connection-successful');
	}
	return t('components.server-dialog.tabs.server-data.all-connections-failed');
});

function checkServer() {
	set(hasCheckedServerStatus, true);
	set(checkServerStatusLoading, true);
	set(progress, []);
	useSubscription(
		serverConnectionStore.checkServerStatus(get(plexServerId)).subscribe(() => {
			set(checkServerStatusLoading, false);
		}),
	);
}

function setup() {
	useSubscription(
		signalrStore.getServerConnectionProgressByPlexServerId(get(plexServerId)).subscribe((progressData) => {
			set(progress, progressData);
		}),
	);
}

onMounted(() => {
	setup();
});

onUnmounted(() => {
	set(checkServerStatusLoading, false);
	set(hasCheckedServerStatus, false);
	set(progress, []);
});
</script>
