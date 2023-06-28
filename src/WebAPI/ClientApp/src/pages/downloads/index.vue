<template>
	<q-page>
		<!-- Download Toolbar -->
		<download-bar :has-selected="downloadStore.hasSelected" @action="batchCommandSwitch($event)" />

		<!--	The Download Table	-->
		<q-row v-if="downloadStore.getServersWithDownloads.length > 0" justify="center">
			<q-col cols="12">
				<q-list>
					<q-expansion-item
						v-for="{ plexServer, downloads } in downloadStore.getServersWithDownloads"
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
									v-model="downloadStore.selected"
									:selected="downloadStore.getSelected(plexServer.id)"
									:download-rows="downloads"
									:server-id="plexServer.id"
									@action="commandSwitch($event)"
									@selected="downloadStore.updateSelected(plexServer.id, $event)"
									@aggregate-selected="updateAggregateSelected(plexServer.id, $event)" />
							</div>
						</template>
					</q-expansion-item>
				</q-list>
			</q-col>
		</q-row>
		<q-row v-else justify="center">
			<q-col cols="auto">
				<h2>{{ t('pages.downloads.no-downloads') }}</h2>
			</q-col>
		</q-row>
		<download-details-dialog :name="dialogName" />
	</q-page>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { DownloadProgressDTO } from '@dto/mainApi';
import ISelection from '@interfaces/ISelection';
import { useOpenControlDialog } from '#imports';

const { t } = useI18n();
const downloadStore = useDownloadStore();
const aggregateSelected = ref<ISelection[]>([]);
const dialogName = 'download-details-dialog';

// region single commands

function batchCommandSwitch(action: string) {
	const ids = get(aggregateSelected).flatMap((x) => x.keys);
	downloadStore.executeDownloadCommand(action, ids);
}

const commandSwitch = ({ action, item }: { action: string; item: DownloadProgressDTO }) => {
	const ids: number[] = [item.id];

	if (action === 'details') {
		useOpenControlDialog(dialogName, item.id);
		return;
	}

	downloadStore.executeDownloadCommand(action, ids);
};

// endregion

// region Selection

function updateAggregateSelected(id: number, payload: ISelection): void {
	const i = get(aggregateSelected).findIndex((x) => x.indexKey === id);
	if (i === -1) {
		get(aggregateSelected).push({ indexKey: id, keys: payload.keys, allSelected: payload.allSelected });
		return;
	}

	get(aggregateSelected)[i].allSelected = payload.allSelected;
	get(aggregateSelected)[i].keys = payload.keys;
}

// endregion
</script>
