<template>
	<q-header class="app-bar">
		<QRow no-wrap>
			<q-toolbar class="app-bar">
				<q-toolbar-title>
					<q-btn
						flat
						round
						dense
						icon="mdi-menu"
						class="q-mr-sm"
						@click.stop="showNavigationDrawer" />
					<q-btn
						to="/"
						flat
						class="q-pa-sm">
						<div
							class="row items-center no-wrap"
							style="height: 2rem;">
							<img
								src="/img/logo/reaparr-full.svg"
								alt="Reaparr"
								style="height: 125%; width: auto;">
							<img
								src="/img/logo/reaparr-title.svg"
								alt="Reaparr"
								style="height: 100%; width: auto; margin-left: 0.5rem; margin-top: 3px;">
						</div>
					</q-btn>
					<q-btn
						flat
						round
						class="q-pa-none"
						@click="copy(globalStore.version)">
						<q-icon name="mdi-alpha-v-circle-outline" />
						<q-tooltip
							anchor="bottom middle"
							self="top middle"
							:offset="[10, 10]">
							{{ $t('components.app-bar.copy-version', { version: globalStore.version }) }}
						</q-tooltip>
					</q-btn>
				</q-toolbar-title>

				<q-btn
					icon="mdi-github"
					flat
					rounded
					style="padding: 0.5rem"
					href="https://github.com/Reaparr/Reaparr"
					target="_blank" />

				<q-btn
					flat
					rounded
					style="padding: 0.5rem"
					@click="dialogStore.openDialog(DialogType.DiscordServerInviteDialog)">
					<DiscordIcon />
				</q-btn>

				<!-- Background Activity Toggle -->
				<BackgroundActivityToggleButton />

				<!-- Account Selector -->
				<AccountSelector />

				<!-- Notifications Selector -->
				<NotificationButton @toggle="showNotificationsDrawer" />
			</q-toolbar>
		</QRow>
	</q-header>
</template>

<script setup lang="ts">
import { useGlobalStore, useDialogStore } from '@store';
import { DialogType } from '@enums';
import { useClipboard } from '@vueuse/core';

const globalStore = useGlobalStore();
const dialogStore = useDialogStore();

const { copy } = useClipboard({ legacy: true });

const emit = defineEmits<{
	(e: 'show-navigation' | 'show-notifications'): void;
}>();

function showNavigationDrawer(): void {
	emit('show-navigation');
}

function showNotificationsDrawer(): void {
	emit('show-notifications');
}
</script>

<style lang="scss">
@use '@/assets/scss/variables' as *;

body {
  &.body--dark {
    .app-bar {
      background: rgba(255, 0, 0, 0.2) !important;
    }
  }

  &.body--light {
    .app-bar {
      background: rgba(255, 0, 0, 1) !important;
    }
  }
}
</style>
