<template>
	<template v-if="plexServers.length > 0">

		<q-expansion-item v-for="(server, i) in plexServers" :icon="server.icon"
											:label="server.name"
											expand-icon="mdi-chevron-down">
			<!-- Server header	-->
			<template v-slot:header>
				<q-item-section avatar>
					<q-status :value="isConnected(server)"/>
				</q-item-section>

				<q-item-section>
					<div class="server-name">
						{{ server.name }}
					</div>
				</q-item-section>
				<q-item-section side>
					<q-btn icon="mdi-cog" flat @click.native.stop="openServerSettings(server.id)"/>
				</q-item-section>
			</template>
			<!-- Render libraries -->
			<q-list v-if="filterLibraries(server.id).length > 0">
				<q-item v-for="(library, y) in filterLibraries(server.id)" clickable v-ripple @click="openMediaPage(library)"
								active-class="text-orange">
					<q-item-section avatar>
						<q-media-type-icon :media-type="library.type"/>
					</q-item-section>
					<q-item-section>{{ library.title }}</q-item-section>
				</q-item>
			</q-list>
			<!-- No libraries available -->
			<template v-else>
				<q-item>
					<q-item-section>{{ $t('components.server-drawer.no-libraries') }}</q-item-section>
				</q-item>
			</template>
		</q-expansion-item>

		<!--		<server-dialog ref="serverDialog"/>-->
	</template>
	<!-- With valid server available -->

	<!-- No servers available -->
	<template v-else>
		<q-item>
			<q-item-section>{{ $t('components.server-drawer.no-servers.header') }}</q-item-section>
		</q-item>
		<q-item>
			<q-item-section>{{ $t('components.server-drawer.no-servers.description') }}</q-item-section>
		</q-item>
	</template>


</template>

<script setup lang="ts">
import Log from 'consola';
import {useSubscription} from '@vueuse/rxjs';
import {LibraryService, ServerService} from '@service';
import {PlexLibraryDTO, PlexMediaType, PlexServerConnectionDTO, PlexServerDTO, SyncServerProgress} from '@dto/mainApi';
import ServerDialog from '@components/Navigation/ServerDialog.vue';
import serverConnectionService from '~/service/serverConnectionService';

const router = useRouter();


const plexServers = ref<PlexServerDTO[]>([]);
const plexLibraries = ref<PlexLibraryDTO[]>([]);
const connections = ref<PlexServerConnectionDTO[]>([]);


// @Ref('serverDialog')
// readonly serverDialogRef!: ServerDialog;


function filterLibraries(plexServerId: number): PlexLibraryDTO[] {
	return plexLibraries.value.filter((x) => x.plexServerId === plexServerId);
}

function openServerSettings(serverId: number): void {
	this.serverDialogRef.open(serverId);
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

function isConnected(server: PlexServerDTO) {
	return connections.value.filter((x) => x.plexServerId === server.id).some((x) => x.latestConnectionStatus.isSuccessful);
}

onMounted(() => {
	useSubscription(
		ServerService.getServers().subscribe((data: PlexServerDTO[]) => {
			plexServers.value = data;
		}));

	useSubscription(
		serverConnectionService.getServerConnections().subscribe((data) => {
			connections.value = data;
		}));

	useSubscription(
		LibraryService.getLibraries().subscribe((data: PlexLibraryDTO[]) => {
			plexLibraries.value = data;
		}));
});

</script>
