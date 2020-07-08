<template>
	<v-navigation-drawer app clipped permanent width="300">
		<!-- Server drawer -->
		<server-drawer />
		<!-- Menu items -->
		<template v-slot:append>
			<v-list>
				<v-list-item v-for="item in getNavItems" :key="item.title" link nuxt :to="item.link">
					<v-list-item-icon>
						<v-icon>{{ item.icon }}</v-icon>
					</v-list-item-icon>

					<v-list-item-content>
						<v-list-item-title>{{ item.title }}</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</v-list>
		</template>
	</v-navigation-drawer>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import ServerDrawer from './ServerDrawer.vue';

interface INavItem {
	title: string;
	icon: string;
	link: string;
}

@Component({
	components: { ServerDrawer },
})
export default class NavigationDrawer extends Vue {
	items: object[] = [];

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
			},
		];
	}
}
</script>
