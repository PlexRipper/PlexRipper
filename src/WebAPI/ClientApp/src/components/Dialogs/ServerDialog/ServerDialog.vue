<template>
	<QCardDialog
		:name="name"
		max-width="1200px"
		content-height="80"
		:scroll="false"
		:loading="loading"
		button-align="right"
		cy="server-dialog-cy"
		@opened="open"
		@closed="close"
	>
		<template #title>
			<EditableText
				size="h5"
				bold="medium"
				:display-text="
					t('components.server-dialog.header', {
						serverName: serverStore.getServerName(plexServer?.id ?? 0) ?? t('general.error.unknown'),
					})
				"
				:value="serverStore.getServerName(plexServer?.id ?? 0)"
				@save="onServerAliasSave"
			/>
		</template>
		<template #default>
			<QRow
				align="start"
				full-height
			>
				<QCol
					cols="auto"
					align-self="stretch"
				>
					<!-- Tab Index -->
					<q-tabs
						v-model="tabIndex"
						vertical
						active-color="red"
					>
						<!--	Server Data	Tab Header -->
						<q-tab
							name="server-data"
							icon="mdi-server"
							data-cy="server-dialog-tab-1"
							:label="t('components.server-dialog.tabs.server-data.header')"
						/>
						<!--	Server Connections Tab Header	-->
						<q-tab
							name="server-connection"
							icon="mdi-connection"
							data-cy="server-dialog-tab-2"
							:label="t('components.server-dialog.tabs.server-connections.header')"
						/>
						<!--	Server Configuration Tab Header	-->
						<q-tab
							name="server-config"
							icon="mdi-cog-box"
							data-cy="server-dialog-tab-3"
							:label="t('components.server-dialog.tabs.server-config.header')"
						/>
						<!--	Library Destinations Tab Header	-->
						<q-tab
							name="download-destinations"
							icon="mdi-folder-edit-outline"
							data-cy="server-dialog-tab-4"
							:label="t('components.server-dialog.tabs.download-destinations.header')"
						/>
						<!--	Server Commands Tab Header	-->
						<q-tab
							name="server-commands"
							icon="mdi-console"
							data-cy="server-dialog-tab-5"
							:label="t('components.server-dialog.tabs.server-commands.header')"
						/>
					</q-tabs>
				</QCol>
				<QCol
					align-self="stretch"
					class="tab-content inherit-all-height scroll"
				>
					<!-- Tab Content -->
					<q-tab-panels
						v-model="tabIndex"
						animated
						vertical
						transition-prev="slide-down"
						transition-next="slide-up"
					>
						<!-- Server Data Tab Content -->
						<q-tab-panel
							name="server-data"
							data-cy="server-dialog-tab-content-1"
						>
							<ServerDataTabContent
								:plex-server="plexServer"
								:is-visible="isVisible"
							/>
						</q-tab-panel>

						<!-- Server Connections Tab Content	-->
						<q-tab-panel
							name="server-connection"
							data-cy="server-dialog-tab-content-2"
						>
							<ServerConnectionsTabContent
								:plex-server-id="plexServerId"
								:is-visible="isVisible"
							/>
						</q-tab-panel>

						<!--	Server Configuration Tab Content	-->
						<q-tab-panel
							name="server-config"
							data-cy="server-dialog-tab-content-3"
						>
							<ServerConfigTabContent :plex-server="plexServer" />
						</q-tab-panel>

						<!--	Library Download Destinations	Tab Content -->
						<q-tab-panel
							name="download-destinations"
							class="inherit-all-height"
							data-cy="server-dialog-tab-content-4"
						>
							<ServerLibraryDestinationsTabContent
								:plex-server="plexServer"
								:plex-libraries="libraryStore.getLibrariesByServerId(plexServerId)"
							/>
						</q-tab-panel>

						<!--	Server Commands -->
						<q-tab-panel
							name="server-commands"
							data-cy="server-dialog-tab-content-5"
						>
							<ServerCommandsTabContent
								:plex-server="plexServer"
								:is-visible="isVisible"
							/>
						</q-tab-panel>
					</q-tab-panels>
				</QCol>
			</QRow>

			<!--			<q-card v-else class="server-dialog-content"> -->
			<!--				<h1>{{ t('components.server-dialog.no-servers-error') }}</h1> -->
			<!--			</q-card> -->
		</template>
		<template #actions>
			<QRow justify="between">
				<QCol cols="auto">
					<HideButton @click="useOpenControlDialog(confirmationServerDialogName)" />
				</QCol>
				<QCol cols="auto">
					<BaseButton
						cy="server-dialog-close-btn"
						flat
						:label="t('general.commands.close')"
						color="default"
						@click="close"
					/>
				</QCol>
			</QRow>
			<!-- Hide Server Confirm Dialog -->
			<ConfirmationDialog
				:confirm-loading="confirmHideDialog"
				:name="confirmationServerDialogName"
				class="q-mr-md"
				text-id="hide-server"
				@confirm="onServerHiddenSave"
			/>
		</template>
	</QCardDialog>
</template>

<script setup lang="ts">
import { set, get } from '@vueuse/core';
import { tap } from 'rxjs/operators';
import type { PlexServerDTO } from '@dto';
import { ref, computed, useCloseControlDialog, useOpenControlDialog } from '#imports';

const props = defineProps<{ name: string }>();

const serverStore = useServerStore();
const libraryStore = useLibraryStore();

const confirmationServerDialogName = 'confirmationServerDialogName';
const confirmHideDialog = ref(false);

const loading = ref(false);
const tabIndex = ref<string>('server-data');
const plexServer = ref<PlexServerDTO | null>(null);
const plexServerId = ref<number>(0);

const isVisible = computed((): boolean => plexServerId.value > 0);
const { t } = useI18n();

function open(event: unknown): void {
	const newPlexServerId = event as number;
	set(plexServerId, newPlexServerId);
	set(loading, true);

	set(plexServer, serverStore.getServer(newPlexServerId));
	set(loading, false);
}

function close(): void {
	useCloseControlDialog(props.name);
	set(plexServerId, 0);
	set(tabIndex, 'server-data');
}

function onServerAliasSave(serverAlias: string): void {
	useSubscription(serverStore.setServerAlias(get(plexServerId), serverAlias).subscribe());
}

function onServerHiddenSave(): void {
	useSubscription(
		serverStore
			.setServerHidden(get(plexServerId), true)
			.pipe(tap(() => close()))
			.subscribe(),
	);
}
</script>

<style lang="scss">
@import '@/assets/scss/variables.scss';
@import 'quasar/src/css/core/typography.sass';

.tab-content {
	max-height: calc(80vh - $q-card-dialog-title-height - $q-card-dialog-actions-height) !important;
}

.editable-text {
	&-item {
		padding-top: 0;
		padding-bottom: 0;
	}
}
</style>
