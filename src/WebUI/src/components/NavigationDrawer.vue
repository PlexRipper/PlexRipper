<template>
	<v-navigation-drawer app clipped permanent width="300">
		<!-- Server drawer -->
		<server-drawer />
		<!-- Menu items -->
		<template v-slot:append>
			<v-list>
				<template v-for="(item, i) in getNavItems">
					<!-- Grouped nav items -->
					<v-list-group v-if="item.children && item.children.length > 0" :key="item.title" color="">
						<template v-slot:activator>
							<v-list-item-icon>
								<v-icon>{{ item.icon }}</v-icon>
							</v-list-item-icon>
							<v-list-item-title>{{ item.title }}</v-list-item-title>
						</template>
						<v-list-item v-for="(child, j) in item.children" :key="j" link nuxt :to="child.link">
							<v-list-item-icon>
								<v-icon>{{ child.icon }}</v-icon>
							</v-list-item-icon>

							<v-list-item-content>
								<v-list-item-title>{{ child.title }}</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
					</v-list-group>
					<!-- Single nav item  -->
					<v-list-item v-else :key="i" link nuxt :to="item.link">
						<v-list-item-icon>
							<v-icon>{{ item.icon }}</v-icon>
						</v-list-item-icon>

						<v-list-item-content>
							<v-list-item-title>
								{{ item.title }}
							</v-list-item-title>
						</v-list-item-content>
						<v-list-item-action v-if="item.title === 'Downloads'">
							<v-avatar left class="red" size="36" :dark="$vuetify.theme.dark">
								<b>{{ downloadTaskCount }}</b>
							</v-avatar>
						</v-list-item-action>
					</v-list-item>
				</template>
			</v-list>
		</template>
	</v-navigation-drawer>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import DownloadService from '@service/downloadService';
import ServerDrawer from './ServerDrawer.vue';

interface INavItem {
	title: string;
	icon: string;
	link: string;
	children?: INavItem[];
}

@Component({
	components: { ServerDrawer },
})
export default class NavigationDrawer extends Vue {
	items: object[] = [];
	downloadTaskCount = 0;
	get getNavItems(): INavItem[] {
		return [
			{
				title: 'Downloads',
				icon: 'mdi-download',
				link: '/downloads',
			},
			{
				title: 'Settings',
				icon: 'mdi-cog',
				link: '/settings',
				children: [
					{
						title: 'Accounts',
						icon: 'mdi-account',
						link: '/settings/accounts',
					},
					{
						title: 'Paths',
						icon: 'mdi-folder',
						link: '/settings/paths',
					},
				],
			},
		];
	}

	created(): void {
		DownloadService.getDownloadList().subscribe((downloadTasks) => {
			this.downloadTaskCount = downloadTasks.length;
		});
	}
}
</script>
