<template>
	<QRow justify="center">
		<!-- Plex Accounts -->
		<QCol
			v-for="(account, index) in accountStore.getAccounts"
			:key="index"
			class="q-pa-sm"
			cols="4"
			md="6"
			style="max-width: 395px"
			xs="12"
		>
			<AccountCard
				:account="account"
				@open-dialog="openDialog(false, account)"
			/>
		</QCol>
		<!-- Add new Account card -->
		<QCol
			class="q-pa-sm"
			cols="4"
			md="6"
			style="max-width: 395px"
			xs="12"
		>
			<AccountCard @open-dialog="openDialog(true, null)" />
		</QCol>
	</QRow>
	<!-- Account Dialog -->
	<AccountDialog :name="accountDialogName" />
</template>

<script setup lang="ts">
import type { PlexAccountDTO } from '@dto';
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
