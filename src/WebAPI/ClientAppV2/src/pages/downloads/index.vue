<template>
	<q-page>
		<!-- Download Toolbar -->
		<download-bar :has-selected="hasSelected" @action="commandSwitch({ action: $event, item: null })" />
		{{ aggregateSelected }}

		<!--	The Download Table	-->
		<q-row v-if="getServersWithDownloads.length > 0" justify="center">
			<q-col cols="12">
				<q-list>
					<q-expansion-item
						v-for="plexServer in getServersWithDownloads"
						:key="plexServer.id"
						default-opened
						class="extra-background q-ma-md">
						<template #header>
							<q-row align="center">
								<!-- Download Server Settings -->
								<q-col>
									<server-download-status />
								</q-col>
								<!-- Download Server Title -->
								<q-col cols="auto">
									<h2>{{ plexServer.name }}</h2>
								</q-col>
								<q-col class="py-0"></q-col>
							</q-row>
						</template>
						<template #default>
							<downloads-table
								v-model="selected"
								:selected="getSelected(plexServer.id)"
								:download-rows="plexServer.downloadTasks"
								:server-id="plexServer.id"
								@action="commandSwitch($event)"
								@selected="updateSelected(plexServer.id, $event)"
								@aggregate-selected="updateAggregateSelected(plexServer.id, $event)" />
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
		<download-details-dialog :download-task="downloadTaskDetail" :dialog="dialog" @close="closeDetailsDialog" />
	</q-page>
</template>

<script setup lang="ts">
import Log from 'consola';
import { ref, computed } from 'vue';
import { get, set } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadService, ServerService } from '@service';
import { DownloadProgressDTO, DownloadTaskDTO, PlexServerDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import { detailDownloadTask } from '@api/plexDownloadApi';
import ISelection from '@interfaces/ISelection';

const plexServers = ref<PlexServerDTO[]>([]);
const serverDownloads = ref<ServerDownloadProgressDTO[]>([]);
const openExpansions = ref<number[]>([]);
const downloadTaskDetail = ref<DownloadTaskDTO | null>(null);
const selected = ref<ISelection[]>([]);
const aggregateSelected = ref<ISelection[]>([]);
const dialog = ref<boolean>(false);

const getServersWithDownloads = computed(() => {
	const serverIds = get(serverDownloads).map((x) => x.id);
	const plexServersWithDownloads = get(plexServers).filter((x) => serverIds.includes(x.id));
	for (const plexServer of plexServersWithDownloads) {
		plexServer.downloadTasks = get(serverDownloads).find((x) => x.id === plexServer.id)?.downloads ?? [];
	}
	return plexServersWithDownloads;
});

const hasSelected = computed(() => {
	return get(selected).some((x) => x.keys.length > 0);
});

// region single commands

const commandSwitch = ({ action, item }: { action: string; item: DownloadProgressDTO | null }) => {
	const ids: number[] = [];
	if (item) {
		ids.push(item.id);
	} else if (hasSelected.value) {
		ids.push(...getSelected());
	} else {
		return;
	}
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
			detailsDownloadTask(item);
			break;
		default:
			Log.error(`Action: ${action} does not have a assigned command with payload: ${item}`, { action, item });
	}
};

function detailsDownloadTask(downloadTask: DownloadTaskDTO): void {
	dialog.value = true;
	detailDownloadTask(downloadTask.id).subscribe((data) => {
		if (data.isSuccess && data.value) {
			downloadTaskDetail.value = data.value;
		}
	});
}

function clearDownloadTasks(downloadTaskIds: number[]): void {
	if (downloadTaskIds && downloadTaskIds.length > 0) {
		DownloadService.clearDownloadTasks(downloadTaskIds);
		return;
	}

	if (hasSelected.value) {
		DownloadService.clearDownloadTasks(getSelected.value);
		selected.value = [];
	} else {
		DownloadService.clearDownloadTasks();
	}
}

// endregion

// region batch commands

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

// endregion

function closeDetailsDialog(): void {
	downloadTaskDetail.value = null;
	dialog.value = false;
}

// region Selection
const getSelected = (id: number): ISelection => {
	const result = get(selected).find((x) => x.indexKey === id);
	return result as ISelection;
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
