<template>
	<v-container>
		<v-row justify="center">
			<!-- Plex Accounts -->
			<v-col v-for="(account, index) in accounts" :key="index" cols="4" style="min-width: 395px">
				<account-card :account="account" @open-dialog="openDialog(account)" @dialog-closed="dialog = false" />
			</v-col>
			<!-- Add new Account card -->
			<v-col cols="4" style="min-width: 395px">
				<account-card @open-dialog="openDialog" />
			</v-col>
		</v-row>
		<v-row>
			<v-col>
				<account-dialog :dialog="dialog" :account="selectedAccount" @dialog-closed="closeDialog" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import AccountService from '@service/accountService';
import SignalrService from '@service/signalrService';
import { PlexAccountDTO } from '@dto/mainApi';
import AccountDialog from '@overviews/AccountOverview/AccountDialog.vue';
import AccountCard from './AccountCard.vue';

@Component({
	components: {
		AccountCard,
		AccountDialog,
	},
})
export default class AccountOverview extends Vue {
	private accounts: PlexAccountDTO[] = [];
	private dialog: boolean = false;
	private selectedAccount: PlexAccountDTO | null = null;

	openDialog(account: PlexAccountDTO | null = null): void {
		this.selectedAccount = account;
		this.dialog = true;
	}

	closeDialog(refreshAccounts: boolean = false): void {
		this.dialog = false;
		if (refreshAccounts) {
			AccountService.fetchAccounts();
		}
	}

	created(): void {
		AccountService.getAccounts().subscribe((data) => {
			this.accounts = data ?? [];
			Log.debug(this.accounts);
		});

		SignalrService.getPlexAccountRefreshProgress().subscribe((data) => {
			Log.debug(data);
		});
	}
}
</script>
