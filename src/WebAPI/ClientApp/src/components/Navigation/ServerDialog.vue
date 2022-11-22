<template>
	<v-dialog :max-width="1200" :value="plexServerId > 0" :width="1200" @click:outside="close">
		<v-card v-if="plexServer">
			<v-card-title class="headline"
				>{{ $t('components.server-dialog.header', { serverName: plexServer.name }) }}
			</v-card-title>

			<v-card-text>
				<v-tabs v-model="tabIndex" vertical>
					<!--	Server Data	Tab Header-->
					<v-tab>
						<v-icon left> mdi-server</v-icon>
						{{ $t('components.server-dialog.tabs.server-data.header') }}
					</v-tab>

					<!--	Server Configuration Tab Header	-->
					<v-tab>
						<v-icon left> mdi-cog-box</v-icon>
						{{ $t('components.server-dialog.tabs.server-config.header') }}
					</v-tab>

					<!--	Library Destinations Tab Header	-->
					<v-tab>
						<v-icon left> mdi-folder-edit-outline</v-icon>
						{{ $t('components.server-dialog.tabs.download-destinations.header') }}
					</v-tab>

					<!--	Server Commands Tab Header	-->
					<v-tab>
						<v-icon left> mdi-console</v-icon>
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
		<v-card v-else>
			<h1>PlexServer object was invalid</h1>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { FolderPathDTO, PlexLibraryDTO, PlexServerDTO, PlexServerSettingsModel, PlexServerStatusDTO } from '@dto/mainApi';
import { FolderPathService, LibraryService, ServerService, SettingsService } from '@service';

@Component<ServerDialog>({})
export default class ServerDialog extends Vue {
	show: boolean = false;
	tabIndex: number | null = null;
	plexServer: PlexServerDTO | null = null;
	folderPaths: FolderPathDTO[] = [];
	plexLibraries: PlexLibraryDTO[] = [];
	plexServerSettings: PlexServerSettingsModel | null = null;
	plexServerId: number = 0;

	get serverStatus(): PlexServerStatusDTO | null {
		return this.plexServer?.status ?? null;
	}

	open(plexServerId: number): void {
		this.plexServerId = plexServerId;
		this.show = true;

		useSubscription(
			ServerService.getServer(plexServerId).subscribe((plexServer) => {
				this.plexServer = plexServer;
			}),
		);
		useSubscription(
			LibraryService.getLibrariesByServerId(plexServerId).subscribe((plexLibraries) => {
				this.plexLibraries = plexLibraries;
			}),
		);
		useSubscription(
			FolderPathService.getFolderPaths().subscribe((folderPaths) => {
				this.folderPaths = folderPaths;
			}),
		);
		useSubscription(
			SettingsService.getServerSettings(plexServerId).subscribe((plexServerSettings) => {
				this.plexServerSettings = plexServerSettings;
			}),
		);
	}

	close(): void {
		this.show = false;
		this.plexServerId = 0;
		this.tabIndex = null;
	}
}
</script>
