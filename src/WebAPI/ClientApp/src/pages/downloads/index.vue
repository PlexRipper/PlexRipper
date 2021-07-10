<template>
	<page>
		<!-- Download Toolbar -->
		<download-bar
			:has-selected="hasSelected"
			@pause="pauseDownloadTasks(getSelected)"
			@stop="stopDownloadTasks(getSelected)"
			@restart="restartDownloadTasks(getSelected)"
			@start="startDownloadTasks(getSelected)"
			@clear="clearDownloadTasks(getSelected)"
			@delete="deleteDownloadTasks(getSelected)"
		/>
		<!--	The Download Table	-->
		<perfect-scrollbar class="download-page-tables">
			<v-row v-if="plexServers.length > 0">
				<v-col>
					<v-expansion-panels v-model="openExpansions" multiple>
						<v-expansion-panel v-for="plexServer in getServersWithDownloads" :key="plexServer.id">
							<v-expansion-panel-header>
								<h2>{{ plexServer.name }}</h2>
							</v-expansion-panel-header>
							<v-expansion-panel-content>
								<downloads-table
									v-model="selected"
									:server-id="plexServer.id"
									@selected="updateSelected(plexServer.id, $event)"
									@pause="pauseDownloadTasks([$event])"
									@clear="clearDownloadTasks([$event])"
									@delete="deleteDownloadTasks([$event])"
									@stop="stopDownloadTasks([$event])"
									@restart="restartDownloadTasks([$event])"
									@start="startDownloadTasks([$event])"
									@details="detailsDownloadTask($event)"
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
import { Component, Vue } from 'vue-property-decorator';
import DownloadService from '@state/downloadService';
import ServerService from '@state/serverService';
import { DownloadTaskDTO, PlexServerDTO } from '@dto/mainApi';
import LoadingSpinner from '@/components/LoadingSpinner.vue';
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
		return this.selected.map((x) => +x.keys).flat(1);
	}

	get getServersWithDownloads(): PlexServerDTO[] {
		return this.plexServers.filter((x) => this.downloads.some((y) => y.plexServerId === x.id));
	}

	get hasSelected(): boolean {
		return this.getSelected.length > 0;
	}

	// region single commands

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
	clearDownloadTasks(downloadTaskIds: number[]): void {
		if (downloadTaskIds && downloadTaskIds.length > 0) {
			DownloadService.clearDownloadTasks(downloadTaskIds);
			return;
		}

		if (this.hasSelected) {
			DownloadService.clearDownloadTasks(this.getSelected);
			this.selected = [];
		} else {
			DownloadService.clearDownloadTasks();
		}
	}

	startDownloadTasks(downloadTaskIds: number[]): void {
		DownloadService.startDownloadTasks(downloadTaskIds);
	}

	pauseDownloadTasks(downloadTaskIds: number[]): void {
		DownloadService.pauseDownloadTasks(downloadTaskIds);
	}

	stopDownloadTasks(downloadTaskIds: number[]): void {
		DownloadService.stopDownloadTasks(downloadTaskIds);
	}

	restartDownloadTasks(downloadTaskIds: number[]): void {
		DownloadService.restartDownloadTasks(downloadTaskIds);
	}

	deleteDownloadTasks(downloadTaskIds: number[]): void {
		DownloadService.deleteDownloadTasks(downloadTaskIds);
	}

	// endregion

	closeDetailsDialog(): void {
		this.downloadTaskDetail = null;
		this.dialog = false;
	}

	created(): void {
		this.$subscribeTo(ServerService.getServers(), (servers) => {
			this.plexServers = servers;
			this.openExpansions = [...Array(servers?.length).keys()] ?? [];
		});

		this.$subscribeTo(DownloadService.getDownloadList(), (downloads) => {
			this.downloads = downloads;
		});
	}
}
</script>
