<template>
	<v-container>
		<v-row>
			<!-- Plex Accounts -->
			<v-col v-for="(account, index) in getAccounts" :key="index" cols="3">
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
import IPlexAccount from '@dto/IPlexAccount';
import AccountCard from './components/AccountCard.vue';
import { UserStore } from '@/store/';

@Component({
	components: {
		AccountCard,
	},
})
export default class Settings extends Vue {
	checkAccount(account: IPlexAccount): void {
		Log.debug(account);
	}

	get getAccounts(): IPlexAccount[] {
		return UserStore.getAccounts;
	}

	async refreshAccounts(): Promise<void> {
		await UserStore.refreshAccounts();
	}
}
</script>
