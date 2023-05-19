<template>
	<v-dialog :max-width="1200" :value="isVisible" :width="1200" @click:outside="close">
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

					<!--	Server Connections Tab Header	-->
					<v-tab>
						<v-icon left> mdi-connection</v-icon>
						{{ $t('components.server-dialog.tabs.server-connections.header') }}
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
						<server-data-tab-content :plex-server="plexServer" :is-visible="isVisible" />
					</v-tab-item>

					<!--	Server Connections Tab Content	-->
					<v-tab-item>
						<ServerConnectionsTabContent
							:plex-server="plexServer"
							:plex-server-settings="plexServerSettings"
							:is-visible="isVisible"
						/>
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
						<server-commands-tab-content :plex-server="plexServer" :is-visible="isVisible" />
					</v-tab-item>
				</v-tabs>
			</v-card-text>
		</v-card>
		<v-card v-else>
			<h1>{{ $t('components.server-dialog.no-servers-error') }}</h1>
		</v-card>
	</v-dialog>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { FolderPathDTO, PlexLibraryDTO, PlexServerDTO, PlexServerSettingsModel } from '@dto/mainApi';
import { FolderPathService, LibraryService, ServerService, SettingsService } from '@service';

@Component
export default class ServerDialog extends Vue {
	show: boolean = false;
	tabIndex: number | null = null;
	plexServer: PlexServerDTO | null = null;
	folderPaths: FolderPathDTO[] = [];
	plexLibraries: PlexLibraryDTO[] = [];
	plexServerSettings: PlexServerSettingsModel | null = null;
	plexServerId: number = 0;

	get isVisible(): boolean {
		return this.plexServerId > 0;
	}

	open(plexServerId: number): void {
		this.plexServerId = plexServerId;
		this.show = true;

		useSubscription(
			ServerService.getServer(plexServerId)
				.pipe(
					tap((plexServer) => {
						this.plexServer = plexServer;
					}),
					switchMap((plexServer) => SettingsService.getServerSettings(plexServer?.machineIdentifier ?? '')),
				)
				.subscribe((plexServerSettings) => {
					this.plexServerSettings = plexServerSettings;
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
	}

	close(): void {
		this.show = false;
		this.plexServerId = 0;
		this.tabIndex = null;
	}
}
</script>
