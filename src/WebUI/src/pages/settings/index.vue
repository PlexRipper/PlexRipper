<template>
	<v-container>
		<v-row>
			<!-- Plex Accounts -->
			<v-col cols="3" v-for="(account, index) in accounts" :key="index">
				<account-card :account="account" />
			</v-col>
			<!-- Add new Account card -->
			<v-col cols="3">
				<account-card />
			</v-col>
		</v-row>
	</v-container>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import AccountCard from './components/AccountCard.vue';
import Log from 'consola';

interface IPlexAccounts {
	username: string;
	password: string;
	isConfirmed: boolean;
}

@Component({
	components: {
		AccountCard,
	},
})
export default class Settings extends Vue {
	accounts: IPlexAccounts[] = [];

	checkAccount(account: IPlexAccounts): void {
		Log.debug(account);
	}

	async mounted(): Promise<void> {
		try {
			this.accounts = await this.$axios.$get('/accounts');
			Log.debug(this.accounts);
		} catch (error) {
			Log.error(error);
		}

		if (this.accounts.length === 0) {
			this.accounts.push({
				username: '',
				password: '',
				isConfirmed: false,
			});
		}
	}
}
</script>
