<template>
	<perfect-scrollbar>
		<v-expansion-panels class="server-panels">
			<!-- With valid server available -->
			<template v-if="plexServers.length > 0">
				<v-expansion-panel v-for="(server, i) in plexServers" :key="i">
					<v-expansion-panel-header>
						<v-row align="center" no-gutters>
							<v-col cols="auto">
								<div class="server-name"><status :value="server.status.isSuccessful" /> {{ server.name }}</div>
							</v-col>
							<v-spacer></v-spacer>
							<v-col cols="auto">
								<v-btn icon @click.native.stop="openServerSettings(server.id)">
									<v-icon>mdi-cog</v-icon>
								</v-btn>
							</v-col>
						</v-row>
					</v-expansion-panel-header>
					<v-expansion-panel-content>
						<v-list nav dense>
							<v-list-item-group color="primary">
								<!-- Render libraries -->
								<template v-if="server.plexLibraries.length > 0">
									<v-list-item v-for="(library, y) in server.plexLibraries" :key="y" @click="openMediaPage(library)">
										<v-list-item-icon>
											<v-icon>{{ library.type | mediaTypeIcon }}</v-icon>
										</v-list-item-icon>
										<v-list-item-content>
											<v-list-item-title v-text="library.title"></v-list-item-title>
										</v-list-item-content>
									</v-list-item>
								</template>
								<!-- No libraries available -->
								<template v-else>
									<v-list-item>
										<v-list-item-icon>
											<v-icon>{{ '' | mediaTypeIcon }}</v-icon>
										</v-list-item-icon>
										<v-list-item-content>
											<v-list-item-title>No libraries available</v-list-item-title>
										</v-list-item-content>
									</v-list-item>
								</template>
							</v-list-item-group>
						</v-list>
					</v-expansion-panel-content>
				</v-expansion-panel>
			</template>
			<!-- No servers available -->
			<template v-else>
				<v-expansion-panel>
					<v-expansion-panel-header>There are no servers available!</v-expansion-panel-header>
					<v-expansion-panel-content>
						Make sure that you have a Plex account registered in the settings and have selected an account with accessible plex
						servers in the top right account selector.
					</v-expansion-panel-content>
				</v-expansion-panel>
			</template>
		</v-expansion-panels>
		<server-dialog :server-id="selectedServerId" @close="closeDialog" />
	</perfect-scrollbar>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import ServerService from '@state/serverService';
import { PlexLibraryDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';
import ServerDialog from '@components/Navigation/ServerDialog.vue';

interface INavItem {
	title: string;
	icon: string;
	link: string;
}

@Component({
	components: {
		ServerDialog,
	},
})
export default class ServerDrawer extends Vue {
	items: object[] = [];
	plexServers: PlexServerDTO[] = [];
	selectedServerId: number = 0;

	get getNavItems(): INavItem[] {
		return [
			{
				title: 'Settings',
				icon: 'mdi-settings',
				link: '/settings',
			},
		];
	}

	openServerSettings(serverId: number): void {
		this.selectedServerId = serverId;
	}

	openMediaPage(library: PlexLibraryDTO): void {
		Log.debug(library);
		switch (library.type) {
			case PlexMediaType.Movie:
				this.$router.push(`/movies/${library.id}`);
				break;
			case PlexMediaType.TvShow:
				this.$router.push(`/tvshows/${library.id}`);
				break;
			default:
				Log.error(library.type + ' was neither a movie or tvshows');
		}
	}

	closeDialog(): void {
		this.selectedServerId = 0;
	}

	created(): void {
		ServerService.getServers().subscribe((data: PlexServerDTO[]) => {
			this.plexServers = data;
		});
	}
}
</script>
