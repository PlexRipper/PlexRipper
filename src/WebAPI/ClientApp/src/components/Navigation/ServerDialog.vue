<template>
	<v-dialog :value="serverId > 0" :width="1200" :max-width="1200" @click:outside="close">
		<v-card v-if="plexServer">
			<v-card-title class="headline">{{ $t('components.server-dialog.header', { serverName: plexServer.name }) }} </v-card-title>

			<v-card-text>
				<v-tabs v-model="tabIndex" vertical>
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
						<server-config-tab-content :plex-server="plexServer" :plex-server-settings="plexServerSettings" />
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
import { combineLatest } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';
import { FolderPathDTO, PlexLibraryDTO, PlexServerDTO, PlexServerSettingsModel, PlexServerStatusDTO } from '@dto/mainApi';
import { FolderPathService, LibraryService, ServerService, SettingsService } from '@service';

@Component<ServerDialog>({})
export default class ServerDialog extends Vue {
	@Prop({ required: true, type: Number, default: 0 })
	readonly serverId!: number;

	show: boolean = false;
	tabIndex: number | null = null;
	plexServer: PlexServerDTO | null = null;
	folderPaths: FolderPathDTO[] = [];
	plexLibraries: PlexLibraryDTO[] = [];
	plexServerSettings: PlexServerSettingsModel | null = null;

	get serverStatus(): PlexServerStatusDTO | null {
		return this.plexServer?.status ?? null;
	}

	close(): void {
		this.$emit('close');
		this.tabIndex = null;
	}

	mounted(): void {
		this.$subscribeTo(
			this.$watchAsObservable('serverId').pipe(
				map((x: { oldValue: number; newValue: number }) => x.newValue),
				switchMap((plexServerId) =>
					combineLatest([
						ServerService.getServer(plexServerId),
						LibraryService.getLibrariesByServerId(plexServerId),
						FolderPathService.getFolderPaths(),
						SettingsService.getServerSettings(plexServerId),
					]),
				),
			),
			([plexServer, plexLibraries, folderPaths, plexServerSettings]: [
				PlexServerDTO | null,
				PlexLibraryDTO[],
				FolderPathDTO[],
				PlexServerSettingsModel,
			]) => {
				if (plexServer) {
					this.plexServer = plexServer;
				}

				if (plexLibraries) {
					this.plexLibraries = plexLibraries;
				}

				if (folderPaths) {
					this.folderPaths = folderPaths;
				}

				if (plexServerSettings) {
					this.plexServerSettings = plexServerSettings;
				}
			},
		);
	}
}
</script>
