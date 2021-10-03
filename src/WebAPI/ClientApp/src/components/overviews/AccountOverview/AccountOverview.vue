<template>
	<v-container>
		<v-row justify="center">
			<!-- Plex Accounts -->
			<v-col v-for="(account, index) in accounts" :key="index" xs="12" md="6" cols="4" style="min-width: 395px">
				<account-card :account="account" @open-dialog="openDialog(false, account)" @dialog-closed="dialog = false" />
			</v-col>
			<!-- Add new Account card -->
			<v-col xs="12" md="6" cols="4" style="min-width: 395px">
				<account-card @open-dialog="openDialog(true, null)" />
			</v-col>
		</v-row>
		<!-- Account Dialog -->
		<v-row>
			<v-col>
				<account-dialog :dialog="dialog" :new-account="newAccount" :account="selectedAccount" @dialog-closed="closeDialog" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { AccountService, LibraryService, ServerService } from '@service';
import { PlexAccountDTO } from '@dto/mainApi';

@Component<AccountOverview>({})
export default class AccountOverview extends Vue {
	private accounts: PlexAccountDTO[] = [];
	private dialog: boolean = false;
	private selectedAccount: PlexAccountDTO | null = null;

	private newAccount: Boolean = false;

	openDialog(newAccount: boolean, account: PlexAccountDTO | null = null): void {
		this.newAccount = newAccount;
		this.selectedAccount = account;
		this.dialog = true;
	}

	closeDialog(refreshAccounts: boolean = false): void {
		this.dialog = false;
		if (refreshAccounts) {
			AccountService.fetchAccounts();
			ServerService.fetchServers();
			LibraryService.fetchLibraries();
		}
	}

	mounted(): void {
		this.$subscribeTo(AccountService.getAccounts(), (data) => {
			this.accounts = data ?? [];
		});
	}
}
</script>
