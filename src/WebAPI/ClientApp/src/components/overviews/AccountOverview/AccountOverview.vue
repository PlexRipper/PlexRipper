<template>
	<v-container>
		<v-row justify="center">
			<!-- Plex Accounts -->
			<v-col v-for="(account, index) in accounts" :key="index" cols="4" md="6" style="min-width: 395px" xs="12">
				<account-card :account="account" @open-dialog="openDialog(false, account)" @dialog-closed="dialog = false" />
			</v-col>
			<!-- Add new Account card -->
			<v-col cols="4" md="6" style="min-width: 395px" xs="12">
				<account-card @open-dialog="openDialog(true, null)" />
			</v-col>
		</v-row>
		<!-- Account Dialog -->
		<v-row>
			<v-col>
				<account-dialog ref="accountDialog" @dialog-closed="closeDialog" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Ref, Vue } from 'vue-property-decorator';
import { useSubscription } from '@vueuse/rxjs';
import { merge } from 'rxjs';
import { AccountService, LibraryService, ServerService } from '@service';
import { PlexAccountDTO } from '@dto/mainApi';
import AccountDialog from '@overviews/AccountOverview/AccountDialog.vue';

@Component<AccountOverview>({})
export default class AccountOverview extends Vue {
	@Ref('accountDialog')
	readonly accountDialogRef!: AccountDialog;

	private accounts: PlexAccountDTO[] = [];
	private selectedAccount: PlexAccountDTO | null = null;
	private newAccount: Boolean = false;

	openDialog(newAccount: boolean, account: PlexAccountDTO | null = null): void {
		this.newAccount = newAccount;
		this.selectedAccount = account;

		this.accountDialogRef.openDialog(newAccount, account);
	}

	closeDialog(refreshAccounts: boolean = false): void {
		if (refreshAccounts) {
			useSubscription(
				merge([
					AccountService.refreshAccounts(),
					ServerService.refreshPlexServers(),
					LibraryService.refreshLibraries(),
				]).subscribe(),
			);
		}
	}

	mounted(): void {
		useSubscription(
			AccountService.getAccounts().subscribe((data) => {
				this.accounts = data ?? [];
			}),
		);
	}
}
</script>
