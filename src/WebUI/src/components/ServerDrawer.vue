<template>
	<v-expansion-panels>
		<!-- With valid server available -->
		<template v-if="plexServers.length > 0">
			<v-expansion-panel v-for="(server, i) in plexServers" :key="i">
				<v-expansion-panel-header>{{ server.name }}</v-expansion-panel-header>
				<v-expansion-panel-content>
					<v-list nav dense>
						<v-list-item-group color="primary">
							<!-- Render libraries -->
							<template v-if="server.plexLibraries.length > 0">
								<v-list-item v-for="(library, y) in server.plexLibraries" :key="y" @click="openMediaPage(library)">
									<v-list-item-icon>
										<v-icon>{{ findIcon(library.type) }}</v-icon>
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
										<v-icon>{{ findIcon('') }}</v-icon>
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
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import SettingsService from '@service/settingsService';
import { PlexAccountDTO, PlexLibraryDTO, PlexMediaType, PlexServerDTO } from '@dto/mainApi';

interface INavItem {
	title: string;
	icon: string;
	link: string;
}

@Component
export default class ServerDrawer extends Vue {
	items: object[] = [];
	plexServers: PlexServerDTO[] = [];
	activeAccount!: PlexAccountDTO | null;

	get getNavItems(): INavItem[] {
		return [
			{
				title: 'Settings',
				icon: 'mdi-settings',
				link: '/settings',
			},
		];
	}

	findIcon(type: PlexMediaType): string {
		switch (type) {
			case PlexMediaType.TvShow:
				return 'mdi-television-classic';
			case PlexMediaType.Movie:
				return 'mdi-filmstrip';
			case PlexMediaType.Music:
				return 'mdi-music';
			default:
				return 'mdi-help-circle-outline';
		}
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
				Log.error('Library was neither a movie or tvshows');
		}
	}

	created(): void {
		SettingsService.getActiveAccount().subscribe((data) => {
			Log.debug(`ServerDrawer => ${data}`);
			this.activeAccount = data;
			this.plexServers = data?.plexServers ?? [];
		});
	}
}
</script>
