<template>
	<q-row justify="center">
		<!-- Plex Accounts -->
		<q-col v-for="(account, index) in accounts" :key="index" class="q-pa-sm" cols="4" md="6" style="max-width: 395px" xs="12">
			<account-card :account="account" @open-dialog="openDialog(false, account)" />
		</q-col>
		<!-- Add new Account card -->
		<q-col class="q-pa-sm" cols="4" md="6" style="max-width: 395px" xs="12">
			<account-card @open-dialog="openDialog(true, null)" />
		</q-col>
	</q-row>
	<!-- Account Dialog -->
	<q-row>
		<q-col>
			<AccountDialog ref="accountDialog" @dialog-closed="closeDialog" />
		</q-col>
	</q-row>
</template>

<script setup lang="ts">
import Log from 'consola';
import { useSubscription } from '@vueuse/rxjs';
import { merge } from 'rxjs';
import { ref } from 'vue';
import { AccountService, LibraryService, ServerService } from '@service';
import { PlexAccountDTO } from '@dto/mainApi';
import { AccountDialog } from '#components';

const accounts = ref<PlexAccountDTO[]>([]);
const selectedAccount = ref<PlexAccountDTO | null>(null);
const isNewAccount = ref(false);
const accountDialog = ref<InstanceType<typeof AccountDialog> | null>(null);

const openDialog = (newAccount: boolean, account: PlexAccountDTO | null = null): void => {
	Log.info('Opening account dialog', accountDialog.value);
	isNewAccount.value = newAccount;
	selectedAccount.value = account;
	accountDialog.value?.openDialog(newAccount, account);
};

const closeDialog = (refreshAccounts = false): void => {
	if (refreshAccounts) {
		useSubscription(
			merge([
				AccountService.refreshAccounts(),
				ServerService.refreshPlexServers(),
				LibraryService.refreshLibraries(),
			]).subscribe(),
		);
	}
};

onMounted(() => {
	useSubscription(
		AccountService.getAccounts().subscribe((data) => {
			accounts.value = data ?? [];
		}),
	);
});
</script>
