<template>
	<v-container>
		<v-row>
			<!-- Plex Accounts -->
			<v-col v-for="(account, index) in accounts" :key="index" cols="4" style="min-width: 395px;">
				<account-card :account="account" @dialog-closed="refreshAccounts()" />
			</v-col>
			<!-- Add new Account card -->
			<v-col cols="4" style="min-width: 395px;">
				<account-card @dialog-closed="refreshAccounts()" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import AccountService from '@service/accountService';
import { PlexAccountDTO } from '@dto/mainApi';
import AccountCard from './components/AccountCard.vue';

@Component({
	components: {
		AccountCard,
	},
})
export default class Accounts extends Vue {
	private accounts: PlexAccountDTO[] = [];

	checkAccount(account: PlexAccountDTO): void {
		Log.debug(account);
	}

	refreshAccounts(): void {
		AccountService.fetchAccounts();
	}

	created(): void {
		AccountService.getAccounts().subscribe((data) => {
			this.accounts = data ?? [];
			Log.debug(this.accounts);
		});
	}
}
</script>
