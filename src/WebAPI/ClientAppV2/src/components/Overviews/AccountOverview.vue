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
	<AccountDialog :name="accountDialogName" />
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { ref, onMounted } from 'vue';
import { set } from '@vueuse/core';
import { AccountService } from '@service';
import { PlexAccountDTO } from '@dto/mainApi';
import { useOpenControlDialog } from '@composables/event-bus';

const accounts = ref<PlexAccountDTO[]>([]);
const accountDialogName = 'accountDialogName';

function openDialog(isNewAccount: boolean, account: PlexAccountDTO | null = null): void {
	useOpenControlDialog(accountDialogName, {
		isNewAccountValue: isNewAccount,
		account,
	});
}

onMounted(() => {
	useSubscription(
		AccountService.getAccounts().subscribe((data) => {
			set(accounts, data ?? []);
		}),
	);
});
</script>
