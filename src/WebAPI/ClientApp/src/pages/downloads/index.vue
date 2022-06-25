<template>
	<page-container>
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
		<vue-scroll class="download-page-tables">
			<v-row v-if="getServersWithDownloads.length > 0">
				<v-col>
					<print :object="selected" />
					<v-expansion-panels v-model="openExpansions" multiple>
						<v-expansion-panel v-for="plexServer in getServersWithDownloads" :key="plexServer.id">
							<v-expansion-panel-header>
								<v-row no-gutters>
									<!-- Download Server Settings -->
									<v-col>
										<server-download-status />
									</v-col>
									<!-- Download Server Title -->
									<v-col cols="auto">
										<h2>{{ plexServer.name }}</h2>
									</v-col>
									<v-col class="py-0"> </v-col>
								</v-row>
							</v-expansion-panel-header>
							<v-expansion-panel-content>
								<downloads-table
									v-model="selected"
									:download-rows="plexServer.downloadTasks"
									:server-id="plexServer.id"
									@action="commandSwitch"
									@selected="updateSelected(plexServer.id, $event)"
								/>
							</v-expansion-panel-content>
						</v-expansion-panel>
					</v-expansion-panels>
				</v-col>
			</v-row>
			<v-row v-else justify="center">
				<v-col cols="auto">
					<h2>{{ $t('pages.downloads.no-downloads') }}</h2>
				</v-col>
			</v-row>
		</vue-scroll>
		<download-details-dialog :download-task="downloadTaskDetail" :dialog="dialog" @close="closeDetailsDialog" />
	</page-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import { DownloadService, ServerService } from '@service';
import { DownloadTaskDTO, PlexServerDTO, ServerDownloadProgressDTO } from '@dto/mainApi';
import { detailDownloadTask } from '@api/plexDownloadApi';
declare interface ISelection {
	plexServerId: number;
	downloadTaskIds: number[];
}

@Component({
	components: {},
})
export default class Downloads extends Vue {
	plexServers: PlexServerDTO[] = [];
	serverDownloads: ServerDownloadProgressDTO[] = [];
	openExpansions: number[] = [];
	downloadTaskDetail: DownloadTaskDTO | null = null;
	selected: ISelection[] = [];

	private dialog: boolean = false;

	get getSelected(): number[] {
		return this.selected.map((x) => x.downloadTaskIds).flat(1);
	}

	get getServersWithDownloads(): PlexServerDTO[] {
		const serverIds = this.serverDownloads.map((x) => x.id);
		const plexServers = this.plexServers.filter((x) => serverIds.includes(x.id));
		for (const plexServer of plexServers) {
			plexServer.downloadTasks = this.serverDownloads.find((x) => x.id === plexServer.id)?.downloads ?? [];
		}
		return plexServers;
	}

	get hasSelected(): boolean {
		return this.getSelected.length > 0;
	}

	// region single commands

	commandSwitch({ action, item }: { action: string; item: DownloadTaskDTO }) {
		const ids = [item.id];
		switch (action) {
			case 'pause':
				this.pauseDownloadTasks(item.id);
				break;
			case 'clear':
				this.clearDownloadTasks(ids);
				break;
			case 'delete':
				this.deleteDownloadTasks(ids);
				break;
			case 'stop':
				this.stopDownloadTasks(item.id);
				break;
			case 'restart':
				this.restartDownloadTasks(item.id);
				break;
			case 'start':
				this.startDownloadTasks(item.id);
				break;
			case 'details':
				this.detailsDownloadTask(item);
				break;
			default:
				Log.error(`Action: ${action} does not have a assigned command with payload: ${item}`, { action, item });
		}
	}

	detailsDownloadTask(downloadTask: DownloadTaskDTO): void {
		this.dialog = true;
		detailDownloadTask(downloadTask.id).subscribe((downloadTaskDetail) => {
			if (downloadTaskDetail.isSuccess && downloadTaskDetail.value) {
				this.downloadTaskDetail = downloadTaskDetail.value;
			}
		});
	}

	updateSelected(plexServerId: number, downloadTaskIds: number[]) {
		const index = this.selected.findIndex((x) => x.plexServerId === plexServerId);
		if (index === -1) {
			this.selected.push({ plexServerId, downloadTaskIds });
		} else {
			this.selected.splice(index, 1, { plexServerId, downloadTaskIds });
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

	startDownloadTasks(downloadTaskId: number): void {
		DownloadService.startDownloadTasks(downloadTaskId);
	}

	pauseDownloadTasks(downloadTaskId: number): void {
		DownloadService.pauseDownloadTasks(downloadTaskId);
	}

	stopDownloadTasks(downloadTaskId: number): void {
		DownloadService.stopDownloadTasks(downloadTaskId);
	}

	restartDownloadTasks(downloadTaskId: number): void {
		DownloadService.restartDownloadTasks(downloadTaskId);
	}

	deleteDownloadTasks(downloadTaskIds: number[]): void {
		DownloadService.deleteDownloadTasks(downloadTaskIds);
	}

	// endregion

	closeDetailsDialog(): void {
		this.downloadTaskDetail = null;
		this.dialog = false;
	}

	mounted(): void {
		this.$subscribeTo(ServerService.getServers(), (servers) => {
			this.plexServers = servers;
			this.openExpansions = [...Array(servers?.length).keys()] ?? [];
		});

		this.$subscribeTo(DownloadService.getServerDownloadList(), (downloads) => {
			this.serverDownloads = downloads;
		});
	}
}
</script>
