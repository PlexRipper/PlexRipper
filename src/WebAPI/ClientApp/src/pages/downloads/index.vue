<template>
	<page>
		{{ getSelected }}
		<!-- Download Toolbar -->
		<download-bar
			:has-selected="hasSelected"
			@pause="pauseDownloadTasks"
			@stop="stopDownloadTasks"
			@restart="restartDownloadTasks"
			@start="startDownloadTasks"
			@clear="clearDownloadTasks"
			@delete="deleteDownloadTasks"
		/>
		<!--	The Download Table	-->
		<perfect-scrollbar class="download-page-tables">
			<v-row v-if="plexServers.length > 0">
				<v-col>
					<v-expansion-panels v-model="openExpansions" multiple>
						<v-expansion-panel v-for="plexServer in plexServers" :key="plexServer.id">
							<v-expansion-panel-header>
								<h2>{{ plexServer.name }}</h2>
							</v-expansion-panel-header>
							<v-expansion-panel-content>
								<downloads-table
									v-model="selected"
									:server-id="plexServer.id"
									@selected="updateSelected(plexServer.id, $event)"
									@pause="pauseDownloadTask"
									@clear="clearDownloadTask"
									@delete="deleteDownloadTask"
									@stop="stopDownloadTask"
									@restart="restartDownloadTask"
									@start="startDownloadTask"
									@details="detailsDownloadTask"
								/>
							</v-expansion-panel-content>
						</v-expansion-panel>
					</v-expansion-panels>
				</v-col>
			</v-row>
			<v-row v-else justify="center">
				<v-col cols="auto">
					<h2>There are currently no downloads in progress</h2>
				</v-col>
			</v-row>
		</perfect-scrollbar>
		<download-details-dialog :download-task="downloadTaskDetail" :dialog="dialog" @close="closeDetailsDialog" />
	</page>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { pauseDownloadTask, restartDownloadTask, startDownloadTask } from '@api/plexDownloadApi';
import DownloadService from '@state/downloadService';
import { DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
import { filter } from 'lodash';
import ISelection from '@interfaces/ISelection';
import DownloadsTable from './components/DownloadsTable.vue';
import DownloadBar from '~/pages/downloads/components/DownloadBar.vue';
import DownloadDetailsDialog from '~/pages/downloads/components/DownloadDetailsDialog.vue';

@Component({
	components: {
		LoadingSpinner,
		DownloadsTable,
		DownloadBar,
		DownloadDetailsDialog,
	},
})
export default class Downloads extends Vue {
	plexServers: PlexServerDTO[] = [];
	downloads: DownloadTaskDTO[] = [];
	openExpansions: number[] = [];
	downloadTaskDetail: DownloadTaskDTO | null = null;
	selected: ISelection[] = [];

	private dialog: boolean = false;

	get getSelected(): number[] {
		return this.selected.map((x) => x.keys).flat(1);
	}

	get hasSelected(): boolean {
		return this.getSelected.length > 0;
	}

	// region single commands

	clearDownloadTask(downloadTask: DownloadTaskDTO): void {
		DownloadService.clearDownloadTasks([downloadTask.id]);
		this.selected = filter(this.selected, (x) => x.id !== downloadTask.id);
	}

	startDownloadTask(downloadTask: DownloadTaskDTO): void {
		this.$subscribeTo(startDownloadTask(downloadTask.id), () => {});
	}

	pauseDownloadTask(downloadTask: DownloadTaskDTO): void {
		this.$subscribeTo(pauseDownloadTask(downloadTask.id), () => {});
	}

	stopDownloadTask(downloadTask: DownloadTaskDTO): void {
		DownloadService.stopDownloadTasks([downloadTask.id]);
	}

	restartDownloadTask(downloadTask: DownloadTaskDTO): void {
		this.$subscribeTo(restartDownloadTask(downloadTask.id), () => {});
	}

	deleteDownloadTask(downloadTask: DownloadTaskDTO): void {
		DownloadService.deleteDownloadTasks([downloadTask.id]);
		this.selected = filter(this.selected, (x) => x.id !== downloadTask.id);
	}

	detailsDownloadTask(downloadTask: DownloadTaskDTO): void {
		this.downloadTaskDetail = downloadTask;
		this.dialog = true;
	}

	updateSelected(plexServerId: number, selected: string[]) {
		const index = this.selected.findIndex((x) => x.indexKey === plexServerId);
		if (index === -1) {
			this.selected.push({ indexKey: plexServerId, keys: selected });
		} else {
			this.selected.splice(index, 1, { indexKey: plexServerId, keys: selected });
		}
	}

	// endregion

	// region batch commands
	clearDownloadTasks(): void {
		if (!this.hasSelected) {
			DownloadService.clearDownloadTasks([]);
		} else {
			DownloadService.clearDownloadTasks(this.getSelected);
			this.selected = [];
		}
	}

	startDownloadTasks(): void {
		Log.info('startDownloadTasks not implemented');
	}

	pauseDownloadTasks(): void {
		Log.info('pauseDownloadTasks not implemented');
	}

	stopDownloadTasks(): void {
		Log.info('stopDownloadTasks not implemented');
	}

	restartDownloadTasks(): void {
		Log.info('restartDownloadTasks not implemented');
	}

	deleteDownloadTasks(): void {
		DownloadService.deleteDownloadTasks(this.getSelected);
		this.selected = [];
	}

	// endregion

	closeDetailsDialog(): void {
		this.downloadTaskDetail = null;
		this.dialog = false;
	}

	created(): void {
		this.$subscribeTo(DownloadService.getDownloadListInServers(), (data) => {
			this.plexServers = data;
			this.openExpansions = [...Array(this.plexServers?.length).keys()] ?? [];
		});
	}
}
</script>
