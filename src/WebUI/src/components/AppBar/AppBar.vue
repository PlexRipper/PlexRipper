<template>
	<v-app-bar class="app-bar" dense app clipped-left>
		<v-toolbar-title>
			<v-btn to="/" outlined nuxt><logo :size="24" class="mr-3" /> PlexRipper </v-btn>
		</v-toolbar-title>

		<v-spacer></v-spacer>

		<v-btn icon href="https://github.com/PlexRipper/PlexRipper" target="_blank">
			<v-icon>mdi-github</v-icon>
		</v-btn>

		<!-- DarkMode toggle -->
		<dark-mode-toggle />

		<!-- Account Selector -->
		<v-menu left bottom offset-y>
			<template #activator="{ on }">
				<v-btn icon v-on="on">
					<v-icon>mdi-account</v-icon>
				</v-btn>
			</template>

			<v-list>
				<v-list-item-group v-if="accounts.length > 0" v-model="activeAccount">
					<!--	All accounts buttons	-->
					<v-list-item @click="setActiveAccount(0)">
						<v-list-item-title> All accounts </v-list-item-title>
					</v-list-item>
					<!--	Button per account	-->
					<v-list-item v-for="(account, index) in accounts" :key="index" @click="setActiveAccount(account.id)">
						<v-list-item-title> {{ account.displayName }}</v-list-item-title>
					</v-list-item>
				</v-list-item-group>
				<!--	No account found -->
				<v-list-item v-else>
					<v-list-item-title> No Accounts available</v-list-item-title>
				</v-list-item>
			</v-list>
		</v-menu>

		<!-- Notifications Selector -->
		<notification-button />
	</v-app-bar>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import type { PlexAccountDTO } from '@dto/mainApi';
import AccountService from '@service/accountService';
import NotificationButton from '@components/AppBar/NotificationButton.vue';
import DarkModeToggle from '@components/General/DarkModeToggle.vue';
import { settingsStore } from '~/store';

@Component({
	components: {
		NotificationButton,
		DarkModeToggle,
	},
})
export default class AppBar extends Vue {
	private accounts: PlexAccountDTO[] = [];

	get activeAccount(): number {
		return settingsStore.activeAccount;
	}

	set activeAccount(value: number) {
		settingsStore.setActiveAccount(value);
	}

	setActiveAccount(accountId: number): void {
		this.activeAccount = accountId;
	}

	created(): void {
		AccountService.getAccounts().subscribe((data) => {
			this.accounts = data?.filter((x) => x.isEnabled) ?? [];
		});
	}
}
</script>
