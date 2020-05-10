<template>
	<v-container>
		<v-row>
			<!-- Plex Accounts -->
			<v-col v-for="(account, index) in accounts" :key="index" cols="3">
				<account-card :account="account" @dialog-closed="refreshAccounts()" />
			</v-col>
			<!-- Add new Account card -->
			<v-col cols="3">
				<account-card @dialog-closed="refreshAccounts()" />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import Log from 'consola';
import { Component, Vue } from 'vue-property-decorator';
import IAccount from '@dto/IAccount';
import AccountCard from './components/AccountCard.vue';

@Component({
	components: {
		AccountCard,
	},
})
export default class Settings extends Vue {
	accounts: IAccount[] = [];

	checkAccount(account: IAccount): void {
		Log.debug(account);
	}

	async refreshAccounts(): Promise<void> {
		try {
			this.accounts = await this.$axios.$get('/accounts');
			Log.debug(this.accounts);
		} catch (error) {
			Log.error(error);
		}
	}

	async mounted(): Promise<void> {
		await this.refreshAccounts();
	}
}
</script>
