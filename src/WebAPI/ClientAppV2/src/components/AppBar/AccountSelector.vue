<template>
	<q-btn-dropdown stretch flat icon="mdi-account" dropdown-icon="mdi-arrow-down">
		<q-list v-if="accounts.length > 0">
			<q-item-label header>ACTIVE ACCOUNT</q-item-label>

			<!--  Account Row  -->
			<q-item
				v-for="(account, index) in accounts"
				:key="index"
				v-close-popup
				clickable
				tabindex="0"
				@click="updateActiveAccountId(account.id)"
			>
				<q-item-section>
					<q-item-label>{{ account.displayName }}</q-item-label>
					<q-item-label caption>{{ account.username }}</q-item-label>
				</q-item-section>
				<q-item-section side>
					<q-btn
						flat
						icon="mdi-refresh"
						:loading="loading[0] || loading[index]"
						:disabled="isLoading"
						@click.stop="runRefreshAccount(account.id)"
					>
					</q-btn>
				</q-item-section>
			</q-item>
		</q-list>
		<!--	No account found -->
		<q-list v-else>
			<q-item-label> {{ $t('components.app-bar.no-accounts') }}</q-item-label>
		</q-list>
	</q-btn-dropdown>
</template>

<script setup lang="ts">
import { useSubscription } from '@vueuse/rxjs';
import { AccountService, ServerService, SettingsService } from '@service';
import { refreshAccount } from '@api/accountApi';
import { PlexAccountDTO } from '@dto/mainApi';

const loading = ref<boolean[]>([false]);
const isLoading = computed(() => loading.value.some((x) => x));
const accounts = ref<PlexAccountDTO[]>([]);
const activeAccountId = ref(0);

function updateActiveAccountId(accountId: number): void {
	useSubscription(SettingsService.updateGeneralSettings('activeAccountId', accountId).subscribe());
}

function runRefreshAccount(accountId = 0): void {
	const index = accountId === 0 ? 0 : accounts.value.findIndex((x) => x.id === accountId);
	loading.value.splice(index, 1, true);
	refreshAccount(accountId).subscribe(() => {
		AccountService.refreshAccounts();
		ServerService.fetchServers();
		loading.value.splice(index, 1, false);
	});
}

onMounted(() => {
	useSubscription(
		AccountService.getAccounts().subscribe((data) => {
			accounts.value = [
				{
					id: 0,
					displayName: 'All Accounts',
				} as any,
			];
			data?.filter((x) => x.isEnabled).forEach((account) => accounts.value.push(account));
			accounts.value.forEach(() => loading.value.push(false));
		}),
	);

	useSubscription(
		SettingsService.getActiveAccountId().subscribe((newActiveAccountId) => {
			if (newActiveAccountId || newActiveAccountId >= 0) {
				activeAccountId.value = newActiveAccountId;
			}
		}),
	);
});
</script>
