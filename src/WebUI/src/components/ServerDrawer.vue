<template>
	<v-expansion-panels>
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
	</v-expansion-panels>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import IPlexServer from '@dto/IPlexServer';
import IPlexLibrary from '@dto/IPlexLibrary';
import SettingsService from '@service/settingsService';

interface INavItem {
	title: string;
	icon: string;
	link: string;
}

@Component
export default class ServerDrawer extends Vue {
	items: object[] = [];
	plexServers: IPlexServer[] = [];

	get getNavItems(): INavItem[] {
		return [
			{
				title: 'Settings',
				icon: 'mdi-settings',
				link: '/settings',
			},
		];
	}

	findIcon(type: string): string {
		switch (type) {
			case 'show':
				return 'mdi-television-classic';
			case 'movie':
				return 'mdi-filmstrip';
			case 'artist':
				return 'mdi-music';
			default:
				return 'mdi-help-circle-outline';
		}
	}

	openMediaPage(library: IPlexLibrary): void {
		Log.debug(library);
		switch (library.type) {
			case 'movie':
				this.$router.push(`/movies/${library.id}`);
				break;
			case 'show':
				this.$router.push(`/tvshows/${library.id}`);
				break;
			default:
				Log.error('Library was neither a movie or tvshows');
		}
	}

	created(): void {
		SettingsService.getActiveAccount().subscribe((data) => {
			Log.debug(`ServerDrawer => ${data}`);
			this.plexServers = data?.plexServers ?? [];
		});
	}
}
</script>
