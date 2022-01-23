<template>
	<v-dialog :value="serverId > 0" max-width="1200" @click:outside="close">
		<v-card v-if="plexServer">
			<v-card-title class="headline">{{ $t('components.server-dialog.header', { serverName: plexServer.name }) }} </v-card-title>

			<v-card-text>
				<v-tabs vertical>
					<!--	Server Data	Tab Header-->
					<v-tab>
						<v-icon left> mdi-server </v-icon>
						{{ $t('components.server-dialog.tabs.server-data.header') }}
					</v-tab>

					<!--	Server Configuration Tab Header	-->
					<v-tab>
						<v-icon left> mdi-cog-box </v-icon>
						{{ $t('components.server-dialog.tabs.server-config.header') }}
					</v-tab>

					<!--	Library Destinations Tab Header	-->
					<v-tab>
						<v-icon left> mdi-folder-edit-outline </v-icon>
						{{ $t('components.server-dialog.tabs.download-destinations.header') }}
					</v-tab>

					<!--	Server Commands Tab Header	-->
					<v-tab>
						<v-icon left> mdi-console </v-icon>
						{{ $t('components.server-dialog.tabs.server-commands.header') }}
					</v-tab>

					<!--	Server Data Tab Content	-->
					<v-tab-item>
						<server-data-tab-content :plex-server="plexServer" :server-status="serverStatus" />
					</v-tab-item>

					<!--	Server Configuration Tab Content	-->
					<v-tab-item>
						<server-config-tab-content :plex-server="plexServer" />
					</v-tab-item>

					<!--	Library Download Destinations	Tab Content -->
					<v-tab-item>
						<server-library-destinations-tab-content :folder-paths="folderPaths" :plex-libraries="plexLibraries" />
					</v-tab-item>

					<!--	Server Commands -->
					<v-tab-item>
						<server-commands-tab-content :plex-server="plexServer" />
					</v-tab-item>
				</v-tabs>
			</v-card-text>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Prop, Vue } from 'vue-property-decorator';
import { FolderPathDTO, PlexLibraryDTO, PlexServerDTO, PlexServerStatusDTO } from '@dto/mainApi';
import { FolderPathService, LibraryService, ServerService } from '@service';
import { map, switchMap } from 'rxjs/operators';
import { combineLatest } from 'rxjs';

@Component<ServerDialog>({
	components: {},
})
export default class ServerDialog extends Vue {
	@Prop({ required: true, type: Number, default: 0 })
	readonly serverId!: number;

	show: boolean = false;

	plexServer: PlexServerDTO | null = null;
	folderPaths: FolderPathDTO[] = [];
	plexLibraries: PlexLibraryDTO[] = [];

	get serverStatus(): PlexServerStatusDTO | null {
		return this.plexServer?.status ?? null;
	}

	close(): void {
		this.$emit('close');
	}

	mounted(): void {
		this.$subscribeTo(
			this.$watchAsObservable('serverId').pipe(
				map((x: { oldValue: number; newValue: number }) => x.newValue),
				switchMap((value) =>
					combineLatest([
						ServerService.getServer(value),
						LibraryService.getLibrariesByServerId(value),
						FolderPathService.getFolderPaths(),
					]),
				),
			),
			([plexServer, plexLibraries, folderPaths]: [PlexServerDTO | null, PlexLibraryDTO[], FolderPathDTO[]]) => {
				if (plexServer) {
					this.plexServer = plexServer;
				}
				if (plexLibraries) {
					this.plexLibraries = plexLibraries;
				}
				if (folderPaths) {
					this.folderPaths = folderPaths;
				}
			},
		);
	}
}
</script>
