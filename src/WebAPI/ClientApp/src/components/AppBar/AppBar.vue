<template>
	<v-app-bar class="app-bar" dense app clipped-left clipped-right>
		<v-toolbar-title>
			<v-app-bar-nav-icon @click.stop="showNavigationDrawer" />
			<v-btn to="/" outlined nuxt><logo :size="24" class="mr-3" /> {{ $t('general.name-version', { version }) }}</v-btn>
		</v-toolbar-title>

		<v-spacer></v-spacer>
		<app-bar-progress-bar />
		<v-spacer></v-spacer>

		<v-btn icon href="https://github.com/PlexRipper/PlexRipper" target="_blank">
			<v-icon>mdi-github</v-icon>
		</v-btn>

		<!-- DarkMode toggle -->
		<dark-mode-toggle />

		<!-- Account Selector -->
		<v-menu left bottom offset-y :close-on-content-click="false">
			<template #activator="{ on }">
				<v-btn icon v-on="on">
					<v-icon>mdi-account</v-icon>
				</v-btn>
			</template>
			<v-list>
				<v-list-item-group v-if="accounts.length > 0" :value="activeAccountId">
					<!--	Button per account	-->
					<v-list-item v-for="(account, index) in accounts" :key="index" @click="updateActiveAccountId(account.id)">
						<v-list-item-content>
							<v-list-item-title> {{ account.displayName }}</v-list-item-title>
						</v-list-item-content>
						<v-list-item-action>
							<v-btn
								icon
								:loading="loading[0] || loading[index]"
								:disabled="isLoading"
								@click.native.stop="refreshAccount(account.id)"
							>
								<v-icon color="grey lighten-1">mdi-refresh</v-icon>
							</v-btn>
						</v-list-item-action>
					</v-list-item>
				</v-list-item-group>
				<!--	No account found -->
				<v-list-item v-else>
					<v-list-item-title> {{ $t('components.app-bar.no-accounts') }}</v-list-item-title>
				</v-list-item>
			</v-list>
		</v-menu>

		<!-- Notifications Selector -->
		<notification-button @toggle="showNotificationsDrawer" />
	</v-app-bar>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { GlobalService, SettingsService, AccountService, ServerService } from '@service';
import { refreshAccount } from '@api/accountApi';
import type { PlexAccountDTO } from '@dto/mainApi';

@Component
export default class AppBar extends Vue {
	private accounts: PlexAccountDTO[] = [];
	private loading: boolean[] = [false];
	private version: string = '?';

	activeAccountId: number = 0;

	get isLoading(): boolean {
		return this.loading.some((x) => x);
	}

	showNavigationDrawer(): void {
		this.$emit('show-navigation');
	}

	showNotificationsDrawer(): void {
		this.$emit('show-notifications');
	}

	updateActiveAccountId(accountId: number): void {
		SettingsService.updateSetting('activeAccountId', accountId);
	}

	refreshAccount(accountId: number = 0): void {
		const index = accountId === 0 ? 0 : this.accounts.findIndex((x) => x.id === accountId);
		this.loading.splice(index, 1, true);
		refreshAccount(accountId).subscribe(() => {
			AccountService.fetchAccounts();
			ServerService.fetchServers();
			this.loading.splice(index, 1, false);
		});
	}

	mounted(): void {
		this.$subscribeTo(GlobalService.getConfigReady(), (config) => {
			this.version = config.version;
		});

		this.$subscribeTo(AccountService.getAccounts(), (data) => {
			this.accounts = [
				{
					id: 0,
					displayName: 'All Accounts',
				} as any,
			];
			data?.filter((x) => x.isEnabled).forEach((account) => this.accounts.push(account));
			this.accounts.forEach(() => this.loading.push(false));
		});

		this.$subscribeTo(SettingsService.getActiveAccountId(), (activeAccountId) => {
			if (activeAccountId || activeAccountId >= 0) {
				this.activeAccountId = activeAccountId;
			}
		});

		// this.$subscribeTo(SignalrService.getPlexAccountRefreshProgress(), (data) => {
		// 	if (data) {
		// 		const index = this.accountRefreshProgress.findIndex((x) => x.plexAccountId === data.plexAccountId);
		// 		if (index > -1) {
		// 			if (!data.isComplete) {
		// 				this.accountRefreshProgress.splice(index, 1, data);
		// 			} else {
		// 				this.accountRefreshProgress.splice(index, 1);
		// 			}
		// 		} else {
		// 			this.accountRefreshProgress.push(data);
		// 		}
		// 	}
		// });
	}
}
</script>
