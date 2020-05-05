<template>
	<v-navigation-drawer v-model="drawer" app clipped>
		<v-treeview dense />
		<!-- <v-list dense>
			<v-list-item v-for="item in items" :key="item.text" link>
				<v-list-item-action>
					<v-icon>{{ item.icon }}</v-icon>
				</v-list-item-action>
				<v-list-item-content>
					<v-list-item-title>
						{{ item.text }}
					</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-subheader class="mt-4 grey--text text--darken-1">SUBSCRIPTIONS</v-subheader>
			<v-list>
				<v-list-item v-for="item in items2" :key="item.text" link>
					<v-list-item-avatar>
						<img :src="`https://randomuser.me/api/portraits/men/${item.picture}.jpg`" alt="" />
					</v-list-item-avatar>
					<v-list-item-title v-text="item.text" />
				</v-list-item>
			</v-list>
			<v-list-item class="mt-4" link>
				<v-list-item-action>
					<v-icon color="grey darken-1">mdi-plus-circle-outline</v-icon>
				</v-list-item-action>
				<v-list-item-title class="grey--text text--darken-1">Browse Channels</v-list-item-title>
			</v-list-item>
			<v-list-item link>
				<v-list-item-action>
					<v-icon color="grey darken-1">mdi-settings</v-icon>
				</v-list-item-action>
				<v-list-item-title class="grey--text text--darken-1">Manage Subscriptions</v-list-item-title>
			</v-list-item>
		</v-list> -->
	</v-navigation-drawer>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Log from 'consola';

@Component
export default class NavigationDrawer extends Vue {
	items: object[] = [];

	async mounted(): Promise<void> {
		const ip = await this.$axios.$get('https://localhost:5001/api/weatherforecast');
		Log.debug(ip);
	}
}
</script>
