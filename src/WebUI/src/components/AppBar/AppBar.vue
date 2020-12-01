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
		<v-menu left bottom offset-y :close-on-content-click="false">
			<template #activator="{ on }">
				<v-btn icon v-on="on">
					<v-icon>mdi-account</v-icon>
				</v-btn>
			</template>
			<v-list>
				<v-list-item-group v-if="accounts.length > 0" v-model="activeAccount">
					<!--	Button per account	-->
					<v-list-item v-for="(account, index) in accounts" :key="index" @click="setActiveAccount(account.id)">
						<v-list-item-content>
							<v-list-item-title> {{ account.displayName }}</v-list-item-title>
							<progress-component
								v-if="getRefreshProgress(account.id)"
								:show-circular="false"
								:percentage="getRefreshProgress(account.id).percentage"
							/>
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
import { refreshAccount } from '@api/accountApi';
import { switchMap } from 'rxjs/operators';
import { of } from 'rxjs';
import SignalrService from '@service/signalrService';
import ProgressComponent from '@components/ProgressComponent.vue';
import { PlexAccountRefreshProgress } from '@dto/mainApi';
import { settingsStore } from '~/store';
@Component({
	components: {
		NotificationButton,
		DarkModeToggle,
		ProgressComponent,
	},
})
export default class AppBar extends Vue {
	private accounts: PlexAccountDTO[] = [];
	private loading: boolean[] = [false];

	private accountRefreshProgress: PlexAccountRefreshProgress[] = [];
	get activeAccount(): number {
		return settingsStore.activeAccount;
	}

	set activeAccount(value: number) {
		settingsStore.setActiveAccount(value);
	}

	get isLoading(): boolean {
		this.loading.some((x) => x);
	}

	setActiveAccount(accountId: number): void {
		this.activeAccount = accountId;
	}

	getRefreshProgress(plexAccountId: number): PlexAccountRefreshProgress {
		return this.accountRefreshProgress.find((x) => x.plexAccountId === plexAccountId);
	}

	refreshAccount(accountId: number = 0): void {
		const index = accountId === 0 ? 0 : this.accounts.findIndex((x) => x.id === accountId);
		this.loading.splice(index, 1, true);
		refreshAccount(accountId)
			.pipe(switchMap(() => of(AccountService.fetchAccounts())))
			.subscribe(() => {
				this.loading.splice(index, 1, false);
			});
	}

	created(): void {
		AccountService.getAccounts().subscribe((data) => {
			this.accounts = [{ id: 0, displayName: 'All Accounts' }];
			data?.filter((x) => x.isEnabled).forEach((account) => this.accounts.push(account));
			this.accounts.forEach(() => this.loading.push(false));
		});

		SignalrService.getPlexAccountRefreshProgress().subscribe((data) => {
			const index = this.accountRefreshProgress.findIndex((x) => x.plexAccountId === data.plexAccountId);
			if (index > -1) {
				if (!data.isComplete) {
					this.accountRefreshProgress.splice(index, 1, data);
				} else {
					this.accountRefreshProgress.splice(index, 1);
				}
			} else {
				this.accountRefreshProgress.push(data);
			}
		});
	}
}
</script>
