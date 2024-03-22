<template>
	<template v-if="serverStore.getServers().length > 0">
		<q-expansion-item
			v-for="(server, index) in serverStore.getServers()"
			:key="index"
			:label="serverStore.getServerName(server.id)"
			expand-icon="mdi-chevron-down">
			<!-- Server header	-->
			<template #header>
				<q-item-section side no-wrap>
					<q-status :value="serverConnectionStore.isServerConnected(server.id)" />
				</q-item-section>

				<q-item-section>
					<div class="server-name">
						<q-icon v-if="server.owned" name="mdi-home" size="24px" left />
						{{ serverStore.getServerName(server.id) }}
					</div>
				</q-item-section>
				<q-item-section side>
					<q-btn icon="mdi-cog" flat :data-cy="`server-dialog-${index}`" @click.stop="openServerSettings(server.id)" />
				</q-item-section>
			</template>
			<!-- Render libraries -->
			<q-list v-if="filterLibraries(server.id).length > 0">
				<q-item
					v-for="(library, y) in filterLibraries(server.id)"
					:key="y"
					v-ripple
					clickable
					active-class="text-orange"
					@click="openMediaPage(library)">
					<q-item-section avatar>
						<q-media-type-icon :media-type="library.type" />
					</q-item-section>
					<q-item-section>{{ libraryStore.getLibraryName(library.id) }}</q-item-section>
				</q-item>
			</q-list>
			<!-- No libraries available -->
			<template v-else>
				<q-item>
					<q-item-section>{{ t('components.server-drawer.no-libraries') }}</q-item-section>
				</q-item>
			</template>
		</q-expansion-item>

		<ServerDialog :name="serverDialogName" />
	</template>
	<!-- With valid server available -->

	<!-- No servers available -->
	<template v-else>
		<q-item>
			<q-item-section>{{ t('components.server-drawer.no-servers.header') }}</q-item-section>
		</q-item>
		<q-item>
			<q-item-section>{{ t('components.server-drawer.no-servers.description') }}</q-item-section>
		</q-item>
	</template>
</template>

<script setup lang="ts">
import Log from 'consola';
import type ServerDialog from '@components/Navigation/ServerDialog.vue';
import { type PlexLibraryDTO, PlexMediaType } from '@dto/mainApi';
import { useOpenControlDialog } from '#imports';

const { t } = useI18n();
const router = useRouter();
const serverStore = useServerStore();
const libraryStore = useLibraryStore();
const serverConnectionStore = useServerConnectionStore();
const serverDialogName = 'serverDialog';

function filterLibraries(plexServerId: number): PlexLibraryDTO[] {
	return libraryStore.getLibraries.filter((x) => x.plexServerId === plexServerId);
}

function openServerSettings(serverId: number): void {
	useOpenControlDialog(serverDialogName, serverId);
}

function openMediaPage(library: PlexLibraryDTO): void {
	switch (library.type) {
		case PlexMediaType.Movie:
			router.push(`/movies/${library.id}`);
			break;
		case PlexMediaType.TvShow:
			router.push(`/tvshows/${library.id}`);
			break;
		case PlexMediaType.Music:
			router.push(`/music/${library.id}`);
			break;
		default:
			Log.error(library.type + ' was neither a movie, tvshow or music library');
	}
}
</script>
<style lang="scss">
.server-name {
	width: 190px;
	display: flex;
	line-height: 24px;
	align-content: center;
	text-overflow: ellipsis;
}

.server-panels {
	z-index: 0;

	&.theme--dark {
		.v-expansion-panel {
			background: rgba(0, 0, 0, 0.3);
		}
	}

	&.theme--light {
		.v-expansion-panel {
			background: rgba(255, 255, 255, 0.3);
		}
	}
}

.ps {
	height: 100%;
	width: 100%;
}
</style>
