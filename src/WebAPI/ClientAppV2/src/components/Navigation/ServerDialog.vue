<template>
	<q-dialog v-model="show" @click:outside="close">
		<q-card v-if="plexServer" class="server-dialog-content">
			<q-card-section>
				<div class="text-h6">
					{{ $t('components.server-dialog.header', { serverName: plexServer.name }) }}
				</div>
			</q-card-section>

			<q-separator />

			<q-splitter v-model="splitterModel">
				<!-- Tab Index -->
				<template #before>
					<q-tabs v-model="tabIndex" vertical>
						<!--	Server Data	Tab Header-->
						<q-tab
							name="server-data"
							icon="mdi-server"
							:label="$t('components.server-dialog.tabs.server-data.header')" />
						<!--	Server Connections Tab Header	-->
						<q-tab
							name="server-connection"
							icon="mdi-connection"
							:label="$t('components.server-dialog.tabs.server-connections.header')" />
						<!--	Server Configuration Tab Header	-->
						<q-tab
							name="server-config"
							icon="mdi-cog-box"
							:label="$t('components.server-dialog.tabs.server-config.header')" />
						<!--	Library Destinations Tab Header	-->
						<q-tab
							name="download-destinations"
							icon="mdi-folder-edit-outline"
							:label="$t('components.server-dialog.tabs.download-destinations.header')" />
						<!--	Server Commands Tab Header	-->
						<q-tab
							name="server-commands"
							icon="mdi-console"
							:label="$t('components.server-dialog.tabs.server-commands.header')" />
					</q-tabs>
				</template>
				<!-- Tab Content -->
				<template #after>
					<q-tab-panels
						v-model="tabIndex"
						animated
						swipeable
						vertical
						transition-prev="jump-up"
						transition-next="jump-up">
						<!-- Server Data Tab Content -->
						<q-tab-panel name="server-data">
							<ServerDataTabContent ref="serverDataTabContent" :plex-server="plexServer" :is-visible="isVisible" />
						</q-tab-panel>

						<!-- Server Connections Tab Content	-->
						<q-tab-panel name="server-connection">
							<ServerConnectionsTabContent
								ref="serverConnectionsTabContent"
								:plex-server="plexServer"
								:plex-server-settings="plexServerSettings"
								:is-visible="isVisible" />
						</q-tab-panel>

						<!--	Server Configuration Tab Content	-->
						<q-tab-panel name="server-config">
							<server-config-tab-content :plex-server="plexServer" :plex-server-settings="plexServerSettings" />
						</q-tab-panel>

						<!--	Library Download Destinations	Tab Content -->
						<q-tab-panel name="download-destinations">
							<server-library-destinations-tab-content :plex-server="plexServer" :plex-libraries="plexLibraries" />
						</q-tab-panel>

						<!--	Server Commands -->
						<q-tab-panel name="server-commands">
							<server-commands-tab-content :plex-server="plexServer" :is-visible="isVisible" />
						</q-tab-panel>
					</q-tab-panels>
				</template>
			</q-splitter>

			<q-separator />

			<q-card-actions align="right">
				<q-btn v-close-popup flat :label="$t('general.commands.close')" color="primary" />
			</q-card-actions>
		</q-card>
		<q-card v-else class="server-dialog-content">
			<h1>{{ $t('components.server-dialog.no-servers-error') }}</h1>
		</q-card>
	</q-dialog>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { switchMap, tap } from 'rxjs/operators';
import Log from 'consola';
import { ref, computed } from '#imports';
import type { FolderPathDTO, PlexLibraryDTO, PlexServerDTO, PlexServerSettingsModel } from '@dto/mainApi';
import { FolderPathService, LibraryService, ServerService, SettingsService } from '@service';
import { ServerDataTabContent, ServerConnectionsTabContent } from '#components';

const show = ref(false);
const tabIndex = ref<string>('server-data');
const plexServer = ref<PlexServerDTO | null>(null);
const plexLibraries = ref<PlexLibraryDTO[]>([]);
const plexServerSettings = ref<PlexServerSettingsModel | null>(null);
const plexServerId = ref(0);
const splitterModel = ref(20);

const isVisible = computed((): boolean => plexServerId.value > 0);

const open = (newPlexServerId: number): void => {
	plexServerId.value = newPlexServerId;
	Log.info('Opening server dialog for server with id: ' + newPlexServerId);
	show.value = true;

	useSubscription(
		ServerService.getServer(newPlexServerId)
			.pipe(
				tap((plexServerData) => {
					plexServer.value = plexServerData;
				}),
				switchMap((plexServer) => SettingsService.getServerSettings(plexServer?.machineIdentifier ?? '')),
			)
			.subscribe((plexServerSettingsData) => {
				plexServerSettings.value = plexServerSettingsData;
			}),
	);
	useSubscription(
		LibraryService.getLibrariesByServerId(newPlexServerId).subscribe((plexLibrariesData) => {
			plexLibraries.value = plexLibrariesData;
		}),
	);
};

const close = (): void => {
	show.value = false;
	plexServerId.value = 0;
	tabIndex.value = 'server-data';
};

defineExpose({
	open,
	close,
});
</script>

<style lang="scss">
.server-dialog-content {
	max-width: 80vw !important;
	min-width: 70vw !important;
}
</style>
