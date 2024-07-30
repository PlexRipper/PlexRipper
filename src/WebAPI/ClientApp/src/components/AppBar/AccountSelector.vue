<template>
	<q-btn-dropdown
		stretch
		flat
		icon="mdi-account"
		dropdown-icon="mdi-arrow-down"
	>
		<q-list v-if="accountsDisplay.length > 0">
			<q-item-label header>
				{{ $t('components.account-selector.title') }}
			</q-item-label>

			<!--  Account Row  -->
			<q-item
				v-for="(account, index) in accountsDisplay"
				:key="index"
				v-close-popup
				clickable
				tabindex="0"
				@click="updateActiveAccountId(account.id)"
			>
				<q-item-section>
					<q-item-label>{{ account.displayName }}</q-item-label>
					<q-item-label caption>
						{{ account.username }}
					</q-item-label>
				</q-item-section>
				<q-item-section side>
					<q-btn
						flat
						icon="mdi-refresh"
						:loading="loading[0] || loading[index]"
						:disabled="isLoading"
						@click.stop="runReSyncAccount(account.id)"
					/>
				</q-item-section>
			</q-item>
		</q-list>
		<!--	No account found -->
		<q-list v-else>
			<q-item-label> {{ t('components.app-bar.no-accounts') }}</q-item-label>
		</q-list>
	</q-btn-dropdown>
</template>

<script setup lang="ts">
import { get } from '@vueuse/core';
import { useSettingsStore, useAccountStore } from '~/store';
import { useSubscription } from '#imports';

const { t } = useI18n();
const settingsStore = useSettingsStore();
const accountStore = useAccountStore();

const loading = ref<Record<number, boolean>>({ 0: false });
const isLoading = computed(() => Object.values(get(loading)).some((x) => x));

const accountsDisplay = computed(() => {
	return [
		{
			id: 0,
			displayName: t('components.account-selector.all-accounts'),
			loading: get(loading)[0],
		},
		...accountStore.accounts
			.filter((x) => x.isEnabled)
			.map((x) => {
				return {
					id: x.id,
					displayName: x.displayName,
					loading: get(loading)[x.id] ?? false,
				};
			}),
	];
});

function updateActiveAccountId(accountId: number): void {
	settingsStore.generalSettings.activeAccountId = accountId;
}

function runReSyncAccount(accountId = 0): void {
	get(loading)[accountId] = true;
	useSubscription(
		accountStore.reSyncAccount(accountId).subscribe(() => {
			get(loading)[accountId] = false;
		}),
	);
}
</script>
