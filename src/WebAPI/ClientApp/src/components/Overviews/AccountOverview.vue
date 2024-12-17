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
			xs="12">
			<AccountCard
				:account="account"
				@open-dialog="dialogStore.openAccountDialog({ accountId: account.id })" />
		</QCol>
		<!-- Add new Account card -->
		<QCol
			class="q-pa-sm"
			cols="4"
			md="6"
			style="max-width: 395px"
			xs="12">
			<AccountCard
				@open-dialog="dialogStore.openAccountDialog({ accountId: 0 })" />
		</QCol>
	</QRow>
	<!-- Account Dialog -->
	<AccountDialog />
</template>

<script setup lang="ts">
import { useAccountStore, useDialogStore } from '@store';

const accountStore = useAccountStore();
const dialogStore = useDialogStore();
</script>
