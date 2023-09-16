<template>
	<q-row justify="center">
		<!-- Plex Accounts -->
		<q-col
			v-for="(account, index) in accountStore.getAccounts"
			:key="index"
			class="q-pa-sm"
			cols="4"
			md="6"
			style="max-width: 395px"
			xs="12">
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
import { PlexAccountDTO } from '@dto/mainApi';
import { useOpenControlDialog } from '@composables/event-bus';
import { useAccountStore } from '~/store';

const accountDialogName = 'accountDialogName';
const accountStore = useAccountStore();

function openDialog(isNewAccount: boolean, account: PlexAccountDTO | null = null): void {
	useOpenControlDialog(accountDialogName, {
		isNewAccountValue: isNewAccount,
		account,
	});
}
</script>
