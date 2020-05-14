<template>
	<v-expansion-panels>
		<v-expansion-panel v-for="(server, i) in getServers" :key="i">
			<v-expansion-panel-header>{{ server.name }}</v-expansion-panel-header>
			<v-expansion-panel-content>
				Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim
				veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.
			</v-expansion-panel-content>
		</v-expansion-panel>
	</v-expansion-panels>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import IPlexServer from '@dto/IPlexServer';
import { UserStore } from '../store';

interface INavItem {
	title: string;
	icon: string;
	link: string;
}

@Component
export default class ServerDrawer extends Vue {
	items: object[] = [];

	get getNavItems(): INavItem[] {
		return [
			{
				title: 'Settings',
				icon: 'mdi-settings',
				link: '/settings',
			},
		];
	}

	get getServers(): IPlexServer[] {
		return UserStore.getServers;
	}
}
</script>
