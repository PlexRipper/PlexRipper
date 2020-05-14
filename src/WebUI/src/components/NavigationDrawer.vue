<template>
	<v-navigation-drawer app clipped>
		<v-layout column fill-height>
			<server-drawer />
			<v-spacer />
			<v-list dense nav>
				<v-list-item v-for="item in getNavItems" :key="item.title" link nuxt :to="item.link">
					<v-list-item-icon>
						<v-icon>{{ item.icon }}</v-icon>
					</v-list-item-icon>

					<v-list-item-content>
						<v-list-item-title>{{ item.title }}</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</v-list>
		</v-layout>
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
				title: 'Settings',
				icon: 'mdi-settings',
				link: '/settings',
			},
		];
	}

	// async mounted(): Promise<void> {
	// 	try {
	// 		const ip = await this.$axios.$get('/weatherforecast');
	// 		Log.debug(ip);
	// 	} catch (error) {
	// 		Log.error(error);
	// 	}
	// }
}
</script>
