<template>
	<q-page>
		<!-- Download Toolbar -->
		<download-bar :has-selected="hasSelected" @action="batchCommandSwitch($event)" />

		<!--	The Download Table	-->
		<q-row v-if="getServersWithDownloads.length > 0" justify="center">
			<q-col cols="12">
				<q-list>
					<q-expansion-item
						v-for="{ plexServer, downloads } in getServersWithDownloads"
						:key="plexServer.id"
						default-opened
						class="background-sm default-border-radius q-ma-md">
						<template #header>
							<q-row align="center">
								<!-- Download Server Settings -->
								<q-col>
									<server-download-status />
								</q-col>
								<!-- Download Server Title -->
								<q-col cols="auto">
									<span class="title">{{ plexServer.name }}</span>
								</q-col>
								<q-col class="q-py-none"></q-col>
							</q-row>
						</template>
						<template #default>
							<div class="q-py-lg">
								<downloads-table
									v-model="selected"
									:selected="getSelected(plexServer.id)"
									:download-rows="downloads"
									:server-id="plexServer.id"
									@action="commandSwitch($event)"
									@selected="updateSelected(plexServer.id, $event)"
									@aggregate-selected="updateAggregateSelected(plexServer.id, $event)" />
							</div>
						</template>
					</q-expansion-item>
				</q-list>
			</q-col>
		</q-row>
		<q-row v-else justify="center">
			<q-col cols="auto">
				<h2>{{ $t('pages.downloads.no-downloads') }}</h2>
			</q-col>
		</q-row>
		<download-details-dialog :name="dialogName" />
	</q-page>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, computed, onMounted } from 'vue';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadService, ServerService } from '@service';
import { DownloadProgressDTO, PlexServerDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';
import { useOpenControlDialog } from '#imports';

const plexServers = ref<PlexServerDTO[]>([]);
const serverDownloads = ref<ServerDownloadProgressDTO[]>([]);
const openExpansions = ref<number[]>([]);
const selected = ref<ISelection[]>([]);
const aggregateSelected = ref<ISelection[]>([]);
const dialogName = 'download-details-dialog';

const getServersWithDownloads = computed((): { plexServer: PlexServerDTO; downloads: DownloadProgressDTO[] }[] => {
	const serverIds = get(serverDownloads).map((x) => x.id);
	const plexServersWithDownloads = get(plexServers).filter((x) => serverIds.includes(x.id));

	return plexServersWithDownloads.map((x) => {
		return {
			plexServer: x,
			downloads: get(serverDownloads).find((y) => y.id === x.id)?.downloads ?? [],
		};
	});
});

const hasSelected = computed(() => {
	return get(selected).some((x) => x.keys.length > 0);
});

// region single commands

function batchCommandSwitch(action: string) {
	const ids = get(aggregateSelected).flatMap((x) => x.keys);
	switch (action) {
		case 'clear':
			clearDownloadTasks(ids);
			break;
		case 'delete':
			deleteDownloadTasks(ids);
			break;
		default:
			Log.error(`Action: ${action} does not have a assigned batch command`);
	}
}

const commandSwitch = ({ action, item }: { action: string; item: DownloadProgressDTO }) => {
	const ids: number[] = [item.id];

	switch (action) {
		case 'pause':
			pauseDownloadTasks(item.id);
			break;
		case 'clear':
			clearDownloadTasks(ids);
			break;
		case 'delete':
			deleteDownloadTasks(ids);
			break;
		case 'stop':
			stopDownloadTasks(item.id);
			break;
		case 'restart':
			restartDownloadTasks(item.id);
			break;
		case 'start':
			startDownloadTasks(item.id);
			break;
		case 'details':
			detailsDownloadTask(item.id);
			break;
		default:
			Log.error(`Action: ${action} does not have a assigned command with payload: ${item}`, { action, item });
	}
};

function clearDownloadTasks(downloadTaskIds: number[]): void {
	if (downloadTaskIds && downloadTaskIds.length > 0) {
		DownloadService.clearDownloadTasks(downloadTaskIds);
		return;
	}

	DownloadService.clearDownloadTasks();
}

// endregion

// region Single commands

function startDownloadTasks(downloadTaskId: number): void {
	DownloadService.startDownloadTasks(downloadTaskId);
}

function pauseDownloadTasks(downloadTaskId: number): void {
	DownloadService.pauseDownloadTasks(downloadTaskId);
}

function stopDownloadTasks(downloadTaskId: number): void {
	DownloadService.stopDownloadTasks(downloadTaskId);
}

function restartDownloadTasks(downloadTaskId: number): void {
	DownloadService.restartDownloadTasks(downloadTaskId);
}

function deleteDownloadTasks(downloadTaskIds: number[]): void {
	DownloadService.deleteDownloadTasks(downloadTaskIds);
}

function detailsDownloadTask(downloadTaskId: number): void {
	useOpenControlDialog(dialogName, downloadTaskId);
}

// endregion

// region Selection
const getSelected = (id: number): ISelection => {
	return get(selected).find((x) => x.indexKey === id) as ISelection;
};

function updateSelected(id: number, payload: ISelection): void {
	const i = selected.value.findIndex((x) => x.indexKey === id);
	if (i === -1) {
		selected.value.push({ indexKey: id, keys: payload.keys, allSelected: payload.allSelected });
		return;
	}

	selected.value[i].allSelected = payload.allSelected;
	selected.value[i].keys = payload.keys;
}

function updateAggregateSelected(id: number, payload: ISelection): void {
	const i = get(aggregateSelected).findIndex((x) => x.indexKey === id);
	if (i === -1) {
		get(aggregateSelected).push({ indexKey: id, keys: payload.keys, allSelected: payload.allSelected });
		return;
	}

	get(aggregateSelected)[i].allSelected = payload.allSelected;
	get(aggregateSelected)[i].keys = payload.keys;
}

function createSelections() {
	for (const server of get(serverDownloads)) {
		if (get(selected).some((x) => x.indexKey === server.id)) {
			continue;
		}
		get(selected).push({
			keys: [],
			indexKey: server.id,
			allSelected: false,
		});
	}
}

// endregion
onMounted(() => {
	useSubscription(
		ServerService.getServers().subscribe((servers) => {
			plexServers.value = servers;
			openExpansions.value = [...Array(servers?.length).keys()] ?? [];
		}),
	);

	useSubscription(
		DownloadService.getServerDownloadList().subscribe((data) => {
			set(serverDownloads, data);
			createSelections();
		}),
	);
});
</script>
