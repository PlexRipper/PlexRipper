<template>
	<v-app-bar color="red" dense app clipped-left>
		<v-toolbar-title>
			<v-btn to="/" text> Plex Ripper </v-btn>
		</v-toolbar-title>

		<v-spacer></v-spacer>

		<v-btn icon href="https://github.com/PlexRipper/PlexRipper" target="_blank">
			<v-icon>mdi-github</v-icon>
		</v-btn>

		<v-btn icon @click="setDarkMode">
			<v-icon v-if="$vuetify.theme.dark">mdi-white-balance-sunny</v-icon>
			<v-icon v-else>mdi-moon-waxing-crescent</v-icon>
		</v-btn>

		<v-menu left bottom offset-y>
			<template v-slot:activator="{ on }">
				<v-btn icon v-on="on">
					<v-icon>mdi-account</v-icon>
				</v-btn>
			</template>

			<v-list>
				<template v-if="accounts.length > 0">
					<v-list-item v-for="(account, index) in accounts" :key="index" @click="setActiveAccount(account)">
						<v-list-item-title> {{ account.displayName }}</v-list-item-title>
					</v-list-item>
				</template>
				<v-list-item v-else>
					<v-list-item-title> No Accounts available</v-list-item-title>
				</v-list-item>
			</v-list>
		</v-menu>
	</v-app-bar>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import type { PlexAccountDTO } from '@dto/mainApi';
import AccountService from '@service/accountService';

@Component
export default class AppBar extends Vue {
	private accounts: PlexAccountDTO[] = [];

	setDarkMode(): void {
		this.$vuetify.theme.dark = !this.$vuetify.theme.dark;
	}

	setActiveAccount(account: PlexAccountDTO): void {
		AccountService.setActiveAccount(account.id);
	}

	created(): void {
		AccountService.getAccounts().subscribe((data) => {
			this.accounts = data?.filter((x) => x.isEnabled) ?? [];
		});
	}
}
</script>
