<template>
	<q-page>
		<!-- Download Toolbar -->
		<download-bar
			:has-selected="hasSelected"
			@pause="pauseDownloadTasks(getSelected)"
			@stop="stopDownloadTasks(getSelected)"
			@restart="restartDownloadTasks(getSelected)"
			@start="startDownloadTasks(getSelected)"
			@clear="clearDownloadTasks(getSelected)"
			@delete="deleteDownloadTasks(getSelected)" />
		<!--	The Download Table	-->
		<q-row v-if="getServersWithDownloads.length > 0">
			<q-col>
				<q-list>
					<q-expansion-item v-for="plexServer in getServersWithDownloads" :key="plexServer.id">
						<template #header>
							<q-row no-gutters>
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
						<template #body>
							<downloads-table
								v-model="selected"
								:download-rows="plexServer.downloadTasks"
								:server-id="plexServer.id"
								@action="commandSwitch"
								@selected="updateSelected(plexServer.id, $event)" />
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
import { get } from '@vueuse/core';
import { useSubscription } from '@vueuse/rxjs';
import { DownloadService, ServerService } from '@service';
import { DownloadTaskDTO, PlexServerDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import { detailDownloadTask } from '@api/plexDownloadApi';

declare interface ISelection {
	plexServerId: number;
	downloadTaskIds: number[];
}

const plexServers = ref<Readonly<PlexServerDTO[]>>([]);
const serverDownloads = ref<Readonly<ServerDownloadProgressDTO[]>>([]);
const openExpansions = ref<Readonly<number[]>>([]);
const downloadTaskDetail = ref<DownloadTaskDTO | null>(null);
const selected = ref<ISelection[]>([]);
const dialog = ref<boolean>(false);

const getSelected = computed(() => selected.value.map((x) => x.downloadTaskIds).flat(1));

const getServersWithDownloads = computed(() => {
	const serverIds = get(serverDownloads).map((x) => x.id);
	const plexServersWithDownloads = get(plexServers).filter((x) => serverIds.includes(x.id));
	for (const plexServer of plexServersWithDownloads) {
		plexServer.downloadTasks = get(serverDownloads).find((x) => x.id === plexServer.id)?.downloads ?? [];
	}
	return plexServersWithDownloads;
});

const hasSelected = computed(() => getSelected.value.length > 0);

// region single commands

const commandSwitch = ({ action, item }: { action: string; item: DownloadTaskDTO }) => {
	const ids = [item.id];
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

function updateSelected(plexServerId: number, downloadTaskIds: number[]): void {
	const index = selected.value.findIndex((x) => x.plexServerId === plexServerId);
	if (index === -1) {
		selected.value.push({ plexServerId, downloadTaskIds });
	} else {
		selected.value.splice(index, 1, { plexServerId, downloadTaskIds });
	}
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

onMounted(() => {
	useSubscription(
		ServerService.getServers().subscribe((servers) => {
			plexServers.value = servers;
			openExpansions.value = [...Array(servers?.length).keys()] ?? [];
		}),
	);

	useSubscription(
		DownloadService.getServerDownloadList().subscribe((downloads) => {
			serverDownloads.value = downloads;
		}),
	);
});
</script>
