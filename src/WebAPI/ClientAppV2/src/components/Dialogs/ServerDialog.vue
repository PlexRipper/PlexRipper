<template>
	<q-card-dialog
		:name="name"
		max-width="1200px"
		height="800px"
		:scroll="false"
		:loading="loading"
		cy="server-dialog-cy"
		@opened="open"
		@closed="close">
		<template #title>
			{{
				t('components.server-dialog.header', {
					serverName: plexServer?.name ?? t('general.error.unknown'),
				})
			}}
		</template>
		<template #default>
			<q-row align="start" full-height>
				<q-col cols="auto" align-self="stretch">
					<!-- Tab Index -->
					<q-tabs v-model="tabIndex" vertical active-color="red">
						<!--	Server Data	Tab Header-->
						<q-tab
							name="server-data"
							icon="mdi-server"
							data-cy="server-dialog-tab-1"
							:label="t('components.server-dialog.tabs.server-data.header')" />
						<!--	Server Connections Tab Header	-->
						<q-tab
							name="server-connection"
							icon="mdi-connection"
							data-cy="server-dialog-tab-2"
							:label="t('components.server-dialog.tabs.server-connections.header')" />
						<!--	Server Configuration Tab Header	-->
						<q-tab
							name="server-config"
							icon="mdi-cog-box"
							data-cy="server-dialog-tab-3"
							:label="t('components.server-dialog.tabs.server-config.header')" />
						<!--	Library Destinations Tab Header	-->
						<q-tab
							name="download-destinations"
							icon="mdi-folder-edit-outline"
							data-cy="server-dialog-tab-4"
							:label="t('components.server-dialog.tabs.download-destinations.header')" />
						<!--	Server Commands Tab Header	-->
						<q-tab
							name="server-commands"
							icon="mdi-console"
							data-cy="server-dialog-tab-5"
							:label="t('components.server-dialog.tabs.server-commands.header')" />
					</q-tabs>
				</q-col>
				<q-col align-self="stretch" class="inherit-all-height scroll">
					<!-- Tab Content -->
					<q-tab-panels v-model="tabIndex" animated vertical transition-prev="slide-down" transition-next="slide-up">
						<!-- Server Data Tab Content -->
						<q-tab-panel name="server-data" data-cy="server-dialog-tab-content-1">
							<ServerDataTabContent :plex-server="plexServer" :is-visible="isVisible" />
						</q-tab-panel>

						<!-- Server Connections Tab Content	-->
						<q-tab-panel name="server-connection" data-cy="server-dialog-tab-content-2">
							<ServerConnectionsTabContent :plex-server="plexServer" :is-visible="isVisible" />
						</q-tab-panel>

						<!--	Server Configuration Tab Content	-->
						<q-tab-panel name="server-config" data-cy="server-dialog-tab-content-3">
							<server-config-tab-content :plex-server="plexServer" :plex-server-settings="plexServerSettings" />
						</q-tab-panel>

						<!--	Library Download Destinations	Tab Content -->
						<q-tab-panel name="download-destinations" class="scroll" data-cy="server-dialog-tab-content-4">
							<server-library-destinations-tab-content :plex-server="plexServer" :plex-libraries="plexLibraries" />
						</q-tab-panel>

						<!--	Server Commands -->
						<q-tab-panel name="server-commands" data-cy="server-dialog-tab-content-5">
							<server-commands-tab-content :plex-server="plexServer" :is-visible="isVisible" />
						</q-tab-panel>
					</q-tab-panels>
				</q-col>
			</q-row>

			<!--			<q-card v-else class="server-dialog-content">-->
			<!--				<h1>{{ t('components.server-dialog.no-servers-error') }}</h1>-->
			<!--			</q-card>-->
		</template>
		<template #actions>
			<BaseButton
				cy="server-dialog-close-btn"
				flat
				:label="t('general.commands.close')"
				color="default"
				@click="useCloseControlDialog(name)" />
		</template>
	</q-card-dialog>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { switchMap, take, tap } from 'rxjs/operators';
import { set } from '@vueuse/core';
import { ref, computed, useCloseControlDialog } from '#imports';
import type { PlexLibraryDTO, PlexServerDTO, PlexServerSettingsModel } from '@dto/mainApi';
import { LibraryService, ServerService, SettingsService } from '@service';
import { ServerDataTabContent, ServerConnectionsTabContent } from '#components';

defineProps<{ name: string }>();

const loading = ref(false);
const tabIndex = ref<string>('server-data');
const plexServer = ref<PlexServerDTO | null>(null);
const plexLibraries = ref<PlexLibraryDTO[]>([]);
const plexServerSettings = ref<PlexServerSettingsModel | null>(null);
const plexServerId = ref(0);

const isVisible = computed((): boolean => plexServerId.value > 0);
const { t } = useI18n();

function open(newPlexServerId: number): void {
	set(plexServerId, newPlexServerId);
	set(loading, true);

	useSubscription(
		ServerService.getServer(newPlexServerId)
			.pipe(
				tap((plexServerData) => {
					set(plexServer, plexServerData);
				}),
				switchMap((plexServer) => SettingsService.getServerSettings(plexServer?.machineIdentifier ?? '')),
				take(1),
			)
			.subscribe({
				next: (plexServerSettingsData) => {
					set(plexServerSettings, plexServerSettingsData);
				},
				complete: () => {
					set(loading, false);
				},
			}),
	);
	useSubscription(
		LibraryService.getLibrariesByServerId(newPlexServerId).subscribe((plexLibrariesData) => {
			set(plexLibraries, plexLibrariesData);
		}),
	);
}

function close(): void {
	set(plexServerId, 0);
	set(tabIndex, 'server-data');
}
</script>
